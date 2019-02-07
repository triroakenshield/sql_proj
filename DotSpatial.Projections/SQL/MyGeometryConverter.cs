using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace DotSpatial.Projections
{
    public class MyGeometryConverter
    {

        SqlGeometry _geom;
        public SqlGeometry rgeom;
        SqlGeometryBuilder _builder;
        ProjectionInfo _sproj;
        ProjectionInfo _tproj;
        int point_count = 0;
        double[] _xy;
        double[] _z;
        double[] _m;
        //int _index;

        private void MakePointArray()
        {
            point_count = this._geom.STNumPoints().Value;
            this._xy = new double[point_count * 2];
            this._z = new double[point_count];
            this._m = new double[point_count];
            int step = 0;
            SqlGeometry p;
            for (int i = 1; i <= point_count; i++)
            {
                p = this._geom.STPointN(i);
                this._xy[step] = p.STX.Value;
                this._xy[step + 1] = p.STY.Value;
                if (p.HasZ) this._z[i] = p.Z.Value;
                if (p.HasM) this._m[i] = p.M.Value;
                step += 2;
            }
        }

        private void Reproject()
        {
            DotSpatial.Projections.Reproject.ReprojectPoints(_xy, _z, _sproj, _tproj, 0, point_count);
        }

        public MyGeometryConverter(SqlGeometry ngeom, string sproj, string tproj)
        {
            this._geom = ngeom;
            this._sproj = ProjectionInfo.FromProj4String(sproj);
            this._tproj = ProjectionInfo.FromProj4String(tproj);

            this.MakePointArray();
            this.Reproject();
            this._builder = new SqlGeometryBuilder();
            this._builder.SetSrid(0);
            this.rgeom = this.ReBuild();
        }

        public MyGeometryConverter(SqlGeometry ngeom, double[] koeff, bool r)
        {
            this._geom = ngeom;

            this.MakePointArray();
            if (r) this._xy = this.Affine(this._xy, koeff);
            else this._xy = this.ReAffine(this._xy, koeff);
            this._builder = new SqlGeometryBuilder();
            this._builder.SetSrid(0);
            this.rgeom = this.ReBuild();
        }

        public static SqlGeometry Reproject(SqlGeometry ngeom, string sproj, string tproj)
        {
            MyGeometryConverter conv = new MyGeometryConverter(ngeom, sproj, tproj);
            return conv.rgeom;
        }

        public static SqlGeometry Reproject(SqlGeometry ngeom, double[] koeff, bool r)
        {
            MyGeometryConverter conv = new MyGeometryConverter(ngeom, koeff, r);
            return conv.rgeom;
        }

        private void BuildLineString(SqlGeometry geom, int si)
        {
            SqlGeometry p = geom.STPointN(1);
            this._builder.BeginFigure(_xy[si * 2], _xy[si * 2 + 1], p.HasZ ? (double?)_z[si] : null, p.HasM ? (double?)_m[si] : null);
            int pc = geom.STNumPoints().Value;
            int k = 2 + si * 2;
            for (int i = 2; i <= pc; i++)
            {
                p = geom.STPointN(i);
                this._builder.AddLine(_xy[k], _xy[k + 1], p.HasZ ? (double?)_z[si + i - 1] : null, p.HasM ? (double?)_m[si + i - 1] : null);
                k += 2;
            }
            this._builder.EndFigure();
        }

        private void BuildCircularString(SqlGeometry geom, int si)
        {
            SqlGeometry p = geom.STPointN(1);
            SqlGeometry p1, p2;
            this._builder.BeginFigure(_xy[si * 2], _xy[si * 2 + 1], p.HasZ ? (double?)_z[si] : null, p.HasM ? (double?)_m[si] : null);
            int pc = geom.STNumPoints().Value;
            int k = 2 + si * 2;
            for (int i = 2; i <= pc; i += 2)
            {
                p1 = geom.STPointN(i);
                p2 = geom.STPointN(i + 1);
                this._builder.AddCircularArc(_xy[k], _xy[k + 1], p.HasZ ? (double?)_z[si + i - 1] : null, p.HasM ? (double?)_m[si + i - 1] : null,
                                             _xy[k + 2], _xy[k + 3], p.HasZ ? (double?)_z[si + i] : null, p.HasM ? (double?)_m[si + i] : null);
                k += 2;
            }
            this._builder.EndFigure();
        }

        private void BuildCompoundCurve(SqlGeometry geom, int si)
        {
            int cc = geom.STNumCurves().Value;
            int pc;
            SqlGeometry curve;
            SqlGeometry p, p1, p2;
            int k = 2 + si * 2;
            for (int ci = 1; ci <= cc; ci++)
            {
                curve = geom.STCurveN(ci);
                pc = curve.STNumPoints().Value;
                p = curve.STPointN(1);
                if (ci == 1) this._builder.BeginFigure(_xy[si * 2], _xy[si * 2 + 1], p.HasZ ? (double?)_z[si] : null, p.HasM ? (double?)_m[si] : null);
                if (curve.STGeometryType().Value == "CircularString")
                {
                    p1 = curve.STPointN(2);
                    p2 = curve.STPointN(3);
                    this._builder.AddCircularArc(_xy[k], _xy[k + 1], p.HasZ ? (double?)_z[si + ci] : null, p.HasM ? (double?)_m[si + ci] : null,
                                                 _xy[k + 2], _xy[k + 3], p.HasZ ? (double?)_z[si + ci + 1] : null, p.HasM ? (double?)_m[si + ci + 1] : null);
                }
                else
                {
                    p2 = curve.STPointN(2);
                    this._builder.AddLine(_xy[k], _xy[k + 1], p2.HasZ ? (double?)_z[ci] : null, p2.HasM ? (double?)_m[ci] : null);
                }
            }
            this._builder.EndFigure();
        }

        private void BuildFigure(SqlGeometry geom, int ssi)
        {
            int pc, si = ssi;
            OpenGisGeometryType gtype = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geom.STGeometryType().Value);
            switch (gtype)
            {
                case OpenGisGeometryType.Point:
                    this._builder.BeginFigure(_xy[si], _xy[si + 1], geom.HasZ ? (double?)_z[si] : null, geom.HasM ? (double?)_m[si] : null);
                    this._builder.EndFigure();
                    break;

                case OpenGisGeometryType.LineString:
                    BuildLineString(geom, si);
                    break;

                case OpenGisGeometryType.Polygon:
                    BuildLineString(geom.STExteriorRing(), si);
                    pc = geom.STNumInteriorRing().Value;
                    si += geom.STExteriorRing().STNumPoints().Value;
                    for (int i = 1; i <= pc; i++)
                    {
                        si += geom.STInteriorRingN(i).STNumPoints().Value;
                        BuildLineString(geom.STInteriorRingN(i), si);
                    }
                    break;

                case OpenGisGeometryType.CircularString:
                    BuildCircularString(geom, si);
                    break;

                case OpenGisGeometryType.CompoundCurve:
                    BuildCompoundCurve(geom, si);
                    break;

                case OpenGisGeometryType.CurvePolygon:
                    BuildFigure(geom.STExteriorRing(), si);
                    pc = geom.STNumInteriorRing().Value;
                    si += geom.STExteriorRing().STNumPoints().Value;
                    for (int i = 1; i <= pc; i++)
                    {
                        si += geom.STInteriorRingN(i).STNumPoints().Value;
                        BuildFigure(geom.STInteriorRingN(i), si);
                    }
                    break;

                default:
                    pc = geom.STNumGeometries().Value;
                    for (int gi = 1; gi <= pc; gi++)
                    {
                        BuildGeometry(geom.STGeometryN(gi), si);
                        si += geom.STGeometryN(gi).STNumPoints().Value;
                    }
                    break;
            }
        }

        private void BuildGeometry(SqlGeometry geom, int ssi)
        {
            int pc;
            OpenGisGeometryType gtype = (OpenGisGeometryType)Enum.Parse(typeof(OpenGisGeometryType), geom.STGeometryType().Value);
            if (!geom.STIsEmpty())
            {
                if (gtype == OpenGisGeometryType.GeometryCollection)
                {
                    this._builder.BeginGeometry(gtype);
                    pc = geom.STNumGeometries().Value;
                    for (int gi = 1; gi <= pc; gi++)
                    {
                        BuildGeometry(geom.STGeometryN(gi), ssi);
                        ssi += geom.STGeometryN(gi).STNumPoints().Value;
                    }
                    this._builder.EndGeometry();
                }
                else
                {
                    this._builder.BeginGeometry(gtype);
                    BuildFigure(geom, ssi);
                    this._builder.EndGeometry();
                }
            }
            else
            {
                this._builder.BeginGeometry(gtype);
                this._builder.EndGeometry();
            }
        }

        public SqlGeometry ReBuild()
        {
            BuildGeometry(this._geom, 0);
            return this._builder.ConstructedGeometry;
        }

        public double[] Affine(double[] points, double[] koeff)
        {
            double nx, ny;
            for (int i = 0; i < points.Length; i += 2)
            {
                nx = koeff[0] * points[i] + koeff[1] * points[i + 1] + koeff[2];
                ny = koeff[3] * points[i] + koeff[4] * points[i + 1] + koeff[5];
                points[i] = nx;
                points[i + 1] = ny;
            }
            return points;
        }

        public double[] ReAffine(double[] points, double[] koeff)
        {
            double k, kx, ky;
            k  = koeff[0] * koeff[4] - koeff[1] * koeff[3];
            kx = koeff[1] * koeff[5] - koeff[2] * koeff[4];
            ky = koeff[0] * koeff[5] - koeff[2] * koeff[3];

            for (int i = 0; i < points.Length; i += 2)
            {
                points[i] = (koeff[4] * points[i] - koeff[1] * points[i + 1] + kx) / k;
                points[i + 1] = -1 * (koeff[3] * points[i] - koeff[0] * points[i + 1] + ky) / k;
            }
            return points;
        }

    }
}

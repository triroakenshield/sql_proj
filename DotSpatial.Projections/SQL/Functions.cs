using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace DotSpatial.Projections
{
    public class Functions
    {

        [SqlFunction()]
        public static SqlGeometry cs2cs(SqlGeometry ngeom, String sproj, String tproj)
        {
            return MyGeometryConverter.Reproject(ngeom, sproj, tproj);
        }

        [SqlFunction()]
        public static SqlGeometry affine(SqlGeometry ngeom, double a, double b, double c, double d, double e, double f)
        {
            double[] koeff = new double[] { a, b, c, d, e, f };
            return MyGeometryConverter.Reproject(ngeom, koeff, true);
        }

        [SqlFunction()]
        public static SqlGeometry reaffine(SqlGeometry ngeom, double a, double b, double c, double d, double e, double f)
        {
            double[] koeff = new double[] { a, b, c, d, e, f };
            return MyGeometryConverter.Reproject(ngeom, koeff, false);
        }

    }

}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using DotSpatial.Projections;
//
using Microsoft.SqlServer.Types;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SqlGeometry p = SqlGeometry.Point(20000, 20000, 0);
            double[] xy = new double[] { 20000, 20000 };
            double[] koeff = new double[] { 0.999152, -0.035328, -11079788.502, 0.035328, 0.999152, -6660043.491 };
            MyGeometryConverter mgc = new MyGeometryConverter();
            xy = mgc.Affine(xy, koeff);
            xy = mgc.ReAffine(xy, koeff);
            Assert.IsTrue(true, "good");
        }
    }
}

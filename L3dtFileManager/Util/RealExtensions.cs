using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager
{
    public static class RealExtensions
    {
        public static bool EQ(this double a, double b, Epsilon e) { return e.IsEqual(a, b); }
        public static bool LE(this double a, double b, Epsilon e) { return e.IsEqual(a, b) || (a < b); }
        public static bool GE(this double a, double b, Epsilon e) { return e.IsEqual(a, b) || (a > b); }

        public static bool NE(this double a, double b, Epsilon e) { return e.IsNotEqual(a, b); }
        public static bool LT(this double a, double b, Epsilon e) { return e.IsNotEqual(a, b) && (a < b); }
        public static bool GT(this double a, double b, Epsilon e) { return e.IsNotEqual(a, b) && (a > b); }
    }
}
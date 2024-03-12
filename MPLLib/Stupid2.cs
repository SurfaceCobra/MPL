using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib
{

    internal class Stupid2
    {
        internal static void StupidTest()
        {
            P0 c0 = () => 88848;
            P1 c1 = () => c0;
            P2 c2 = () => c1;
            P3 c3 = () => c2;
            P4 c4 = () => c3;
            P5 c5 = () => c4;
            P6 c6 = () => c5;
            P7 c7 = () => c6;
            P8 c8 = () => c7;
            P9 c9 = () => c8;
            c0();
            c1()();
            c2()()();
            c3()()()();
            c4()()()()();
            c5()()()()()();
            c6()()()()()()();
            c7()()()()()()()();
            c8()()()()()()()()();
            c9()()()()()()()()()();


            B0[] t0 = { () => 1557 };
            B1[] t1 = { () => t0 };
            B2[] t2 = { () => t1 };
            B3[] t3 = { () => t2 };
            B4[] t4 = { () => t3 };
            B5[] t5 = { () => t4 };
            t0[0]();
            t1[0]()[0]();
            t2[0]()[0]()[0]();
            t3[0]()[0]()[0]()[0]();
            t4[0]()[0]()[0]()[0]()[0]();
            t5[0]()[0]()[0]()[0]()[0]()[0]();
        }

        internal delegate int B0();
        internal delegate B0[] B1();
        internal delegate B1[] B2();
        internal delegate B2[] B3();
        internal delegate B3[] B4();
        internal delegate B4[] B5();

        internal delegate int P0();
        internal delegate P0 P1();
        internal delegate P1 P2();
        internal delegate P2 P3();
        internal delegate P3 P4();
        internal delegate P4 P5();
        internal delegate P5 P6();
        internal delegate P6 P7();
        internal delegate P7 P8();
        internal delegate P8 P9();

        //internal static P0 _(int i) => (x) => x;

        public enum Work
        {
            SetCacheFromMemory,
            SetMemoryFromCache,
            AddCacheFromMemory,
            AddMemoryFromCache,
        }

    }
}

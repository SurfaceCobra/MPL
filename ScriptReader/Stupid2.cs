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

           // _(1)(2);
        }

        //internal static P0 _(int i) => (x) => x;

        internal delegate List<int> P0(int i);
        internal delegate P0 P1(int i);
        internal delegate P1 P2(int i);
        internal delegate P2 P3(int i);

        public enum Work
        {
            SetCacheFromMemory,
            SetMemoryFromCache,
            AddCacheFromMemory,
            AddMemoryFromCache,
        }

    }
}

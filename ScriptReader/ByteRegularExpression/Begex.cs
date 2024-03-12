using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.ByteRegularExpression
{
    public partial class Begex
    {
        internal MatchGroup group { get; init; }
    }

    internal class MatchGroup : IBegexArg
    {

        internal IMatchOption[] options { get; init; }
        internal IBegexArg[] args { get; init; }

        public MatchGroup(IMatchOption[] options, IBegexArg[] args)
        {
            this.options = options;
            this.args = args;
        }
    }

    internal class MatchOrByte : IBegexArg
    {
        internal bool[] matchBytes { get; init; }
        public MatchOrByte(params byte[] matchBytes)
        {
            this.matchBytes = Enumerable.Repeat(false, 256).ToArray();
            foreach (byte b in matchBytes)
                this.matchBytes[b] = true;
        }
        public MatchOrByte(bool[] matchBytes)
        {
            this.matchBytes = matchBytes;
        }
    }

    internal interface IBegexArg
    {

    }

    internal interface IMatchOption
    {
        internal class CountOf : IMatchOption
        {
            internal CountOf(Range countRange)
            {
                this.countRange = countRange;
            }
            
            internal Range countRange { get; init; }
        }


        internal class Exclude : IMatchOption
        {
            internal Exclude() { }
        }
        internal class Swap : IMatchOption
        {
            internal Swap(byte[] swapByte)
            {
                this.swapByte = swapByte;
            }

            internal byte[] swapByte { get; init; }
        }
        internal class Var : IMatchOption
        {
            internal Var(string varName)
            {
                this.varName = varName;
            }

            internal string varName { get; init; }
        }
    }
}

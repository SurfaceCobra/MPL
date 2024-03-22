using MPLLib.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.ByteRegularExpression
{
    public partial class Begex
    {
        public void Compile()
        {
            foreach(var item in this.group.innerMatchGroups)
            {

            }
        }
    }
    internal partial class MatchGroup : ICompileArg
    {
        public string Compile(int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("""
                public static bool @_MATCHGROUP_MATCH()
                {
                """);

            foreach(InnerMatchGroup innerGroup in this.innerMatchGroups)
            {
                
            }

            sb.AppendLine("""
                }
                """);
            return sb.ToString();
        }
    }
    internal partial class InnerMatchGroup : IMethodCompileArg
    {
        public string Compile(int index, int methodIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("""
                public static bool @_INNERMATCHGROUP_MATCH()
                {
                """);

            foreach(IBegexArg arg in this.args) switch (arg)
                {

                    case MatchOrByte orByte:
                        sb.AppendLine(orByte.Compile(index++));
                        break;
                    case MatchGroup matchGroup:
                        throw new Exception();
                        break;
                    default:
                        throw new Exception();
                }

            sb.AppendLine("""
                return true;
                }
                """);
            return sb.ToString();
        }
    }
    internal partial class MatchOrByte : ICompileArg
    {
        public string Compile(int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("switch(src[{index}]){");
            for (byte i = 0x00; i < 0xFF; i++)
                sb.AppendLine($"case 0x{i.ToString("X2")}: {(matchBytes[i] ? "return false;" : "break;")}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    internal interface ICompileArg
    {
        public string Compile(int index);
    }
    internal interface IMethodCompileArg
    {
        public string Compile(int index, int methodIndex);
    }
}

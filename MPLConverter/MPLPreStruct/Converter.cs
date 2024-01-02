using MPLConverter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPL
{

    public class StringBlock
    {
        List<StringBuilder> stringBuilders = new List<StringBuilder>();

        public void Stack(StringBlock sb)
        {
            foreach (var b in sb.stringBuilders)
            {
                stringBuilders.Add(b.Insert(0, "    "));
            }
        }
        public void AppendLine(params string[] strs)
        {
            stringBuilders.Add(new StringBuilder().AppendJoin(' ', strs));
        }

        public override string ToString()
        {
            return new StringBuilder().AppendJoin(Environment.NewLine, stringBuilders).ToString();
        }
    }

    public static class Converter
    {
        

        public static string Convert(MPLPreStruct mplData)
        {
            return Converter.MPL(mplData).ToString();
        }

        static StringBlock MPL(MPLPreStruct mplData)
        {
            StringBlock sb = new StringBlock();

            foreach(var pair in mplData.Namespaces)
            {
                sb.AppendLine("namespace",pair.Key);
                sb.AppendLine("{");
                sb.Stack(Converter.Namespace(pair.Value));
                sb.AppendLine("}");
            }

            return sb;
        }

        static StringBlock Namespace(MPLPreStruct.Namespace nameSpace)
        {
            StringBlock sb = new StringBlock();

            foreach (var pair in nameSpace.Classes)
            {
                sb.AppendLine("class", pair.Key);
                sb.AppendLine("{");
                sb.Stack(Converter.Class(pair.Value));
                sb.AppendLine("}");
            }

            return sb;
        }

        static StringBlock Class(MPLPreStruct.Class Class)
        {
            StringBlock sb = new StringBlock();

            foreach(var pair in Class.Fields)
            {
                switch(pair.Value.o.type.o)
                {
                    case HolderType.Instance:

                        sb.AppendLine(pair.Value.o.objType.o, pair.Value.o.EncodeName(pair.Key), ";");

                        break;
                }

                
            }


            return sb;
        }
    }
}
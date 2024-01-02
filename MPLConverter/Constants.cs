using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLConverter
{
    internal static class KeyWord
    {
        internal const string Return = "return";
        internal const string New = "new";
        internal const string Class = "class";
        internal const string NameSpace = "namespace";
        internal const string Public = "public";
        internal const string Private = "private";
        internal const string Protected = "protected";
        internal const string Dynamic = "dynamic";
        internal const string Global = "global";
        internal const string Static = "static";

        internal const string Function = "function";
        internal const string Instance = "instance";
        internal const string CSFunction = "csfunction";
        internal const string Property = "property";
        internal const string Inline = "inline";
        internal const string CSInline = "csinline";
    }
    internal static class KeyWord2
    {
        internal const string equal = "=";
        internal const string add = "+";
    }

    public enum Secure
    {
        Public,
        Private,
        Protected
    }
    public enum Location
    {
        Null,
        None,
        Dynamic, //저장위치:객체, 접근위치:객체 - instance 사용
        Global, //저장위치:타입, 접근위치:객체 - function등 사용, 접근할때 접근한 객체를 전달하니까
        Static, //저장위치:타입, 접근위치:타입 - 스태틱
    }
    public enum HolderType
    {
        Null,
        None,
        Instance,
        Function,
        Property,
        CSFunction,
    }

    public enum OptionShape
    {
        Null,
        None,
        Small,
        Medium,
        Big,
    }

}

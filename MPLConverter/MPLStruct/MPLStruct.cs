using MPLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLConverter
{
    public class MPLStruct
    {




        public class Namespace
        {
            public Ranged<Secure> secure;
            public Dictionary<Ranged<string>, Ranged<Class>> classes;
        }


        public class Class
        {
            public Ranged<Secure> secure;
            public Ranged<Location> location;
            public Ranged<HolderType> holderType;

            public Dictionary<Ranged<string>, Ranged<IField>> fields;
        }

        public interface IField
        {
            public Secure secure { get;}
            public HolderType holderType { get;}
            public Class type { get;}


        }
        public class Instance : IField
        {
            public Secure secure { get; init; }
            public HolderType holderType { get; init; }
            public Class type { get; init; }
        }
        public class Function : IField
        {
            public Secure secure { get; init; }
            public HolderType holderType { get; init; }
            public Class type { get; init; }
        }
        public class Initializer : IField
        {
            public Secure secure { get; init; }
            public HolderType holderType { get; init; }
            public Class type { get; init; }
        }
        public class CSFunction : IField
        {
            public Secure secure { get; init; }
            public HolderType holderType => HolderType.CSFunction;
            public Class type { get; init; }
        }

        public interface Expr
        {

        }
    }
}

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using MPLConverter;
using MPLLib;
using MPLLib.Beauty;

namespace MPL
{
    public class MPLPreStruct
    {
        public interface IMPLPreStructElement
        {

        }


        public Dictionary<Ranged<string>, Ranged<Namespace>> Namespaces = new();


        public class Namespace :  IMPLPreStructElement
        {

            public Dictionary<Ranged<string>, Ranged<Class>> Classes = new();
        }
        public class Class :  IMPLPreStructElement
        {


            public Ranged<Secure> secure;
            public Dictionary<Ranged<string>, Ranged<Field>> Fields = new();




        }
        public class Field :  IMPLPreStructElement
        {

            public Ranged<Secure> secure;
            public Ranged<Location> location;
            public Ranged<HolderType> type;
            public Ranged<string> objType;
            public Expr data;

            



            public string EncodeName(string name)
            {

                switch (this.type.o)
                {
                    case HolderType.Instance:
                        return $"INSTANCE_{name}";
                    case HolderType.Function:
                        return $"FUNCTION_{name}";
                    case HolderType.Property:
                        return $"PROPERTY_{name}";
                    default: throw new System.Exception();
                }
            }



        }


        public class Expr
        {
            /// <summary>
            /// 미정
            /// </summary>
            public class Unknown : Expr
            {
                public Ranged<string> str1;

                public Unknown(Ranged<string> str1)
                {
                    this.str1 = str1;
                }
            }

            public class UnknownBlock : Expr
            {
                public Expr[] exprs1;

                public UnknownBlock(params Expr[] exprs1)
                {
                    this.exprs1 = exprs1;
                }
            }


            /// <summary>
            /// 함수를 호출하는 식
            /// </summary>
            public class CallFunction : Expr
            {
                public Selector func1;
                public Option opt;
            }
            /// <summary>
            /// 대입 식
            /// </summary>
            public class Assign : Expr
            {
                public Selector type1;
                public string name2;
            }
            /// <summary>
            /// 등호 식
            /// </summary>
            public class Equal : Expr
            {
                public Expr obj1;
                public Expr obj2;
            }
            /// <summary>
            /// 반환 식
            /// </summary>
            public class Return : Expr
            {
                public Expr obj1;

                public Return(Expr obj1)
                {
                    this.obj1 = obj1;
                }
            }
            /// <summary>
            /// 리터럴 문자열 식
            /// </summary>
            public class Literal : Expr
            {
                public string obj1;

                public Literal(string obj1)
                {
                    this.obj1 = obj1;
                }
            }
            /// <summary>
            /// 객체 생성 식
            /// </summary>
            public class New : Expr
            {
                public CallFunction obj1;

                public New(CallFunction obj1)
                {
                    this.obj1= obj1;
                }
            }
            /// <summary>
            /// 객체를 나타내는 식
            /// </summary>
            public class Object : Expr
            {
                public Selector obj1;

                public Object(Selector obj1)
                {
                    this.obj1 = obj1;
                }
            }
            /// <summary>
            /// 타입을 나타내는 식
            /// </summary>
            public class ObjectType : Expr
            {
                public Selector objType1;

                public ObjectType(Selector objType1)
                {
                    this.objType1 = objType1;
                }
            }

            /// <summary>
            /// 객체 정의 식
            /// </summary>
            public class Init : Expr
            {
                public ObjectType objType1;
                public Object obj;

                public Init(ObjectType objType1, Object obj)
                {
                    this.objType1 = objType1;
                    this.obj = obj;
                }
            }
            /// <summary>
            /// 선택자
            /// </summary>
            public class Selector : Expr
            {
                public Ranged<string> obj1;

                public Selector(Ranged<string> obj1)
                {
                    this.obj1 = obj1;
                }
            }



            /// <summary>
            /// 식 블럭
            /// </summary>
            public class Block : Expr
            {
                public Expr[] objs;

                public Block(Expr[] exprs)
                {
                    this.objs = exprs;
                }
                public Block() { }
            }

            /// <summary>
            /// (int a1, int a2)
            /// </summary>
            public class OptionDef : Expr
            {
                public Init[] inits;

                public OptionDef(Init[] inits)
                {
                    this.inits = inits;
                }
            }

            /// <summary>
            /// (a1, a2)
            /// </summary>
            public class Option : Expr
            {
                public Object[] objs;
            }
        }

    }
    

    public interface IMPLStruct
    {

    }

    public class MPLStruct : IMPLStruct
    {
        public class Namespace
        {
            public Dictionary<string, Class> Classes = new();
        }
        public interface IClass
        {
            //public bool GetFieldInfo(SelectorBox sb, Expr[] values);

            public Dictionary<string, Field> Fields { get; }

            public Dictionary<string, Class> Classes { get; }
        }
        public class Class : IClass
        {
            public Dictionary<string, Class> Classes { get; init; }

            public Dictionary<string, Field> Fields { get; init; }
        }


        public interface IInstance
        {

        }

        /// <summary>
        /// 동작하는 객체
        /// </summary>
        public class Instance : IInstance
        {

        }

        /// <summary>
        /// 이름이 적힌 파라미터
        /// int value1 같은것
        /// </summary>
        public class NamedParameter
        {
            public bool isIndexer = false;

        }
        public class SelectorBox
        {
            public SelectorBox(Expr.Selector selector)
            {
                this.selector = selector;
            }

            public string current => selector.obj1[index];
            public void MoveNext() => index++;

            Expr.Selector selector;
            int index = 0;
        }

        /// <summary>
        /// 함수 식의 최소단위
        /// </summary>
        public class Expr
        {

            /// <summary>
            /// 함수를 호출하는 식
            /// </summary>
            public class CallFunction : Expr
            {
                public Selector func1;
                public Expr[] obj2;
            }
            /// <summary>
            /// 대입 식
            /// </summary>
            public class Assign : Expr
            {
                public Selector type1;
                public string name2;
            }
            /// <summary>
            /// 등호 식
            /// </summary>
            public class Equal : Expr
            {
                public Expr obj1;
                public Expr obj2;
            }
            /// <summary>
            /// 반환 식
            /// </summary>
            public class Return : Expr
            {
                public Expr obj1;
            }
            /// <summary>
            /// 리터럴 문자열 식
            /// </summary>
            public class Literal : Expr
            {
                public string obj1;
            }
            /// <summary>
            /// 객체 생성 식
            /// </summary>
            public class New : Expr
            {
                public Selector obj1;
            }
            /// <summary>
            /// 객체를 나타내는 식
            /// </summary>
            public class Object : Expr
            {
                public Selector obj1;
            }
            /// <summary>
            /// 선택자
            /// </summary>
            public class Selector : Expr
            {
                public string[] obj1;
            }
            /// <summary>
            /// 식 블럭
            /// </summary>
            public class Block : Expr
            {
                public Expr[] obj1;
            }
        }
        /// <summary>
        /// 필드
        /// </summary>
        public class Field
        {
            public IClass innerType;
            public Location location;

            /// <summary>
            /// 객체 모양 필드
            /// </summary>
            public class Instance : Field
            {

            }
            public class Function : Field
            {
                public Class ReturnType;
                public NamedParameter Parameter;
                public Expr Body;
            }
            /// <summary>
            /// 필드의 저장, 접근 위치
            /// Dynamic - 접근과 저장 모두 객체
            /// Global - 접근은 객체, 저장은 클래스
            /// Static - 접근과 저장 모두 클래스
            /// </summary>
            public enum Location
            {
                Static,
                Global,
                Dynamic
            }
            //필드에 접근할때, 접근할때 사용한 클래스 또는 객체가 같이 전달됨
            //함수의 경우에는 Global인데, 접근한 객체를 전달해야 하고 함수 자체는 클래스에 저장되있기 때문
        }
    }

    public class MPLStructDebug : IMPLStruct
    {

    }
}
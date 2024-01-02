using Microsoft.VisualBasic.FileIO;
using MPL;
using MPLLib;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using static MPL.MPLPreStruct;
using static MPL.MPLPreStruct.Expr;

namespace MPLConverter
{
    public static class Builder
    {
        enum StateBuild
        {
            None,
            NamespaceName,
            NamespaceBody
        }

        public static MPLPreStruct Build(ScriptBlock.Block src)
        {
            StateBuild state = StateBuild.None;
            Ranged<String> namespacename = null;
            MPLPreStruct mplPS = new MPLPreStruct();


            foreach (var v in src)
            {
                switch (v)
                {
                    case ScriptBlock.Value str:
                        switch (state, str.value.o)
                        {
                            case (StateBuild.None, "namespace"):
                                state = StateBuild.NamespaceName;
                                break;

                            case (StateBuild.NamespaceName, _):
                                namespacename = str.value;
                                state = StateBuild.NamespaceBody;
                                break;



                            default: throw new Exception();
                        }
                        break;
                    case ScriptBlock.Block block:
                        switch(state)
                        {
                            case StateBuild.NamespaceBody:
                                var ns = BuildNamespace(block);

                                mplPS.Namespaces.Add(namespacename, new Ranged<Namespace>(ns, block.range));
                                namespacename = null;

                                state = StateBuild.None;
                                break;
                            default:throw new Exception();
                        }
                        break;
                }

            }
            return mplPS;
        }
        enum StateNamespace
        {
            GrabOptions, //class 나오기전까지 public같은거 수집
            ClassName,
            HeritageCheck,
            ClassBody
        }

        private static Namespace BuildNamespace(ScriptBlock.Block src)
        {
            StateNamespace state = StateNamespace.GrabOptions;

            Ranged<Secure> secure = new(Secure.Private, 0..0);
            bool isSecureStateChanged = false;

            Ranged<String> classname = null;

            Namespace mplPN = new Namespace();


            foreach (var v in src)
            {
                switch(v)
                {
                    case ScriptBlock.Value str:
                        switch (state, str.value.o)
                        {
                            case (StateNamespace.GrabOptions, "class"):
                                state = StateNamespace.ClassName;
                                isSecureStateChanged = false;
                                break;

                            case (StateNamespace.GrabOptions, "public"):
                                if (isSecureStateChanged) throw new Exception();
                                secure = str.value.Convert(Secure.Public);
                                isSecureStateChanged = true;
                                break;

                            case (StateNamespace.GrabOptions, "private"):
                                if (isSecureStateChanged) throw new Exception();
                                secure = str.value.Convert(Secure.Private);
                                isSecureStateChanged = true;
                                break;

                            case (StateNamespace.GrabOptions, "protected"):
                                if (isSecureStateChanged) throw new Exception();
                                secure = str.value.Convert(Secure.Protected);
                                isSecureStateChanged = true;
                                break;


                            case (StateNamespace.ClassName, var name):
                                classname = str.value;
                                state = StateNamespace.HeritageCheck;
                                break;

                            case (StateNamespace.HeritageCheck, "="):
                                state = StateNamespace.ClassBody;
                                break;

                            case (StateNamespace.HeritageCheck, _):
                                throw new Exception();
                                break;

                            default: throw new Exception();
                        }
                        break;
                    case ScriptBlock.Block block:
                        switch(state)
                        {
                            case StateNamespace.ClassBody:

                                mplPN.Classes.Add(classname, BuildClassWith(block, secure));
                                state = StateNamespace.GrabOptions;
                                break;
                        }
                        break;
                    default:
                        throw new Exception();
                }


            }
            return mplPN;
        }



        enum StateClass
        {
            GrabOptionsorThingsorEnd,
        }

        private static Ranged<Class> BuildClassWith(ScriptBlock.Block src, Ranged<Secure> secure)
        {
            Ranged<Class> c = new(BuildClass(src),src.range);
            c.o.secure = secure;
            return c;
        }

        private static Class BuildClass(ScriptBlock.Block src)
        {
            StateClass state = StateClass.GrabOptionsorThingsorEnd;

            Ranged<Secure> secure = new(Secure.Private, 0..0);

            bool isSecureStateChanged = false;

            Ranged<String> fieldname = null;

            Class mplCS = new Class();


            foreach (var v in src)
            {
                ScriptBlock.Value strimshi = v as ScriptBlock.Value;
                ScriptBlock.Block block = v as ScriptBlock.Block;
                var largefield= BuildField(block);
                mplCS.Fields.Add(new(largefield.name, block.range), largefield.field);
                
            }




            if (state == StateClass.GrabOptionsorThingsorEnd || state == StateClass.GrabOptionsorThingsorEnd)
            {
                return mplCS;
            }
            else
            {
                throw new Exception();
            }
        }


        enum StateField
        {
            GrabSecureorNext,
            GrabHolderTypeorNext,
            GrabObjType,
            GrabName,
            GrabOptionOrInitializeorEnd,

            GrabFuncInsertExpr,
            GrabInstanceInitializeorEnd,
            GrabInstanceInitializeExpr,
            End,

        }



        private static (string name,Ranged<Field> field) BuildField(ScriptBlock.Block src)
        {
            Field field = new Field();
            field.secure = new(Secure.Private, 0..0);

            StateField state = StateField.GrabSecureorNext;
            string name = null;

            ScriptBlock.Block exprblock = new ScriptBlock.Block(ScriptBlock.Bracket.Shape.EOL);
            Expr exprstartblock;

            foreach (var v in src)
            {


                switch(v)
                {
                    case ScriptBlock.Value str:
                        switch (state, str.value.o)
                        {


                            case (_, KeyWord.Public):
                                if (state > StateField.GrabSecureorNext)
                                    throw new Exception();

                                field.secure = new Ranged<Secure>(Secure.Public, str.range);
                                goto securefinish;


                            case (_, KeyWord.Private):
                                if (state > StateField.GrabSecureorNext)
                                    throw new Exception();

                                field.secure = new Ranged<Secure>(Secure.Private, str.range);
                                goto securefinish;


                            securefinish:
                                state = StateField.GrabHolderTypeorNext;
                                break;






                            case (_, KeyWord.Instance):
                                if (state > StateField.GrabObjType)
                                    throw new Exception();

                                field.type = new Ranged<HolderType>(HolderType.Instance, str.range);
                                goto holdertypefinish;


                            case (_, KeyWord.Function):
                                if (state > StateField.GrabObjType)
                                    throw new Exception();

                                field.type = new Ranged<HolderType>(HolderType.Function, str.range);
                                goto holdertypefinish;

                            holdertypefinish:
                                state = StateField.GrabObjType;
                                break;







                            case (StateField.GrabObjType, _):
                                field.objType = str.value;
                                goto objtypefinish;

                            objtypefinish:
                                state = StateField.GrabName;
                                break;



                            case (StateField.GrabName, _):
                                name = str.value;
                                state = StateField.GrabOptionOrInitializeorEnd;
                                break;


                            case (StateField.GrabOptionOrInitializeorEnd, _):

                                break;


                            case (StateField.GrabInstanceInitializeorEnd, _):

                                if (str.value != "=")
                                    throw new Exception();


                                state = StateField.GrabInstanceInitializeExpr;

                                break;


                            case (StateField.GrabInstanceInitializeExpr, _):
                                exprblock.Add(new ScriptBlock.Value(str.value));
                                break;

                        }
                        break;
                    case ScriptBlock.Block block:
                        switch (state)
                        {
                            case StateField.GrabInstanceInitializeExpr:
                                exprblock.Add(block);
                                break;

                            case StateField.GrabFuncInsertExpr:
                                exprstartblock = BuildExpr(block);
                                break;
                        }
                        break;
                    default:
                        throw new Exception();
                        break;
                }

            }

            if(state == StateField.GrabInstanceInitializeorEnd)
            {
                return new(name, new(field,src.range));
            }
            if(state == StateField.GrabInstanceInitializeExpr)
            {
                field.data = BuildExpr(exprblock);
                return new(name, new(field, src.range));
            }
            if(state == StateField.GrabFuncInsertExpr) // true면 사실 안되는 코드
            {
                field.data = BuildExpr(exprblock);
                return new(name, new(field, src.range));
            }
            throw new Exception();
            return (new Ranged<string>("imshi",0..0),new Ranged<Field>(field,0..0));
        }


        enum StateExpr
        {
            GrabStart,
            GrabSingle,
            GrabDouble,
            GrabTriple,


            GetInit,
            GetType,
            GetObject,
        }


        private static Expr BuildExpr(ScriptBlock script)
        {
            switch (script)
            {
                case ScriptBlock.Value value:
                    return new Expr.Unknown(value.value);
                case ScriptBlock.Block block:
                    List<Expr> list = new List<Expr>();
                    foreach (var val in block)
                    {
                        list.Add(BuildExpr(val));
                    }
                    return new Expr.UnknownBlock(list.ToArray());
                default: throw new Exception();
            }
        }








        /*
        private static Expr BuildExpr(ScriptBlock.Block src) => BuildExpr(BlockForeachDude(src).GetEnumerator());
        private static Expr BuildExpr(IEnumerator<BlockOutput> e, StateExpr state = StateExpr.GrabStart)
        {
            List<Expr> stacks = new List<Expr>();

            List<Ranged<string>> strstacks = new List<Ranged<string>>();

            while (e.MoveNext())
            {
                var (isBlock, str, block) = e.Current;


                

                if (isBlock)
                    switch (state, str.value.o)
                    {

                        case (StateExpr.GrabStart, KeyWord.Return):
                            if (stacks.Count > 0)
                                throw new Exception();
                            return new Expr.Return(BuildExpr(e));

                        case (StateExpr.GrabStart, _):
                            stacks.Add(new Expr.Unknown(str.value));
                            break;

                        case (StateExpr.GrabSingle, _):

                            break;
                    }
                else
                {
                    stacks.Add(BuildExpr(block));
                }
            }

            if (stacks.Count == 0)
                return new Expr.Block();
            else if (stacks.Count == 1)
                return stacks[0];
            else
                return new Expr.Block(stacks.ToArray());


        }
        private static void BuildExpr2()
        {




        Start:


        Tree1:;



        }

        private delegate ExprSender Mydelegate(ExprSender sender);
        private ExprSender TreeStart(ExprSender sender)
        {
            if (sender.stacks.Count > 0)
                throw new Exception();


            if(sender.e.MoveNext())
            {
                var curr = sender.e.Current;
                if(curr.isBlock)
                {

                }
                else
                {
                    switch(curr.str.value)
                    {
                        case KeyWord.Return:
                            break;

                        case KeyWord.New:
                            break;
                    }
                }
            }
        }

        private static ExprSender TreeReturn(ExprSender sender)
        {
            if (sender.e.MoveNext())
            {
                var curr = sender.e.Current;
                if (curr.isBlock)
                {

                }
                else
                {

                }
            }
            else
            {
                //return void;
                throw new NotImplementedException();
            }
        }


        private static ExprSender TreeSelector(ExprSender sender)
        {
            if (sender.e.MoveNext())
            {
                var curr = sender.e.Current;
                if (curr.isBlock)
                {
                    //TreeSelector();
                }
                else
                {

                }
            }
            else
            {
                throw new NotImplementedException();
            }


            int a = 1;
            a.CompareTo(1);
        }

        private static ExprSender TreeFunctionOption()
        {
            int a;
        }
        */



        /// <summary>
        /// obsolite under here
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>



        private static Expr ReadExpr(ScriptBlock block) => throw new Exception();
        private static Expr ReadExpr(ExprSender sender)
        {
            while (sender.e.MoveNext())
            {
                var v = sender.e.Current;
                switch (v)
                {
                    case ScriptBlock.Value str:
                        switch (sender.skips)
                        {
                            case 0:
                                if (str.value == KeyWord.Return)
                                {
                                    sender.finStacks.Add(new Expr.Return(ReadObject(sender)));
                                }
                                else if (str.value == KeyWord.New)
                                {
                                    return new Expr.New(ReadFunction(sender));
                                }
                                else if (false/*isDot*/)
                                {

                                }
                                else if (str.value == KeyWord2.equal)
                                {
                                    throw new Exception("skip 0 equal error");
                                }
                                else if (str.value == "#EOL#")
                                {
                                    return null;
                                }
                                else if (IsLiteralExpression(str.value))
                                {
                                    return new Expr.Literal(str.value);
                                }
                                else if (true)
                                {
                                    sender.stacks.Add(str);
                                    return ReadExpr(sender);
                                }
                                else throw new Exception("wat");
                                break;
                            case 1:
                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                            default: throw new Exception(sender.skips + " det?");
                        }
                        break;
                    case ScriptBlock.Block block:
                        return ReadExpr(block);
                        break;
                    default:
                        throw new Exception();
                }

            }

            return null;// umjunsick;
        }

        private static bool IsLiteralExpression(string str)
        {
            return str.All(v=>Char.IsDigit(v));
        }

        private static Expr ReadObject(ExprSender sender)
        {
            if (sender.e.MoveNext()) ;

            var v = sender.e.Current;
            switch (v)
            {
                case ScriptBlock.Value str:
                    return new Expr.Selector(str.value);
                case ScriptBlock.Block block:
                    return ReadExpr(block);
                default:
                    throw new Exception();
            }
        }

        private static Expr.CallFunction ReadFunction(ExprSender sender)
        {
            throw new Exception();
        }




        enum StateExpr2
        {

        }
        class ExprSender
        {
            internal IEnumerator<ScriptBlock> e;
            internal List<ScriptBlock> stacks = new List<ScriptBlock>();
            internal StateExpr2 state;
            internal int skips => this.stacks.Count;

            internal List<Expr> finStacks = new List<Expr>();
        }




    
    }
}
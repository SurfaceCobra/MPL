using MPLLib.DataExecute;
using MPLLib.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.ByteRegularExpression
{
    public partial class Begex
    {
        static WordInfo[] infos { get; } = new WordInfo[] {
                (WordStatus.UsedWord, ".."),
                (WordStatus.UsedWord, "c"),
                (WordStatus.UsedWord, "x"),
                (WordStatus.UsedWord, "s"),
                (WordStatus.UsedWord, "h"),
            };

        public Begex(string begex)
        {
            
            ScriptReader reader = new ScriptReader(begex, infos);


            ScriptBlock.Block block = ScriptBlock.Create(reader);
            group = ConvertBlock(block);
        }


        MatchGroup ConvertBlock(ScriptBlock.Block block)
        {
            MatchGroup group;

            List<IMatchOption> options = new List<IMatchOption>();
            List<IBegexArg> args = new List<IBegexArg>();

            if (block.shape == Bracket.Shape.EOF) ;



            var flag = ParseResultType.Byte | ParseResultType.ByteArray | ParseResultType.BlockSmall | ParseResultType.BlockMedium | ParseResultType.BlockBig | ParseResultType.BlockComp;
            var e = block.GetCachedEnumerator(5);
            while (TryParseAnything(e, flag, out ParseResult result)) switch (result)
                {
                    case ParseResult.ByteResult br:
                        args.Add(new MatchOrByte(br.result));
                        break;
                    case ParseResult.BlockBigResult bbr:
                        args.Add(ConvertOrByte(bbr.result));
                        break;


                    case ParseResult.ByteArrayResult bar:
                        foreach (var v in bar.result)
                            args.Add(new MatchOrByte(v));
                        break;
                    case ParseResult.BlockSmallResult bsr:
                        args.Add(ConvertBlock(bsr.result));
                        break;

                    case ParseResult.BlockMediumResult bmr:
                        break;

                    case ParseResult.BlockCompResult bcr:
                        options.Add(ConvertMatchOption(bcr.result));
                        break;

                    default:
                        throw new Exception();
                }

            return new MatchGroup(options.ToArray(), args.ToArray());
        }


        IMatchOption ConvertMatchOption(ScriptBlock.Block block)
        {
            //개뻘짓 하는중 ScriptReader 업데이트 필요함
            

            var name = String.Concat(block.values.Select(x=> (x as ScriptBlock.Value)).TakeWhile(x => x.value.o != "=").Select(x=>x.value.o));
            var args = block.values.Select(x => (x as ScriptBlock.Value)).SkipWhile(x => x.value.o != "=").Skip(1);

            

            switch (name)
            {
                case "countof":
                    var newargs = string.Concat(args.Select(x => x.value.o)).Split(',');
                    return new IMatchOption.CountOf(int.Parse(newargs[0])..int.Parse(newargs[1]));

                case "exclude":
                    return new IMatchOption.Exclude();

                case "swap":
                    List<byte> byteList = new List<byte>();
                    var e = args.OfType<ScriptBlock>().GetCachedEnumerator(5); // 매우 불편한 코드 문제있음
                    while(TryParseAnything(e, ParseResultType.Byte|ParseResultType.ByteArray, out ParseResult result)) switch(result)
                    {
                            case ParseResult.ByteResult br:
                                byteList.Add(br.result);
                                break;
                            case ParseResult.ByteArrayResult bar:
                                byteList.AddRange(bar.result);
                                break;
                            default:
                                throw new Exception();
                    }


                    return new IMatchOption.Swap(byteList.ToArray());

                default:
                    throw new Exception();
            }
        }
        enum StateConvertMatchOption
        {
            ReadName,
            ReadEqual,
            ReadData,
            ReadShimPyoorEnd
        }

        
        MatchOrByte ConvertOrByte(ScriptBlock.Block block)
        {
            bool[] bools = Enumerable.Repeat(false, 256).ToArray();
            var e = block.GetCachedEnumerator(5);
            while (TryParseAnything(e, ParseResultType.Byte | ParseResultType.ByteRange, out var result)) switch (result)
            {
                case ParseResult.ByteResult br:
                    bools[br.result] = true;
                    break;
                case ParseResult.ByteRangeResult brr:
                    for (byte i = brr.result.Item1; i <= brr.result.Item2; i++)
                        bools[i] = true;
                    break;
                default:
                    throw new Exception();
            }

            return new MatchOrByte(bools);
        }

        static byte ConvertSingleChar(string s)
        {
            if (s.Length != 1) throw new Exception();
            return (byte)s[0];
        }
        static byte ConvertSingleByte(string s)
        {
            if (s.Length != 2) throw new Exception();
            return Convert.ToByte($"{s[0]}{s[1]}", 16);
        }
        static byte[] ConvertString(string s)
        {
            if (!(s.StartsWith("\"") && s.EndsWith("\"")))
                throw new Exception();
            return Encoding.UTF8.GetBytes(s[1..(s.Length-1)]);
        }
        static byte[] ConvertByteArray(string s)
        {
            if (s.Length % 2 != 0) throw new Exception();
            byte[] result = new byte[(s.Length - 2) / 2];
            for (int i = 0; i < s.Length - 2; i += 2)
            {
                result[i] = Convert.ToByte($"{s[i + 1]}{s[i + 2]}", 16);
            }
            return result;
        }


        bool TryParseAnything(ICachedEnumerator<ScriptBlock> e, ParseResultType flags, out ParseResult output)
        {
            output = ParseAnything(e, flags);
            return output is not ParseResult.EndResult;
        }
        ParseResult ParseAnything(ICachedEnumerator<ScriptBlock> e, ParseResultType flags)
        {
            if (e.TryMoveNext(out var arg))
            {
                if (arg is ScriptBlock.Value innerValue) switch (innerValue.value.o)
                    {
                        case "c" when flags.HasFlag(ParseResultType.Byte):
                            Func<string, byte> singleExecuteFunc = ConvertSingleChar;
                            goto SingleByteExecution;
                        case "x" when flags.HasFlag(ParseResultType.Byte):
                            singleExecuteFunc = ConvertSingleByte;
                            goto SingleByteExecution;
                        SingleByteExecution:
                            if (TryMoveNextValue(e, out string output))
                            {
                                byte byte1 = singleExecuteFunc(output);
                                //check byte range
                                if (!flags.HasFlag(ParseResultType.ByteRange))
                                {
                                    return new ParseResult.ByteResult(byte1);
                                }
                                else
                                {
                                    if (TryPeekValue(e, 2, out List<string> outList))
                                    {
                                        if (outList[1] == "-")
                                        {
                                            e.MoveNext();
                                            byte range2 = (ParseAnything(e, ParseResultType.Byte) as ParseResult.ByteResult).result;
                                            return new ParseResult.ByteRangeResult((Math.Min(byte1, range2), Math.Max(byte1, range2)));
                                        }
                                    }
                                    return new ParseResult.ByteResult(byte1);
                                }
                            }
                            throw new Exception();

                        case "s" when flags.HasFlag(ParseResultType.ByteArray):
                            Func<string, byte[]> arrayByteExecution = ConvertString;
                            goto ArrayByteExecution;
                        case "h" when flags.HasFlag(ParseResultType.ByteArray):
                            arrayByteExecution = ConvertByteArray;
                            goto ArrayByteExecution;
                        ArrayByteExecution:
                            if (TryMoveNextValue(e, out output))
                            {
                                return new ParseResult.ByteArrayResult(arrayByteExecution(output));
                            }
                            throw new Exception();




                        default: throw new Exception();
                    }


                else if (arg is ScriptBlock.Block innerBlock) switch (innerBlock.shape)
                    {
                        case Bracket.Shape.Small when flags.HasFlag(ParseResultType.BlockSmall):
                            return new ParseResult.BlockSmallResult(innerBlock);
                        case Bracket.Shape.Medium when flags.HasFlag(ParseResultType.BlockMedium):
                            return new ParseResult.BlockMediumResult(innerBlock);
                        case Bracket.Shape.Big when flags.HasFlag(ParseResultType.BlockBig):
                            return new ParseResult.BlockBigResult(innerBlock);
                        case Bracket.Shape.Comparer when flags.HasFlag(ParseResultType.BlockComp):
                            return new ParseResult.BlockCompResult(innerBlock);
                    }

                else throw new Exception();
            }
            else
            {
                return new ParseResult.EndResult();
            }
            throw new Exception();
        }


        bool TryPeekValue(ICachedEnumerator<ScriptBlock> e, int length, out List<string> output)
        {
            output = new List<string>(length);
            if (e.TryPeek(length, out var innerE))
            {
                foreach (var arg in innerE)
                    if (arg is ScriptBlock.Value innerValue)
                        output.Add(innerValue.value.o);
                    else return false;
                return true;
            }
            return false;
        }
        bool TryMoveNextValue(ICachedEnumerator<ScriptBlock> e, out string output)
        {
            if(e.TryMoveNext(out var innerE))
            {
                if(innerE is  ScriptBlock.Value innerValue)
                {
                    output = innerValue.value.o;
                    return true;
                }
                output = "";
                return false;
            }
            output = "";
            return false;
        }
    }

    [Flags]
    public enum ParseResultType : UInt64
    {
        None = 0,
        All = UInt64.MaxValue,
        End = 1<<0,
        Byte = 1 << 1,
        ByteArray = 1 << 2,
        ByteRange = 1 << 3,
        BlockSmall = 1 << 4,
        BlockMedium = 1 << 5,
        BlockBig = 1 << 6,
        BlockComp = 1 << 7,
        BlockDollar = 1 << 8,
    }
    internal interface ParseResult
    {
        public ParseResultType resultType {get;}
        public class EndResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.End;
            public EndResult() { }
        }
        public class ByteResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.Byte;
            public byte result;

            public ByteResult(byte result)
            {
                this.result = result;
            }
        }
        public class ByteArrayResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.ByteArray;
            public byte[] result;

            public ByteArrayResult(byte[] result)
            {
                this.result = result;
            }
        }
        public class ByteRangeResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.ByteRange;
            public (byte, byte) result;

            public ByteRangeResult((byte, byte) result)
            {
                this.result = result;
            }
        }
        public class BlockSmallResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.BlockSmall;
            public ScriptBlock.Block result;

            public BlockSmallResult(ScriptBlock.Block result)
            {
                this.result = result;
            }
        }
        public class BlockMediumResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.BlockMedium;
            public ScriptBlock.Block result;

            public BlockMediumResult(ScriptBlock.Block result)
            {
                this.result = result;
            }
        }
        public class BlockBigResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.BlockBig;
            public ScriptBlock.Block result;

            public BlockBigResult(ScriptBlock.Block result)
            {
                this.result = result;
            }
        }
        public class BlockCompResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.BlockComp;
            public ScriptBlock.Block result;

            public BlockCompResult(ScriptBlock.Block result)
            {
                this.result = result;
            }
        }
        public class BlockDollarResult : ParseResult
        {
            public ParseResultType resultType => ParseResultType.BlockDollar;
            public ScriptBlock.Block result;

            public BlockDollarResult(ScriptBlock.Block result)
            {
                this.result = result;
            }
        }
    }
}

using MPLLib.Beauty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib
{
    public interface ScriptBlock : IRangeInside
    {

        public class Block : ScriptBlock, IEnumerable<ScriptBlock>
        {
            public Block(Bracket.Shape shape)
            {
                this._shape = shape;
            }
            public void Add(ScriptBlock item)
            {

                if (item is Block block)
                {
                    //if (block.values.Count > 0)
                    values.Add(item);
                }
                else
                {
                    values.Add(item);
                }
            }
            public void ChangeShape(Bracket.Shape shape)
            {
                this._shape = shape;
            }

            public List<ScriptBlock> values = new List<ScriptBlock>();

            Bracket.Shape _shape;
            public Bracket.Shape shape => _shape;

            public Range range => Ranged.Merge(values.ConvertAll(x => x.range));

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                string s = Bracket.StringOf((shape, Bracket.Location.Open));
                if (s.Length > 0)
                {
                    sb.AppendLine();
                    sb.Append(s);
                    sb.AppendLine();
                }

                foreach (var b in values)
                {
                    switch (b)
                    {
                        case ScriptBlock.Block bb:
                            sb.Append(bb.ToString());
                            break;
                        case ScriptBlock.Value vb:
                            sb.Append(vb.ToString());
                            sb.Append(" ");
                            break;
                    }
                }
                sb.Append(Bracket.StringOf((shape, Bracket.Location.Close)));
                sb.AppendLine();
                return sb.ToString();
            }

            public IEnumerator<ScriptBlock> GetEnumerator() => this.values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
        public class Value : ScriptBlock
        {
            public Value(Ranged<string> value)
            {
                this.value = value;
            }

            public Ranged<string> value;

            public Range range => value.range;

            public Ranged<string> ToRangedString()
            {
                return this.value;
            }
            public override string ToString()
            {
                return this.value;
            }
        }

        public static ScriptBlock.Block Create(ScriptReader reader) => Fix(CreateUnFixed(Bracket.Shape.EOF, reader.Footer("\x00").GetEnumerator()));
        private static ScriptBlock.Block CreateUnFixed(Bracket.Shape opener, IEnumerator<Ranged<string>> reader)
        {
            ScriptBlock.Block block = new ScriptBlock.Block(opener);
            while (reader.MoveNext())
            {
                var str = reader.Current;
                bool IsOk;

                if (str.o.Length == 1)
                {
                    if (Bracket.Groups.TryGetValue(str.o[0], out (Bracket.Shape shape, Bracket.Location loc) val))
                    {
                        //특수 텍스트
                        switch (val.shape, val.loc)
                        {
                            case (Bracket.Shape.EOL, Bracket.Location.CloseOnly):
                            default:
                                IsOk = true;
                                break;
                            case (_, Bracket.Location.Open):
                                IsOk = false;
                                block.values.Add(CreateUnFixed(val.shape, reader));
                                break;

                            case (_, Bracket.Location.CloseOnly):
                            case (_, Bracket.Location.Close):
                                IsOk = false;
                                if (val.shape == opener)
                                    return block;
                                else throw new Exception("Bracket mismatch");
                        }

                    }
                    else
                    {
                        IsOk = true;
                    }
                }
                else
                {
                    IsOk = true;
                }
                if (IsOk)
                {
                    //일반 텍스트
                    block.values.Add(new ScriptBlock.Value(str));
                }


            }
            throw new Exception("File End");
        }

        private static ScriptBlock.Block Fix(ScriptBlock.Block srcblock)
        {
            List<int> slist = new List<int>();
            for (int i = 0; i < srcblock.values.Count; i++)
            {
                if (srcblock.values[i] is ScriptBlock.Block block)
                {
                    srcblock.values[i] = Fix(block);
                }


            }
            if (srcblock.values.Any(IsSemicolon))
            {
                var splitedinside = srcblock.values.Split(IsSemicolon).ToList();
                if (splitedinside.Last().Count == 0)
                {
                    splitedinside.RemoveAt(splitedinside.Count - 1);
                }
                srcblock.values.Clear();
                foreach (var inside in splitedinside)
                {
                    var v = new ScriptBlock.Block(Bracket.Shape.EOL);
                    v.values = inside;
                    srcblock.Add(v);
                }
            }

            return srcblock;


            bool IsSemicolon(ScriptBlock b) => b is ScriptBlock.Value imshi && imshi.value == ";";
        }

    }
    public static class Bracket
    {
        public enum Shape
        {
            Null,
            None,
            NoGap,
            Small,
            Medium,
            Big,
            Comparer,
            String,
            Char,
            EOL,
            EOF
        }

        public enum Location
        {
            Null,
            None,
            Open,
            Close,
            CloseOnly,
            Both,

        }

        public static Dictionary<char, (Shape, Location)> Groups = new()
        {
            ['('] = (Shape.Small, Location.Open),
            [')'] = (Shape.Small, Location.Close),
            ['{'] = (Shape.Medium, Location.Open),
            ['}'] = (Shape.Medium, Location.Close),
            ['['] = (Shape.Big, Location.Open),
            [']'] = (Shape.Big, Location.Close),
            ['<'] = (Shape.Comparer, Location.Open),
            ['>'] = (Shape.Comparer, Location.Close),
            [';'] = (Shape.EOL, Location.CloseOnly),
            ['"'] = (Shape.String, Location.Both),
            ['\''] = (Shape.Char, Location.Both),
            ['~'] = (Shape.NoGap, Location.Both),
            [(char)0x00] = (Shape.EOF, Location.CloseOnly),
        };
        public static string StringOf((Shape, Location) val)
        {
            switch (val.Item1)
            {
                case Shape.Small:
                    switch (val.Item2)
                    {
                        case Location.Open:
                            return "(";
                        case Location.Close:
                            return ")";
                        default:
                            throw new Exception();
                    }

                case Shape.Medium:
                    switch (val.Item2)
                    {
                        case Location.Open:
                            return "{";
                        case Location.Close:
                            return "}";
                        default:
                            throw new Exception();
                    }

                case Shape.Big:
                    switch (val.Item2)
                    {
                        case Location.Open:
                            return "[";
                        case Location.Close:
                            return "]";
                        default:
                            throw new Exception();
                    }

                case Shape.Comparer:
                    switch (val.Item2)
                    {
                        case Location.Open:
                            return "<";
                        case Location.Close:
                            return ">";
                        default:
                            throw new Exception();
                    }

                case Shape.EOL:
                    switch (val.Item2)
                    {
                        case Location.Open:
                            return "";
                        case Location.Close:
                            return "#CLS#";
                        case Location.CloseOnly:
                            return "#EOL#";
                        default:
                            throw new Exception();
                    }

                case Shape.String:
                    return "\"";

                case Shape.Char:
                    return "\'";

                case Shape.EOF:
                    switch (val.Item2)
                    {
                        case Location.Open:
                            return "";
                        case Location.Close:
                        case Location.CloseOnly:
                            return "#EOF#";
                        default:
                            throw new Exception();
                    }

                default:
                    return "#NULL#";
            }
        }
    }


}

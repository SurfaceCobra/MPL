using MPLLib.ExtensionMethod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MPLLib.DataExecute
{

    public class ScriptReader : IEnumerable<Ranged<string>>
    {
        private ICachedEnumerator<char> _data { get; set; }
        private ScriptReader()
        {

        }
        public ScriptReader(string str) : this(str.GetEnumerator()) { }
        public ScriptReader(IEnumerable<char> data) : this(data.GetEnumerator()) { }
        public ScriptReader(IEnumerable<char> data, IEnumerable<WordInfo> CustomInfo) : this(data.GetEnumerator(), CustomInfo) { }
        public ScriptReader(IEnumerator<char> data) : this(data, Enumerable.Empty<WordInfo>()) { }
        public ScriptReader(IEnumerator<char> data, IEnumerable<WordInfo> CustomInfo, bool RemoveBaseInfo = false)
        {

            List<WordInfo> FullInFo;

            if(RemoveBaseInfo)
                FullInFo = CustomInfo.ToList();
            else
                FullInFo = BaseInfoList.Concat(CustomInfo).ToList();
            


            int maxlength = FullInFo.Max((x) => x.word.Length);
            this._data = new CachedEnumerator<char>(data, maxlength);
            InfoList = new List<WordInfo>[maxlength];
            for (int i = 0; i < maxlength; i++)
                InfoList[i] = new List<WordInfo>();
            FullInFo.ForEach(x => InfoList[x.word.Length - 1].Add(x));

        }

        private IEnumerator<Ranged<string>> Reader(ReadPasser passer)
        {
            List<char> Q = new List<char>();
            while (passer.MoveNext())
            {
                WordInfo info = TryFindWord(passer.data.CachedValues);

                switch (info.status)
                {
                    case WordStatus.ImportantWord:
                    case WordStatus.UsedWord:
                    case WordStatus.IgnoreWord:
                        if (Q.Count > 0)
                        {
                            yield return new Ranged<string>(new string(Q.ToArray()), (passer.index - Q.Count)..passer.index);
                            Q.Clear();
                        }
                        break;
                }
                switch (info.status)
                {
                    case WordStatus.ImportantWord:
                        foreach (var v in info.reader(passer))
                        {
                            if (v.o.Length > 0)
                                yield return v;
                        }

                        break;
                    case WordStatus.UsedWord:
                        yield return new Ranged<string>(info.word, (passer.index - info.word.Length)..passer.index);
                        MM.Repeat(info.word.Length, passer.MoveNext);
                        passer.data.CachedValues.RemoveRange(0, info.word.Length - 1);
                        break;
                    case WordStatus.None:
                        Q.Add(passer.Current);
                        break;
                }
            }
            if (Q.Count > 0)
            {
                yield return new Ranged<string>(new string(Q.ToArray()), (passer.index - Q.Count)..passer.index);
            }
            yield break;
        }

        public delegate IEnumerable<Ranged<string>> CustomReader(ReadPasser passer);






        private WordInfo TryFindWord(List<char> charlist)
        {
            for (int i = charlist.Count; i > 0;)
            {
                var theVars = InfoList[--i].Where(x => charlist.Match(x.word, x.word.Length));
                switch (theVars.Count())
                {
                    case > 1:
                        throw new Exception("Multiple Syntax Crash Boom Why");
                    case 1:
                        return theVars.First();
                }
            }
            return (WordStatus.None, "", null);
        }

        public List<WordInfo>[] InfoList;

        public static List<WordInfo> BaseInfoList;

        static ScriptReader()
        {
            BaseInfoList = new List<WordInfo>()
            {
                (WordStatus.UsedWord, ".."),
                (WordStatus.UsedWord, "=>"),
                (WordStatus.UsedWord, "->"),
                (WordStatus.UsedWord, "~"),
                (WordStatus.UsedWord, "`"),
                (WordStatus.UsedWord, "!"),
                (WordStatus.UsedWord, "@"),
                (WordStatus.UsedWord, "#"),
                (WordStatus.UsedWord, "$"),
                (WordStatus.UsedWord, "%"),
                (WordStatus.UsedWord, "^"),
                (WordStatus.UsedWord, "&"),
                (WordStatus.UsedWord, "*"),
                (WordStatus.UsedWord, "("),
                (WordStatus.UsedWord, ")"),
                (WordStatus.UsedWord, "-"),
                (WordStatus.UsedWord, "_"),
                (WordStatus.UsedWord, "="),
                (WordStatus.UsedWord, "+"),
                (WordStatus.UsedWord, "["),
                (WordStatus.UsedWord, "{"),
                (WordStatus.UsedWord, "]"),
                (WordStatus.UsedWord, "}"),
                (WordStatus.UsedWord, "\\"),
                (WordStatus.UsedWord, "|"),
                (WordStatus.UsedWord, ";"),
                (WordStatus.UsedWord, ":"),
                (WordStatus.UsedWord, "'"),
                (WordStatus.UsedWord, ","),
                (WordStatus.UsedWord, "<"),
                (WordStatus.UsedWord, "."),
                (WordStatus.UsedWord, ">"),
                (WordStatus.UsedWord, "/"),
                (WordStatus.UsedWord, "?"),
                (WordStatus.IgnoreWord, " "),
                (WordStatus.IgnoreWord, "\r"),
                (WordStatus.IgnoreWord, "\n"),
                (WordStatus.IgnoreWord, "\t"),

                (WordStatus.ImportantWord, "\"", CreateReaderStringWithOptions(false, false)),
                (WordStatus.ImportantWord, "@\"", CreateReaderStringWithOptions(true, false)),
                (WordStatus.ImportantWord, "$\"", CreateReaderStringWithOptions(false, true)),
                (WordStatus.ImportantWord, "@$\"", CreateReaderStringWithOptions(true, true)),
                (WordStatus.ImportantWord, "$@\"", CreateReaderStringWithOptions(true, true)),

                (WordStatus.ImportantWord, "@\"", null),
                (WordStatus.ImportantWord, "$\"", null),
                (WordStatus.ImportantWord, "@$\"", null),
                (WordStatus.ImportantWord, "$@\"", null),

                (WordStatus.ImportantWord, "//", CreateReaderAnnotationUntil(Environment.NewLine)),
                (WordStatus.ImportantWord, "/*", CreateReaderAnnotationUntil("*/")),

            };
        }

        IEnumerator<Ranged<string>> IEnumerable<Ranged<string>>.GetEnumerator() => Reader((_data, 0));

        IEnumerator IEnumerable.GetEnumerator() => Reader((_data, 0));



        public static CustomReader CreateReaderAnnotationUntil(string until)
        {
            return (x) => PartialReaderAnnotation(x, until);
        }
        private static CustomReader CreateReaderStringWithOptions(bool isAt, bool isFormat)
        {
            return (x) => PartialReaderString(x, isAt, isFormat);
        }



        private static IEnumerable<Ranged<string>> PartialReaderAnnotation(ReadPasser passer, string ender)
        {
            List<char> Q = new List<char>();
            Q.Add(passer.data.Current);
            while (passer.MoveNext())
            {
                if (passer.data.CachedValues.Match(ender, ender.Length))
                {
                    ender.ForEach(Q.Add);
                    yield return new Ranged<string>(new string(Q.ToArray()), (passer.index - Q.Count)..passer.index);
                    yield break;
                }
                else
                {
                    Q.Add(passer.data.Current);
                }
            }
        end:
            throw new Exception("Unfinished Annotation");
        }

        private static IEnumerable<Ranged<string>> PartialReaderString(ReadPasser passer, bool isAt, bool isFormat)
        {
            List<char> Q = new List<char>();
            Q.Add(passer.data.Current);
            while (passer.MoveNext())
            {
                switch (passer.data.Current, isAt, isFormat)
                {
                    case ('"', _, _):
                        Q.Add(passer.data.Current);
                        yield return new Ranged<string>(new string(Q.ToArray()), (passer.index - Q.Count)..passer.index);
                        yield break;
                    case ('\\', false, _):
                        Q.Add(passer.data.Current);
                        if (!passer.MoveNext())
                            goto end;
                        Q.Add(passer.data.Current);
                        break;
                    case (_, _, true):
                        throw new NotImplementedException("not");
                    default:
                        Q.Add(passer.data.Current);
                        break;
                }
            }
        end:
            throw new Exception("Unfinished String");
        }
    }
    public enum WordStatus
    {
        None,
        ImportantWord,
        UsedWord,
        IgnoreWord,
    }
    public record struct WordInfo(WordStatus status, string word, ScriptReader.CustomReader? reader)
    {
        public static implicit operator WordInfo((WordStatus status, string word) value)
        {
            return new WordInfo(value.status, value.word, null);
        }


        public static implicit operator (WordStatus status, string word, ScriptReader.CustomReader? reader)(WordInfo value)
        {
            return (value.status, value.word, value.reader);
        }

        public static implicit operator WordInfo((WordStatus status, string word, ScriptReader.CustomReader? reader) value)
        {
            return new WordInfo(value.status, value.word, value.reader);
        }
    }

    public record struct ReadPasser(ICachedEnumerator<char> data, int index) : IEnumerator<char>
    {
        public char Current => data.Current;

        object IEnumerator.Current => data.Current;

        public void Dispose()
        {
            data.Dispose();
        }

        public bool MoveNext()
        {
            bool b = data.MoveNext();
            if (b) index++;
            return b;
        }

        public void Reset()
        {
            data.Reset();
            index = 0;
        }

        public static implicit operator (ICachedEnumerator<char> data, int index)(ReadPasser value)
        {
            return (value.data, value.index);
        }

        public static implicit operator ReadPasser((ICachedEnumerator<char> data, int index) value)
        {
            return new ReadPasser(value.data, value.index);
        }
    }

}

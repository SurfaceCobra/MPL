using MPLLib.Beauty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MPLLib
{
    public class ScriptReader2 : IEnumerable<Ranged<string>>
    {
        private ICachedEnumerator<char> _data { get; set; }
        private ScriptReader2()
        {

        }
        public ScriptReader2(string str) : this(str.GetEnumerator()) { }
        public ScriptReader2(IEnumerable<char> data) : this(data.GetEnumerator()) { }

        public ScriptReader2(IEnumerator<char> data) : this(data, Enumerable.Empty<WordInfo>()) { }
        public ScriptReader2(IEnumerator<char> data, IEnumerable<WordInfo> CustomInfo)
        {
            
            var FullInFo = BaseInfoList.Concat(CustomInfo);

            int maxlength = FullInFo.Max((x) => x.word.Length);
            this._data = new CachedEnumerator<char>(data, maxlength);
            InfoList = new List<WordInfo>[maxlength];
            for(int i=0;i<maxlength;i++)
                InfoList[i] = new List<WordInfo>();
            FullInFo.ForEach(x => InfoList[x.word.Length-1].Add(x));
            
        }

        private IEnumerator<Ranged<string>> Reader(ICachedEnumerator<char> data)
        {
            List<char> Q = new List<char>();
            int index;
            for(index=0;data.MoveNext();index++)
            {
                WordInfo info = TryFindWord(data.CachedValues);

                switch (info.status)
                {
                    case WordStatus.ImportantWord:
                    case WordStatus.UsedWord:
                    case WordStatus.IgnoreWord:
                        if (Q.Count > 0)
                        {
                            yield return new Ranged<string>(new string(Q.ToArray()), (index - Q.Count)..index);
                            Q.Clear();
                        }
                        break;
                }
                switch (info.status)
                {
                    case WordStatus.ImportantWord:

                        break;
                    case WordStatus.UsedWord:
                        yield return new Ranged<string>(info.word, (index-info.word.Length)..index);
                        index += info.word.Length;
                        MM.Repeat(info.word.Length, data.MoveNext);
                        data.CachedValues.RemoveRange(0, info.word.Length-1);
                        break;
                    case WordStatus.None:
                        Q.Add(data.Current);
                        break;
                }
            }
            if (Q.Count > 0)
            {
                yield return new Ranged<string>(new string(Q.ToArray()), (index - Q.Count)..index);
                Q.Clear();
            }
            yield break;
        }

        public delegate IEnumerator<Ranged<string>> CustomReader(ICachedEnumerator<char> data);



        public enum WordStatus
        {
            None,
            ImportantWord,
            UsedWord,
            IgnoreWord,
        }


        private WordInfo TryFindWord(List<char> charlist)
        {
            for(int i=charlist.Count; i>0;)
            {
                var theVars = InfoList[--i].Where(x => charlist.Match(x.word, x.word.Length));
                switch(theVars.Count())
                {
                    case > 1:
                        throw new Exception("Multiple Syntax Crash Boom Why");
                    case 1:
                        return theVars.First();
                }
            }
            return (WordStatus.None, "", null);
        }

        public Dictionary<string, CustomReader>[] ImportantWords { get; init; }
        public List<string>[] UsedWords { get; init; }
        public List<string>[] IgnoreWords { get; init; }


        public static List<WordInfo>[] InfoList;

        public static List<WordInfo> BaseInfoList;

        public static Dictionary<string, CustomReader> ImportantWordsBase;
        public static List<string> UsedWordsBase;
        public static List<string> IgnoreWordsBase;

        static ScriptReader2()
        {
            BaseInfoList = new List<WordInfo>()
            {
                (WordStatus.UsedWord, "..", null),
                (WordStatus.UsedWord, "=>", null),
                (WordStatus.UsedWord, "->", null),
                (WordStatus.UsedWord, "~", null),
                (WordStatus.UsedWord, "`", null),
                (WordStatus.UsedWord, "!", null),
                (WordStatus.UsedWord, "@", null),
                (WordStatus.UsedWord, "#", null),
                (WordStatus.UsedWord, "$", null),
                (WordStatus.UsedWord, "%", null),
                (WordStatus.UsedWord, "^", null),
                (WordStatus.UsedWord, "&", null),
                (WordStatus.UsedWord, "*", null),
                (WordStatus.UsedWord, "(", null),
                (WordStatus.UsedWord, ")", null),
                (WordStatus.UsedWord, "-", null),
                (WordStatus.UsedWord, "_", null),
                (WordStatus.UsedWord, "=", null),
                (WordStatus.UsedWord, "+", null),
                (WordStatus.UsedWord, "[", null),
                (WordStatus.UsedWord, "{", null),
                (WordStatus.UsedWord, "]", null),
                (WordStatus.UsedWord, "}", null),
                (WordStatus.UsedWord, "\\", null),
                (WordStatus.UsedWord, "|", null),
                (WordStatus.UsedWord, ";", null),
                (WordStatus.UsedWord, ":", null),
                (WordStatus.UsedWord, "'", null),
                (WordStatus.UsedWord, "\"", null),
                (WordStatus.UsedWord, ",", null),
                (WordStatus.UsedWord, "<", null),
                (WordStatus.UsedWord, ".", null),
                (WordStatus.UsedWord, ">", null),
                (WordStatus.UsedWord, "/", null),
                (WordStatus.UsedWord, "?", null),
                (WordStatus.IgnoreWord, " ", null),
                (WordStatus.IgnoreWord, "\r", null),
                (WordStatus.IgnoreWord, "\n", null),
                (WordStatus.IgnoreWord, "\t", null),

            };

        }

        IEnumerator<Ranged<string>> IEnumerable<Ranged<string>>.GetEnumerator() => Reader(_data);

        IEnumerator IEnumerable.GetEnumerator() => Reader(_data);

    }

    public record struct WordInfo(ScriptReader2.WordStatus status, string word, ScriptReader2.CustomReader? reader)
    {
        public static implicit operator (ScriptReader2.WordStatus status, string word, ScriptReader2.CustomReader? reader)(WordInfo value)
        {
            return (value.status, value.word, value.reader);
        }

        public static implicit operator WordInfo((ScriptReader2.WordStatus status, string word, ScriptReader2.CustomReader? reader) value)
        {
            return new WordInfo(value.status, value.word, value.reader);
        }
    }
}

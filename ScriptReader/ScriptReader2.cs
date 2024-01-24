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
        public ScriptReader2(string str) : this(str.GetEnumerator(), Enumerable.Empty<KeyValuePair<string, CustomReader>>(), Enumerable.Empty<string>(), Enumerable.Empty<string>())
        { }
        public ScriptReader2(IEnumerable<char> data) : this(data.GetEnumerator(), Enumerable.Empty<KeyValuePair<string, CustomReader>>(), Enumerable.Empty<string>(), Enumerable.Empty<string>())
        { }
        public ScriptReader2(IEnumerator<char> data, IEnumerable<KeyValuePair<string, CustomReader>> ImportantWordsCustom, IEnumerable<string> UsedWordsCustom, IEnumerable<string> IgnoreWordsCustom)
        {
            int maxlength = 1;
            

            ImportantWordsBase.ForEach((v) => maxlength = Math.Max(maxlength, v.Key.Length));
            ImportantWordsCustom.ForEach((v) => maxlength = Math.Max(maxlength, v.Key.Length));
            UsedWordsBase.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            UsedWordsCustom.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            IgnoreWordsBase.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            IgnoreWordsCustom.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            this._data = new CachedEnumerator<char>(data,maxlength);


            ImportantWords = new Dictionary<string, CustomReader>[maxlength];
            UsedWords = new List<string>[maxlength];
            IgnoreWords = new List<string>[maxlength];

            for(int i=0;i<maxlength;i++)
            {
                ImportantWords[i] = new Dictionary<string, CustomReader>();
                UsedWords[i] = new List<string>();
                IgnoreWords[i] = new List<string>();
            }

            ImportantWordsBase.Concat(ImportantWordsCustom).ForEach((v) => ImportantWords[v.Key.Length-1].Add(v.Key, v.Value));
            UsedWordsBase.Concat(UsedWordsCustom).ForEach((v) => UsedWords[v.Length-1].Add(v));
            IgnoreWordsBase.Concat(IgnoreWordsCustom).ForEach((v) => IgnoreWords[v.Length-1].Add(v));

            


        }



        private IEnumerator<Ranged<string>> Reader(ICachedEnumerator<char> data)
        {
            List<char> Q = new List<char>();
            int index;
            for(index=0;data.MoveNext();index++)
            {
                WordStatus status = TryFindWord(data.CachedValues, out string word, out CustomReader reader);

                switch (status)
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
                switch (status)
                {
                    case WordStatus.ImportantWord:

                        break;
                    case WordStatus.UsedWord:
                        yield return new Ranged<string>(word, (index-word.Length)..index);
                        index += word.Length;
                        MM.Repeat(word.Length, data.MoveNext);
                        data.CachedValues.RemoveRange(0, word.Length-1);
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



        enum WordStatus
        {
            None,
            ImportantWord,
            UsedWord,
            IgnoreWord,
        }
        private WordStatus TryFindWord(List<char> charlist, out string word, out CustomReader reader)
        {
            for (int i = charlist.Count; i > 0;)
            {
                var theVars = ImportantWords[--i].Where(x => charlist.Match(x.Key, x.Key.Length));
                switch (theVars.Count())
                {
                    case > 1:
                        throw new Exception("Custom ImportantWord Syntax Error");
                    case 1:
                        word = theVars.First().Key;
                        reader = theVars.First().Value;
                        return WordStatus.ImportantWord;
                }
            }
            for (int i = charlist.Count; i > 0;)
            {
                string? theString = UsedWords[--i].Find(x => charlist.Match(x, x.Length));
                if (theString != null)
                {
                    word = theString;
                    reader = null;
                    return WordStatus.UsedWord;
                }
            }
            for (int i = charlist.Count; i > 0;)
            {
                string? theString = IgnoreWords[--i].Find(x => charlist.Match(x, x.Length));
                if (theString != null)
                {
                    word = theString;
                    reader = null;
                    return WordStatus.IgnoreWord;
                }
            }//same code Ctrl C V not good code?


            word = "";
            reader = null;
            return WordStatus.None;
        }

        public Dictionary<string, CustomReader>[] ImportantWords { get; init; }
        public List<string>[] UsedWords { get; init; }
        public List<string>[] IgnoreWords { get; init; }


        public static Dictionary<string, CustomReader> ImportantWordsBase;
        public static List<string> UsedWordsBase;
        public static List<string> IgnoreWordsBase;

        static ScriptReader2()
        {
            ImportantWordsBase = new Dictionary<string, CustomReader>()
            {
                ["\"\"\""] = (data) =>
                {
                    return null;
                },
                ["안돼"] = (data) =>
                {
                    return null;
                },
            };


            UsedWordsBase = new List<string>(new string[]{
            "###", "...",
            "##", "..", "=>", "->",
            "\"", "#", ".", "~", "`", "!", "@", "$", "%", "^",
            "&", "*", "(", ")", "-", "+", "=", "[", "]", "{",
            "}", ":", ";", "\'", ",", "<", ".", ">", "?", "/",
            "\\", "|",
            });

            IgnoreWordsBase = new List<string>(new string[]{
            " ", "\r", "\n", "\t",
            });
        }

        IEnumerator<Ranged<string>> IEnumerable<Ranged<string>>.GetEnumerator() => Reader(_data);

        IEnumerator IEnumerable.GetEnumerator() => Reader(_data);

    }
}

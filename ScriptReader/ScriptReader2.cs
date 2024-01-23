using MPLLib.Beauty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MPLLib
{
    public class ScriptReader2 : IEnumerable<Ranged<string>>
    {
        private IEnumerator<char> _data { get; set; }
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
            this._data = data;

            ImportantWordsBase.ForEach((v) => maxlength = Math.Max(maxlength, v.Key.Length));
            ImportantWordsCustom.ForEach((v) => maxlength = Math.Max(maxlength, v.Key.Length));
            UsedWordsBase.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            UsedWordsCustom.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            IgnoreWordsBase.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            IgnoreWordsCustom.ForEach((v) => maxlength = Math.Max(maxlength, v.Length));
            MaxLength = maxlength;

            ImportantWords = new Dictionary<string, CustomReader>[MaxLength];
            UsedWords = new List<string>[MaxLength];
            IgnoreWords = new List<string>[MaxLength];

            ImportantWordsBase.Concat(ImportantWordsCustom).ForEach((v) => ImportantWords[v.Key.Length].Add(v.Key, v.Value));
            UsedWordsBase.Concat(UsedWordsCustom).ForEach((v) => UsedWords[v.Length].Add(v));
            IgnoreWordsBase.Concat(IgnoreWordsCustom).ForEach((v) => IgnoreWords[v.Length].Add(v));


        }



        private IEnumerator<Ranged<string>> Reader(IEnumerator<char> data)
        {
            int checkdelay = 0;
            List<char> Q = new List<char>();
            for(int index = 0; data.MoveNext(); index++)
            {
                
            }
            throw new NotImplementedException();
        }

        public delegate IEnumerator<Ranged<string>> CustomReader(IEnumerator<char> data);



        private CompareFlag CompareUsedWords(List<char> chars, out string foundword, out bool check)
        {
            foundword = "";
            bool isFound = false;
            bool isCheck = false;
            for (int i = 1; i < MaxLength + 1; i++) // word 길이별 반복문
            {
                foreach (var str in ImportantWords[i].Keys.Concat(UsedWords[i]).Concat(IgnoreWords[i]))
                {
                    for (int j = i; j > 0; j--) // word 하위 매칭용, i==j일때 true면 Found, i!=j일때 true면 Check, j는 매칭할 길이
                    {
                        if (chars.MatchFromBehind(str, j))
                        {
                            if (i == j)
                            {
                                isFound = true;
                                foundword = str;
                            }
                            else
                            {
                                isCheck = true;
                            }
                        }
                    }
                }
            }

            bool InlineFunction(IEnumerable<string> umjunsick, int step, out string str2)
            {
                foreach (var str in umjunsick)
                {
                    for (int j =step; j > 0; j--) // word 하위 매칭용, i==j일때 true면 Found, i!=j일때 true면 Check, j는 매칭할 길이
                    {
                        if (chars.MatchFromBehind(str, j))
                        {
                            if (step == j)
                            {
                                str2 = str;
                                return true;
                            }
                            else
                            {
                                isCheck = true;
                            }
                        }
                    }
                }
            }


        }
        private enum CompareFlag
        {
            NotFound,
            ImportantWord,
            UsedWord,
            IgnoreWord
        }

        private int MaxLength { get; init; }

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

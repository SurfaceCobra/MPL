using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MPLLib
{
    namespace DataExecute

    {

        public class ScriptReaderOLD : IEnumerable<Ranged<string>>
        {
            char[] Src;
            int index;

            public ScriptReaderOLD(string src)
            {
                this.Src = src.ToCharArray();
                index = 0;
            }

            public ScriptReaderOLD(char[] src, int index)
            {
                this.Src = src;
                index = 0;
            }

            public bool TryReadNext(out Ranged<string> output)
            {
                output = null;


                //index 는 시작지점, curr은 길이

                int curr = 0;
                int currIndex() => index + curr;
                char currWord() => Src[currIndex()];

                if (currIndex() >= Src.Length)
                {
                    return false;
                }

                //초기 IgnoreWord 제거
                while (true)
                {
                    if (currIndex() >= Src.Length)
                    {
                        return false;
                    }
                    else if (IgnoreWords.Contains(currWord()))
                    {
                        curr++;
                    }
                    else
                    {
                        break;
                    }
                }

                //시작이 UsedWords일경우 바로 반환
                if (UsedWords.Contains(currWord()))
                {
                    output = new Ranged<string>(currWord().ToString(), index..currIndex());
                    index += curr + 1;

                    return true;
                }



                int beginIndex = currIndex();


                //시작이 "일경우 다음 "까지 쭉 달림
                if (currWord() == '\"')
                {
                    curr++;
                    while (currWord() != '\"')
                    {
                        curr++;
                    }
                    curr++;

                    output = new Ranged<string>(new(Src[beginIndex..currIndex()]), index..currIndex());

                    index += curr;
                    return true;
                }


                curr++;




                while (true)
                {
                    if (currIndex() >= Src.Length)
                    {
                        if (beginIndex == currIndex())
                        {
                            return false;
                        }
                        else
                        {
                            output = new Ranged<string>(new(Src[beginIndex..currIndex()]), index..currIndex());
                            index += curr;
                            return true;
                        }
                    }
                    else if (UsedWords.Contains(currWord()) || IgnoreWords.Contains(currWord()))
                    {
                        output = new Ranged<string>(new(Src[beginIndex..currIndex()]), index..currIndex());
                        index += curr;
                        return true;
                    }
                    curr++;
                }
            }

            public string ReadNext()
            {
                TryReadNext(out Ranged<string> str);
                return str;
            }

            public IEnumerator<Ranged<string>> GetEnumerator()
            {
                while (this.TryReadNext(out Ranged<string> str))
                {
                    yield return str;
                }
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            char[] UsedWords = new char[]
            {
            '.',
            ',',
            '<',
            '>',
            '/',
            '?',
            ':',
            ';',
            '\'',
            '[',
            ']',
            '{',
            '}',
            '(',
            ')',
            '*',
            '&',
            '^',
            '%',
            '$',
            '#',
            '@',
            '!',
            '~',
            '`',
            '-',
            '=',
            '+',
            '.',
            };
            char[] IgnoreWords = new char[]
            {
            ' ',
            '\r',
            '\n',
            '\t',
            };


        }


        public class FunctionReader : IEnumerable<string>
        {
            public ScriptReaderOLD reader;

            public enum Bracket
            {
                Small,
                Medium,
                Big,
                Comparer
            }

            public IEnumerator<string> GetEnumerator()
            {
                Stack<Bracket> stack = new Stack<Bracket>(32);
                foreach (string text in this.reader)
                {
                    switch (text)
                    {
                        case "(":
                            stack.Push(Bracket.Small);
                            goto end;
                        case "{":
                            stack.Push(Bracket.Medium);
                            goto end;
                        case "[":
                            stack.Push(Bracket.Big);
                            goto end;
                        case "<":
                            stack.Push(Bracket.Comparer);
                            goto end;

                        end:
                            yield return text;
                            break;

                        case ")":
                            if (stack.Peek() != Bracket.Small)
                                throw new Exception(stack.Peek() + "괄호가 더닫힘");
                            stack.Pop();
                            break;
                        case "}":
                            if (stack.Peek() != Bracket.Medium)
                                throw new Exception(stack.Peek() + "괄호가 더닫힘");
                            stack.Pop();
                            break;
                        case "]":
                            if (stack.Peek() != Bracket.Big)
                                throw new Exception(stack.Peek() + "괄호가 더닫힘");
                            stack.Pop();
                            break;
                        case ">":
                            if (stack.Peek() != Bracket.Comparer)
                                throw new Exception(stack.Peek() + "괄호가 더닫힘");
                            stack.Pop();
                            break;
                    }

                    if (stack.Count == 0)
                        yield break;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }




    }



}

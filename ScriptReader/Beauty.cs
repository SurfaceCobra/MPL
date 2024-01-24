using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using static MPLLib.ScriptBlock;

namespace MPLLib
{
    namespace Beauty
    {

        public static class ExtensionMethodIsHere
        {

            public static void WriteLine(this string str) => Console.WriteLine(str);
            public static void Write(this string str) => Console.Write(str);

            public static void WriteLine(this object obj) => Console.WriteLine(obj.ToString());
            public static void Write(this object obj) => Console.Write(obj.ToString());

            public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
            {
                foreach (T item in values) action(item);
                return values;
            }

            public static IEnumerable<T> AllWriteThenLine<T>(this IEnumerable<T> values)
            {
                foreach (T item in values) Console.Write(item.ToString());
                Console.WriteLine();
                return values;
            }
            public static IEnumerable<T> AllWriteLine<T>(this IEnumerable<T> values, object afterwriter)
            {
                foreach (T item in values)
                {
                    Console.WriteLine(item.ToString());
                    Console.WriteLine(afterwriter.ToString());
                }
                return values;
            }
            public static IEnumerable<T> AllWriteLine<T>(this IEnumerable<T> values)
            {
                foreach (T item in values) Console.WriteLine(item.ToString());
                return values;
            }
            public static IEnumerable<T> AllWrite<T>(this IEnumerable<T> values, object afterwriter)
            {
                foreach (T item in values)
                {
                    Console.Write(item.ToString());
                    Console.Write(afterwriter.ToString());
                }
                return values;
            }
            public static IEnumerable<T> AllWrite<T>(this IEnumerable<T> values)
            {
                foreach (T item in values) Console.Write(item.ToString());
                return values;
            }


            public static IEnumerable<T> WriteLine<T>(this IEnumerable<T> value)
            {
                Console.WriteLine();
                return value;
            }
            public static IEnumerable<T> WriteLine<T>(this IEnumerable<T> value, object afterwriter)
            {
                Console.WriteLine(afterwriter.ToString());
                return value;
            }
            public static IEnumerable<T> Write<T>(this IEnumerable<T> value, object afterwriter)
            {
                Console.Write(afterwriter.ToString());
                return value;
            }

            public static int Length(this Range range) => range.End.Value - range.Start.Value;


            public static IEnumerable<int> FindAllIndex<T>(this List<T> list, Predicate<T> predicate)
            {
                for(int i=0;i<list.Count; i++)
                {
                    if (predicate(list[i]))
                    {
                        yield return i;
                    }
                }
            }

            public static IEnumerable<List<T>> Split<T>(this List<T> list, T match) => Split<T>(list, (x) => object.Equals(match, x));
            public static IEnumerable<List<T>> Split<T>(this List<T> list, Predicate<T> predicate)
            {
                List<int> splitlist = list.FindAllIndex((x) => predicate(x)).ToList();
                splitlist.Insert(0,-1);
                splitlist.Add(list.Count+1);
                for(int i=1;i<splitlist.Count;i++)
                {
                    int skipcount = splitlist[i - 1] + 1;
                    int takecount = splitlist[i] - splitlist[i - 1] - 1;
                    yield return list.Skip(skipcount).Take(takecount).ToList();
                }
            }

            public static bool Match<T>(this IEnumerable<T> origin, IEnumerable<T> target, int matchlength)
            {
                IEnumerator<T> val1 = origin.GetEnumerator();
                IEnumerator<T> val2 = target.GetEnumerator();
                for(int i=0;i<matchlength;i++)
                {
                    
                    bool state = !val1.MoveNext();
                    state |= !val2.MoveNext();
                    state |= !Object.Equals(val1.Current, val2.Current);

                    if (state) return false;
                }
                return true;
            }

            public static bool MatchFromBehind<T>(this List<T> origin, List<T> target) => MatchFromBehind(origin, target, origin.Count, target.Count);
            public static bool MatchFromBehind<T>(this List<T> origin, IEnumerable<T> target, int matchlength) => MatchFromBehind(origin, target, origin.Count, matchlength);
            public static bool MatchFromBehind<T>(this IEnumerable<T> origin, IEnumerable<T> target, int originlength, int matchlength)
            {
                return origin.Skip(originlength - matchlength).Take(matchlength).Match(target, matchlength);
            }



        }

        public static class MM
        {
            public static void Repeat(int count, Action act)
            {
                for (int i = 0; i < count; i++) act();
            }
            public static void Repeat<T1>(int count, Action<T1> act, T1 val1)
            {
                for (int i = 0; i < count; i++) act(val1);
            }

            public static IEnumerable<Tout> Repeat<Tout>(int count, Func<Tout> func)
            {
                for (int i = 0; i < count; i++) yield return func();
            }

            public static IEnumerable<Tout> Repeat<T1, Tout>(int count, Func<T1, Tout> func, T1 val1)
            {
                for (int i = 0; i < count; i++) yield return func(val1);
            }

        }

        public class Flag<T> where T:IEquatable<T>
        {

        }

        public class DummyException : Exception
        {
            public DummyException()
            {

            }
        }
        
    }
}

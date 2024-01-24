using MPLLib;
using MPLLib.Beauty;
using MPLConverter;
using MPL;

namespace ConsoleTester
{
    internal class Program
    {
        static string DIR = @"C:\Users\류기암\Source\Repos\MPL\MPLConverter\Source.txt";
        static char EOFWord = (char)0xFF;


        static void Main(string[] args)
        {
            ScriptReader2Test();
        }

        static void ScriptReader2Test()
        {
            ScriptReader2 newReader = new ScriptReader2("여러분 @제가   !! 언제=>! 경1557제");
            newReader.AllWriteLine("------------").WriteLine("@@END@@");
        }


        static void ForeachChangeTest()
        {
            List<int> ints = new List<int>();
            for (int i = 0; i < 10; i++) ints.Add(i);

            var v = ints.GetEnumerator();
            while (v.MoveNext())
            {
                Console.WriteLine(v.Current);
            }
        }

        static void ScriptReaderTest()
        {
            string src = File.ReadAllText(DIR);

            ScriptReader reader = new ScriptReader(src);

            foreach (string s in reader)
            {
                Console.WriteLine(s);
            }
        }

        static void ScriptBlockReaderTest()
        {
            string src = File.ReadAllText(DIR);

            ScriptReader reader = new ScriptReader(src);

            ScriptBlock.Block scriptBlock = ScriptBlock.Create(reader);

            Console.WriteLine(scriptBlock);

            ;
        }

        static void BlockBuildTest()
        {
            string src = File.ReadAllText(DIR);

            ScriptReader reader = new ScriptReader(src);

            //reader.AllWriteLine();

            //Console.Read();

            ScriptBlock.Block block = ScriptBlock.Create(reader);

            //Console.WriteLine(block);
            //Console.ReadLine();

            //Console.WriteLine("");

            var mplPS = Builder.Build(block);


            Console.WriteLine("");

            ;

        }

        static void SplitTest()
        {
            List<int> ints = new List<int> { 0,0, 1, 2, 3, 4, 0, 5, 6, 7, 0, 8, 9,0 };

            ints.FindAllIndex(i => i == 0).AllWriteLine();

            ints.Split(x => x == 0).ForEach(x=> x.AllWriteThenLine());
            Console.WriteLine();
            ints.Split(0).ForEach(x=> x.AllWriteThenLine());

        }

        static void MatchTest()
        {
            string str1 = "qwerasdf";
            string str2 = "asdfwwww";

            str1.Match(str2, 5).WriteLine();
            str1.MatchFromBehind(str2, 8, 4).WriteLine();
        }

        static void CachedEnumeratorTest()
        {
            using ICachedEnumerator<char> cce = "     12345".GetCachedEnumerator(5);
            while (cce.MoveNext())
                cce.CachedValues.AllWriteThenLine();


            "First Half Done!".WriteLine();

            cce.Reset();
            while(cce.MoveNext())
                cce.Peek(10).AllWriteThenLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPLLib.DataExecute;
using static MPLLib.DataExecute.ScriptReader;

namespace MPLLib.DataExecute
{
    public partial class Begex
    {

        public static void ParseStart(string begex)
        {
            WordInfo[] customInfos =
            [
                (WordStatus.UsedWord, "x"),
                (WordStatus.UsedWord, "c"),
                (WordStatus.UsedWord, "h"),
                (WordStatus.UsedWord, "s"),
                (WordStatus.UsedWord, ";=>"),
                (WordStatus.UsedWord, ":=>"),
                (WordStatus.ImportantWord, "$", CreateReaderAnnotationUntil("$")),
            ];
            ScriptReader reader = new ScriptReader(begex.GetEnumerator(), customInfos);
            ScriptBlock.Block block = ScriptBlock.Create(reader);

            ParseBlockBasic(block);
        }
        public static void ParseBlockBasic(ScriptBlock.Block srcBlock)
        {



            var src = srcBlock.GetEnumerator();
            while(src.MoveNext())
            {
                ScriptBlock unknown = src.Current;
                switch(unknown)
                {
                    case ScriptBlock.Value value:
                        bool isCharorString;
                        switch(value.value)
                        {

                        }
                        break;
                    case ScriptBlock.Block block:
                        switch(block.shape)
                        {
                            case Bracket.Shape.Small or Bracket.Shape.EOF:
                                break;
                            case Bracket.Shape.Medium:
                                break;
                            case Bracket.Shape.Big:
                                break;
                        }
                        break;
                }
            }
        }
        public static void ParseBlockOrByte()
        {

        }

        public static void ParseBlockCountOf()
        {

        }
    }
}

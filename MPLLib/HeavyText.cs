using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib
{
    public class TextInfo
    {
        public Color? textColor;
        public Color? backgroundColor;
        public bool? isItalic;
        public bool? isLined;
        public Color? LineColor;

        public TextInfo()
        {

        }

        public TextInfo(Color? textColor, Color? backgroundColor, bool? isItalic, bool? isLined, Color? lineColor) : this()
        {
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
            this.isItalic = isItalic;
            this.isLined = isLined;
            LineColor = lineColor;
        }

        public static TextInfo GetNull()
        {
            return new TextInfo();
        }
        public static TextInfo GetDefault()
        {
            return new TextInfo(Color.Black, Color.White, false, false, null);
        }
        public static TextInfo GetDark()
        {
            return new TextInfo(Color.White, Color.DarkGray, false, false, null);
        }
    }
    public class HeavyText
    {
        string OriginText;
        Dictionary<Ranged<string>, TextInfo> texts;
        TextInfo BaseTextInfo;

        private HeavyText()
        {
            texts = new Dictionary<Ranged<string>, TextInfo>();
        }
        public HeavyText(string OriginText, TextInfo BaseTextInfo) : this()
        {
            this.OriginText = OriginText;
        }
        public HeavyText(string OriginText) : this(OriginText, TextInfo.GetDefault())
        {

        }

        public void AddString(Ranged<string> str) => this.AddString(str, new TextInfo());

        public void AddString(Ranged<string> str, TextInfo info) => texts.Add(str, info);



        public string GetHTML()
        {
            StringBuilder sb = new StringBuilder();
            int PushIndex = 0;

            List<ConvertableData> ConvertList = new List<ConvertableData>();

            for(int i=0;i< OriginText.Length; i++)
            {
                if (HTMLEncodingHelper.TryGetValue(OriginText[i], out string value))
                {
                    ConvertList.Add(new ConvertableData(new Ranged<string>(value, i..1), TextInfo.GetNull()));
                }
            }

            ConvertList.Sort((left, right) => left.resultString.range.Start.Value.CompareTo(right.resultString.range.Start.Value));

            sb = sb.Replace("\r\n", "<br>");
            return "";
        }
        private Dictionary<char, string> HTMLEncodingHelper = new Dictionary<char, string>()
        {
            ['"'] = "&quot;",
            ['&'] = "&amp;",
            ['<'] = "&lt;",
            ['>'] = "&gt;",
            ['?'] = "-",
            //[''] = "&nbsp;",
            ['¡'] = "&iexcl;",
            ['￠'] = "&cent;",
            ['￡'] = "&pound;",
            ['¤'] = "&curren;",
            ['￥'] = "&yen;",
            ['|'] = "&brvbar;",
            ['§'] = "&sect;",
            ['¨'] = "&uml;",
            ['ⓒ'] = "&copy;",
            ['ª'] = "&ordf;",
            ['≪'] = "&laquo;",
            ['￢'] = "&not;",
            //[''] = "&shy;",
            ['?'] = "&reg;",
            ['°'] = "&deg;",
            ['±'] = "&plusmn;",
            ['²'] = "&sup2;",
            ['³'] = "&sup3;",
            ['´'] = "&acute;",
            ['μ'] = "&micro;",
            ['¶'] = "&para;",
            ['·'] = "&middot;",
            ['¸'] = "&cedil;",
            ['¹'] = "&sup1;",
            ['º'] = "&ordm;",
            ['≫'] = "&raquo;",
            ['¼'] = "&frac14;",
            ['½'] = "&frac12;",
            ['¾'] = "&frac34;",
            ['¿'] = "&iquest;",
            ['A'] = "&Agrave;",
            ['A'] = "&Aacute;",
            ['A'] = "&Acirc;",
            ['A'] = "&Atilde;",
            ['A'] = "&Auml;",
            ['A'] = "&Aring;",
            ['Æ'] = "&AElig;",
            ['C'] = "&Ccedil;",
            ['E'] = "&Egrave;",
            ['E'] = "&Eacute;",
            ['E'] = "&Ecirc;",
            ['E'] = "&Euml;",
            ['I'] = "&Igrave;",
            ['I'] = "&Iacute;",
            ['I'] = "&Icirc;",
            ['I'] = "&Iuml;",
            ['Ð'] = "&ETH;",
            ['N'] = "&Ntilde;",
            ['O'] = "&Ograve;",
            ['O'] = "&Oacute;",
            ['O'] = "&Ocirc;",
            ['O'] = "&Otilde;",
            ['O'] = "&Ouml;",
            ['×'] = "&times;",
            ['Ø'] = "&Oslash;",
            ['U'] = "&Ugrave;",
            ['U'] = "&Uacute;",
            ['U'] = "&Ucirc;",
            ['U'] = "&Uuml;",
            ['Y'] = "&Yacute;",
            ['Þ'] = "&THORN;",
            ['ß'] = "&szlig;",
            ['a'] = "&agrave;",
            ['a'] = "&aacute;",
            ['a'] = "&acirc;",
            ['a'] = "&atilde;",
            ['a'] = "&auml;",
            ['a'] = "&aring;",
            ['æ'] = "&aelig;",
            ['c'] = "&ccedil;",
            ['e'] = "&egrave;",
            ['e'] = "&eacute;",
            ['e'] = "&ecirc;",
            ['e'] = "&euml;",
            ['i'] = "&igrave;",
            ['i'] = "&iacute;",
            ['i'] = "&icirc;",
            ['i'] = "&iuml;",
            ['ð'] = "&eth;",
            ['n'] = "&ntilde;",
            ['o'] = "&ograve;",
            ['o'] = "&oacute;",
            ['o'] = "&ocirc;",
            ['o'] = "&otilde;",
            ['o'] = "&ouml;",
            ['÷'] = "&divide;",
            ['ø'] = "&oslash;",
            ['u'] = "&ugrave;",
            ['u'] = "&uacute;",
            ['u'] = "&ucirc;",
            ['u'] = "&uuml;",
            ['y'] = "&yacute;",
            ['þ'] = "&thorn;",
            ['y'] = "&yuml;",
        };
    }
    public class ConvertableData
    {
        //resultString의 range는 HTML 변환과정에서 originString의 range로 표시됨
        public Ranged<string> resultString;
        public TextInfo info;

        public ConvertableData(Ranged<string> resultString, TextInfo info)
        {
            this.resultString = resultString;
            this.info = info;
        }
    }
}

﻿using MPLLib.DataExecute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.DataExecute
{
    public partial class Begex
    {

        static BegexArg[] ParseFullBlock(string regex)
        {
            int i = 0;
            List<BegexArg> args = new List<BegexArg>();
            while (i < regex.Length)
            {
                args.Add(ParseSingleBlock(regex, ref i));
            }
            return args.ToArray();
        }
        static char[] ParseString(string regex, ref int i)
        {
            if (regex[i++] != '\"') throw new Exception();
            List<char> charlist = new List<char>();
            while (i < regex.Length)
            {
                char c = regex[i++];
                if (c == '\"')
                    return charlist.ToArray();
                charlist.Add(c);
            }
            throw new Exception();
        }
        static byte ParseSingleUnknown(string regex, ref int i)
        {
            switch (regex[i++])
            {
                case 'x':
                    return ParseSingleByte(regex, ref i);
                case 'c':
                    return ParseSingleChar(regex, ref i);
                default: throw new Exception();
            }
        }
        static byte ParseSingleByte(string regex, ref int i)
        {
            return Convert.ToByte($"{regex[i++]}{regex[i++]}", 16);
        }
        static byte ParseSingleChar(string regex, ref int i)
        {
            return Encoding.ASCII.GetBytes(regex, i++, 1)[0];
        }
        static Range ParseCountOf(string regex, ref int i)
        {
            StringBuilder sb = new StringBuilder();
            while (regex[i++] != '}') sb.Append(regex[i - 1]);
            string countResult = sb.ToString();

            if (countResult.Any(x => x == ','))
            {
                string[] countResults = countResult.Split(',');
                switch (countResults.Length)
                {
                    case 1:
                        return int.Parse(countResults[0])..int.MaxValue;
                    case 2:
                        return int.Parse(countResults[0])..int.Parse(countResults[1]);
                    default:
                        throw new Exception();
                }
            }
            else
                return int.Parse(countResult)..int.Parse(countResult);
        }
        static bool[] ParseOrByte(string regex, ref int i)
        {
            bool[] result = Enumerable.Repeat(false, 256).ToArray();
            byte? lastByte = null;
            bool isReverse = false;
            while (true)
            {
                char m = regex[i++];
                switch (m)
                {
                    case 'x':
                        if (lastByte is not null)
                            result[lastByte ?? 0x00] = true;
                        lastByte = ParseSingleByte(regex, ref i);
                        break;
                    case 'c':
                        if (lastByte is not null)
                            result[lastByte ?? 0x00] = true;
                        lastByte = ParseSingleChar(regex, ref i);
                        break;
                    case '-':
                        byte laterByte = ParseSingleUnknown(regex, ref i);
                        byte min = Math.Min(lastByte ?? 0x00, laterByte);
                        byte max = Math.Max(lastByte ?? 0x00, laterByte);
                        for (byte j = min; j <= max; j++)
                            result[j] = true;
                        lastByte = null;
                        break;
                    case '^':
                        isReverse = !isReverse;
                        break;
                    case ']':
                        if (lastByte is not null)
                            result[lastByte ?? 0x00] = true;
                        if (isReverse)
                            for (int j = 0; j < 256; j++) result[j] = !result[j];
                        return result;
                }
            }
        }
        static BegexArg ParseSingleBlock(string regex, ref int i)
        {
            List<List<BegexArg>> argsOrs = [[]];
            int currentOrStep = 0;

            while (true)
            {
                if (i == regex.Length)
                {
                    return Finalize();
                }
                char m = regex[i++];
                switch (m)
                {
                    case 'x':
                        argsOrs[currentOrStep].Add(new BegexArg.OrByte(ParseSingleByte(regex, ref i)));
                        break;
                    case 'c':
                        argsOrs[currentOrStep].Add(new BegexArg.OrByte(ParseSingleChar(regex, ref i)));
                        break;
                    case '.':
                        argsOrs[currentOrStep].Add(new BegexArg.OrByte(Enumerable.Repeat(true, 256).ToArray()));
                        break;

                    case 's':
                        foreach (byte c in ParseString(regex, ref i))
                        {
                            argsOrs[currentOrStep].Add(new BegexArg.OrByte(c));
                        }
                        break;
                    case 'h':
                        char[] chars = ParseString(regex, ref i);
                        if (chars.Length % 2 == 1) throw new Exception();
                        for (int j = 0; j < chars.Length; j += 2)
                        {
                            argsOrs[currentOrStep].Add(new BegexArg.OrByte(Convert.ToByte($"{chars[j]}{chars[j + 1]}", 16)));
                        }
                        break;

                    case '[':
                        argsOrs[currentOrStep].Add(new BegexArg.OrByte(ParseOrByte(regex, ref i)));
                        break;
                    case '{':
                    openCountOf:
                        Range range = ParseCountOf(regex, ref i);
                        return new BegexArg.CountOf(Finalize(), range);
                    case '|':
                        currentOrStep++;
                        argsOrs.Add([]);
                        break;
                    case '(':
                        return Finalize();
                    case ')':
                        if (i == regex.Length) return Finalize();
                        switch (regex[i])
                        {
                            case '{' or '+':
                                i++;
                                goto openCountOf;
                            default:
                                return Finalize();
                        }
                }
            }

            BegexArg Finalize() => new BegexArg.GroupOrBytes(argsOrs.Select(x => new BegexArg.GroupBytes(x.ToArray())).ToArray());
        }
        enum ParseState
        {
            Basic,
            OrState,
            OrStateRangeEnd,
        }



    }
}

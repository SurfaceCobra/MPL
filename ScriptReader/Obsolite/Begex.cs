using System.Text;

namespace MPLLib.DataExecute
{
    public partial class Begex
    {
        BegexArg[] args;
        public Begex(string regex)
        {
            this.args = ParseFullBlock(regex);
        }

        public int MatchAt(byte[] bytes, int index)
        {
            int startindex = index;
            int i = index;
            foreach (var arg in args)
            {
                if (!arg.IsMatch(bytes, ref i))
                {
                    return 0;
                }
            }
            return i - startindex;
        }
        public Range[] MatchAll(byte[] bytes, int index, int length)
        {
            List<Range> ranges = new List<Range>();
            for (int i = index; i < index + length;)
            {
                int matchLen = MatchAt(bytes, i);
                if (matchLen != 0)
                {
                    ranges.Add(i..(i + matchLen));
                    i += matchLen; //  no shifted duplicate
                }
                else
                {
                    i++;
                }
            }
            return ranges.ToArray();
        }
        public Range[] MatchAll(byte[] bytes) => MatchAll(bytes, 0, bytes.Length);
        public void CompileCSMatchAt()
        {
            //DynamicMethod method = new DynamicMethod("CompiledRegex", typeof((int index, int count)[]), new Type[] { typeof(byte[]), typeof(int), typeof(int) });
            //gg
            throw new Exception();
        }
    }
    internal interface BegexArg
    {
        public interface GroupWorking;
        public bool IsMatch(byte[] bytes, ref int i);


        public class OrByte : BegexArg
        {
            public OrByte(bool[] adresses)
            {
                this.adresses = adresses;
            }
            public OrByte(params byte[] trueBytes)
            {
                this.adresses = Enumerable.Repeat(false, 256).ToArray();
                foreach (byte b in trueBytes)
                {
                    adresses[b] = true;
                }
            }
            bool[] adresses { get; init; }

            public bool IsMatch(byte[] bytes, ref int i)
            {
                if (adresses[bytes[i]])
                {
                    i++;
                    return true;
                }
                return false;
            }
        }
        public class GroupBytes : BegexArg, GroupWorking
        {
            public GroupBytes(BegexArg[] args)
            {
                this.args = args;
            }
            BegexArg[] args { get; init; }

            public bool IsMatch(byte[] bytes, ref int i)
            {
                int startindex = i;
                try
                {
                    foreach (var arg in args)
                    {
                        if (!arg.IsMatch(bytes, ref i))
                        {
                            i = startindex;
                            return false;
                        }
                    }
                    return true;
                }
                catch
                {
                    i = startindex;
                    return false;
                }
            }
        }

        public class GroupOrBytes : BegexArg, GroupWorking
        {
            public GroupOrBytes(GroupBytes[] orGroups)
            {
                this.orGroups = orGroups;
            }
            GroupBytes[] orGroups { get; init; }

            public bool IsMatch(byte[] bytes, ref int i)
            {
                try
                {
                    foreach (var orGroup in orGroups)
                    {
                        if (orGroup.IsMatch(bytes, ref i))
                            return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        public class CountOf : BegexArg, GroupWorking
        {
            public CountOf(BegexArg arg, Range range)
            {
                this.arg = arg;
                this.range = range;
            }


            BegexArg arg { get; init; }
            Range range { get; init; }
            public bool IsMatch(byte[] bytes, ref int i)
            {
                int startindex = i;
                int count = 0;
                while (true)
                    if (arg.IsMatch(bytes, ref i))
                    {
                        if (count == range.End.Value - 1)
                            return true;
                        count++;
                    }
                    else if (range.Start.Value <= count && count < range.End.Value)
                        return true;
                    else
                    {
                        i = startindex;
                        return false;
                    }
            }
        }
    }

    internal interface IBegexArg
    {
        public interface IMatchable
        {
            public bool IsMatch(byte[] bytes, int i, out IBegexMatchResult ResultData);
        }
        


        public class SingleByte : IMatchable
        {
            public SingleByte(byte matchByte)
            {
                this.matchByte = matchByte;
            }
            byte matchByte { get; init; }
            public bool IsMatch(byte[] bytes, int i, out IBegexMatchResult result)
            {
                if (bytes[i]== matchByte)
                {
                    result = new IBegexMatchResult.Length(1);
                    return true;
                }
                result = null;
                return false;
            }
        }
        public class OrByte : IMatchable
        {
            public OrByte(bool[] adresses)
            {
                this.adresses = adresses;
            }
            public OrByte(params byte[] trueBytes)
            {
                this.adresses = Enumerable.Repeat(false, 256).ToArray();
                foreach (byte b in trueBytes)
                {
                    adresses[b] = true;
                }
            }
            bool[] adresses { get; init; }

            public bool IsMatch(byte[] bytes, int i, out IBegexMatchResult result)
            {
                if (adresses[bytes[i]])
                {
                    result = new IBegexMatchResult.Length(1);
                    return true;
                }
                result = null;
                return false;
            }
        }

        public class Group
        {
            public Group(IBegexArg[] args, BegexOption[] options)
            {
                this.args = args;
                this.options = options;
            }
            IBegexArg[] args { get; init; }

            BegexOption[] options { get; init; }
        }


    }
    internal interface BegexOption
    {
        public class LookAround : BegexOption
        {

        }
    }
    

    internal interface IBegexMatchResult
    {
        public struct None : IBegexMatchResult { }
        public struct Length : IBegexMatchResult
        {
            public int length { get; init; }
            public Length(int length)
            {
                this.length = length;
            }
        }
    }
}




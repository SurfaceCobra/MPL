using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPLLib.Cached;
using MPLLib.ArrayVariants;
using MPLLib.ExtensionMethod;

namespace MPLLib.ByteRegularExpression
{
    public partial class Begex
    {
        const int CACHESIZE = 4 * 1024;
    }

    public class BegexRun : IDisposable
    {
        CachedStreamDynamic cs { get; init; }
        long currentIndex = 0;
        IndexArray<byte> currentBytes { get; init; }


        MatchGroup BaseBlock { get; init; }

        public BegexRun(Begex begex, Stream stream, int searchBuffer = 1024)
        {
            currentBytes = new IndexArray<byte>(searchBuffer);
            cs = new CachedStreamDynamic(stream);
            this.BaseBlock = begex.group;
        }
        public void Dispose() => cs.Dispose();

        public IEnumerable<byte[]> Run()
        {
            for(long i=0;i<cs.stream.Length;)
            {
                if(RunAt(i))
                {
                    i = currentIndex;
                    yield return currentBytes.ToArray();
                }
                else
                {
                    i++;
                }
            }
            yield break;
        }

        public bool RunAt(long index)
        {
            currentIndex = index;
            try
            {
                currentBytes.Clear();
                return SafeLaunch(()=>RunBlock(BaseBlock), false);
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        private bool RunBlock(MatchGroup block)
        {


            Range range = 1..1;
            bool exclude = false;
            byte[] swapbytes = null;
            foreach (var option in block.options)
            {
                switch (option)
                {
                    case IMatchOption.CountOf optionCountOf:
                        range = optionCountOf.countRange;
                        break;
                    case IMatchOption.Exclude:
                        exclude = true;
                        break;
                    case IMatchOption.Swap optionSwap:
                        swapbytes = optionSwap.swapByte;
                        break;
                    default:
                        throw new Exception();
                }
            }

            if(!SafeLaunch(() =>
            {
                int repeatCount;
                for (repeatCount = 0; repeatCount < range.End.Value; repeatCount++)
                {
                    if (!SafeLaunch(() => RunBlockInnerRepeat(block.args)))
                    {
                        break;
                    }
                }

                if (!range.IsInside(repeatCount))
                    return false;
                return true;
            }, exclude, swapbytes))
            {
                return false;
            }


            return true;
        }

        private bool RunBlockInnerRepeat(IEnumerable<IBegexArg> args)
        {
            foreach (IBegexArg arg in args)
            {
                switch (arg)
                {
                    case MatchOrByte orByte:
                        byte currentByte = cs[currentIndex++];
                        if (!orByte.matchBytes[currentByte])
                            return false;
                        currentBytes.Add(currentByte);
                        break;
                    case MatchGroup innerBlock:
                        if (!SafeLaunch(()=>RunBlock(innerBlock)))
                            return false;
                        break;
                }
            }
            return true;
        }

        private bool SafeLaunch(Func<bool> func, bool exclude=false, byte[] swapbytes = null)
        {
            long lastCurrentIndex = currentIndex;
            int lastByteCount = currentBytes.Count;

            if(func())
            {
                if(swapbytes is not null)
                {
                    currentBytes.ClearAfter(lastByteCount);
                    currentBytes.AddRange(swapbytes);
                }
                if(exclude)
                {
                    currentBytes.ClearAfter(lastByteCount);
                }
                return true;
            }
            else
            {
                currentIndex = lastCurrentIndex;
                currentBytes.ClearAfter(lastByteCount);
                return false;
            }
        }
    }
}
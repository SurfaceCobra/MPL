using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLLib.ZipManage
{
    public class VirtualZip : IDisposable
    {
        private ZipArchive zipArchive { get; init; }
        private MemoryStream zipStream { get; init; }
        public VirtualZip()
        {
            zipStream = new MemoryStream();
            zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
        }
        public void AddFile(string path, byte[] value, CompressionLevel compLevel=CompressionLevel.Optimal)
        {
            var fileArchive = zipArchive.CreateEntry(path, compLevel);
            using (var entryStream = fileArchive.Open())
            using (var fileToCompressStream = new MemoryStream(value))
                fileToCompressStream.CopyTo(entryStream);
        }
        public void AddFile(string path, Stream stream, CompressionLevel compLevel = CompressionLevel.Optimal)
        {
            var fileArchive = zipArchive.CreateEntry(path, compLevel);
            using (var entryStream = fileArchive.Open())
            using (var fileToCompressStream = stream)
                fileToCompressStream.CopyTo(entryStream);
        }
        public void CreateFolder(string folderPath)
        {
            zipArchive.CreateEntry(folderPath, CompressionLevel.Optimal);
        }
        public void Dispose()
        {
            zipArchive.Dispose();
            zipStream.Dispose();
        }
        public void SaveAndDispose(Stream saveStream)
        {
            zipArchive.Dispose();
            zipStream.WriteTo(saveStream);
            zipStream.Dispose();
        }
        public void SaveAndDispose(string path)
        {
            using FileStream fs = File.OpenWrite(path);
            this.SaveAndDispose(fs);
        }
    }
}

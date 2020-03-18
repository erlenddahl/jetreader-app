using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using JetReader.Service;

namespace JetReader.DependencyService
{
    public class UWPFileService : FileService
    {
        public override async Task<long> GetFileSizeInBytes(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            var props = await file.GetBasicPropertiesAsync();
            return (long)props.Size;
        }

        public override async Task<Stream> LoadFileStreamAsync(string filePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            return await file.OpenStreamForReadAsync();
        }
    }
}

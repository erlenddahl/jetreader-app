using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Service
{
    public class FileService
    {

        public static string StorageFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string ToAbsolute(string path)
        {
            return Path.Combine(StorageFolder, path);
        }

        public async Task<string> ReadAllTextAsync(string filename)
        {
            return await Task.Run(() => File.ReadAllText(ToAbsolute(filename)));
        }

        public async Task WriteAllTextAsync(string filename, string contents)
        {
            await Task.Run(() => File.WriteAllText(ToAbsolute(filename), contents));
        }

        public async Task<string> CreateDirectoryAsync(string directory, bool clean = false)
        {
            directory = ToAbsolute(directory);
            await Task.Run(() =>
            {
                if (clean && Directory.Exists(directory))
                    Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            });
            return directory;
        }

        public async Task<string> WriteBytesAsync(string filePath, byte[] dataBytes)
        {
            return await Task.Run(() =>
            {
                filePath = ToAbsolute(filePath);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.WriteAllBytes(filePath, dataBytes);

                return filePath;
            });
        }

        public async Task<Stream> LoadFileStreamAsync(string filePath)
        {
            return await Task.Run(() => LoadFileStream(ToAbsolute(filePath)));
        }

        public virtual Stream LoadFileStream(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public async Task<bool> FileExists(string filename)
        {
            return await Task.Run(() => File.Exists(ToAbsolute(filename)));
        }

        public async Task DeleteFolder(string path)
        {
            var abs = ToAbsolute(path);
            if (!Directory.Exists(abs)) return;
            await Task.Run(() => Directory.Delete(abs, true));
        }

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Service {
    public interface IFileService {
        Task<bool> FileExists(string filename);
        Task DeleteFolder(string path);
        string StorageFolder { get; }
        Task<string> ReadAllTextAsync(string filename);
        Task WriteAllTextAsync(string filename, string contents);
        Task<string> CreateDirectoryAsync(string directory, bool clean = false);
        Task<string> WriteBytesAsync(string fileName, byte[] dataBytes);
        Task<Stream> LoadFileStreamAsync(string filePath);
    }
}

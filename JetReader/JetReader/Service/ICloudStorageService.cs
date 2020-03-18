using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Service {
    public interface ICloudStorageService {
        bool IsConnected();
        void SaveJson<T>(T json, string[] path);
        Task BackupFile(string file, string[] path);
        Task<T> LoadJson<T>(string[] path);
        Task<List<string>> GetFileList(string path, Func<string, bool> filter = null);
        Task<List<T>> LoadJsonList<T>(string[] path);
        void DeleteNode(string[] path);
        Task<bool> RestoreFile(string toPath, string fromPath);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.DependencyService;
using Windows.Storage;

namespace JetReader.UWP.DependencyService {
    public class FileHelper : IFileHelper {
        public string GetLocalFilePath(string filename) {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, filename);
        }
    }
}

using System.Diagnostics;
using System.Threading.Tasks;
using EbookReader.Service;
using SQLite;

namespace EbookReader.Model.Format
{
    public class BookData
    {
        [PrimaryKey]
        public string Id { get; set; }

        private string GetTempLocation()
        {
            return FileService.ToAbsolute(Id);
        }

        public async Task DeleteTempLocation(FileService fs)
        {
            await fs.DeleteFolder(GetTempLocation());
        }

        public async Task ExtractToTemp(FileService fs, Ebook book)
        {
            await fs.CreateDirectoryAsync(GetTempLocation());

            foreach (var (filename, filedata) in book.ExtractFiles)
            {
                var path = GetTempPath(filename);
                Debug.WriteLine("Exported file: " + path);
                await fs.WriteBytesAsync(path, filedata);
            }
        }

        public string GetTempPath(string filename)
        {
            return System.IO.Path.Combine(GetTempLocation(), filename.Replace("/", "_-_").Replace("\\", "_-_"));
        }

        public void PrintTempFiles()
        {
            foreach (var file in System.IO.Directory.GetFiles(GetTempLocation(), "*.*"))
                Debug.WriteLine(file);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Format;
using EbookReader.Service;
using Newtonsoft.Json;
using SQLite;

namespace EbookReader.Books
{
    public class BookInfo {
        [PrimaryKey]
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? BookmarksSyncLastChange { get; set; }
        public EbookFormat Format { get; set; }
        public DateTime? FinishedReading { get; set; }
        public string BookLocation { get; set; }
        public string CoverFilename { get; set; }

        [Ignore]
        public List<ChapterData> ChapterInfo { get; set; }
        public string ChapterDataJson {
            get => JsonConvert.SerializeObject(ChapterInfo);
            set => ChapterInfo = JsonConvert.DeserializeObject<List<ChapterData>>(value);
        }

        public int Spine { get; set; }
        public int SpinePosition { get; set; }

        [Ignore]
        public virtual Position Position {
            get => new Position(Spine, SpinePosition);
            set {
                Spine = value.Spine;
                SpinePosition = value.SpinePosition;
            }
        }
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
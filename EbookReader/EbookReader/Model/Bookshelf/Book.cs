using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Format;
using EbookReader.Service;
using SQLite;

namespace EbookReader.Model.Bookshelf {
    public class Book {
        [PrimaryKey]
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? BookmarksSyncLastChange { get; set; }
        public EbookFormat Format { get; set; }
        public DateTime? FinishedReading { get; set; }
        public string BookLocation { get; set; }
        public string CoverFilename { get; set; }

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

        public async Task CreateTempLocation(FileService fs)
        {
            await fs.CreateDirectoryAsync(GetTempLocation());
        }

        public async Task SaveToTempLocation(FileService fs, string filename, byte[] data)
        {
            await fs.WriteBytesAsync(System.IO.Path.Combine(GetTempLocation(), filename), data);
        }

        public string GetTempPath(string filename)
        {
            return System.IO.Path.Combine(GetTempLocation(), filename);
        }
    }
}
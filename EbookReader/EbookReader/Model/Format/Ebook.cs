using Autofac;
using EbookReader.BookLoaders;
using EbookReader.Books;
using EbookReader.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EbookReader.Model.Format
{
    public class Ebook
    {
        private BookInfo info;

        public BookInfo Info => info ?? (info = GenerateBookInfo());
        public string Path { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public List<EbookChapter> HtmlFiles { get; set; }
        public byte[] CoverData { get; set; }
        public string CoverFilename { get; set; }

        public EbookFormat Format { get; set; }
        public virtual IEnumerable<(string filename, byte[] filedata)> ExtractFiles { get { yield break; } }

        public Ebook(BookInfo info = null)
        {
            this.info = info;
        }

        private BookInfo GenerateBookInfo()
        {
            var fs = IocManager.Container.Resolve<FileService>();
            return new BookInfo
            {
                Id = fs.GetFileHash(Path).Result,
                BookFileSize = fs.GetFileSizeInBytes(Path).Result,
                Title = Title,
                Format = Format,
                BookLocation = Path,
                CoverFilename = CoverFilename,
                ChapterInfo = HtmlFiles.Select(p => new ChapterData(p)).ToList()
            };
        }

        public IEnumerable<(string title, string value)> GetInfo()
        {
            yield return ("Title", Title);
            yield return ("Author", Author);
            yield return ("Language", Language);
            yield return ("Description", Description);
            yield return ("", "");

            yield return ("Book file path", Path);
            yield return ("Book file size", (Info.BookFileSize / 1000d).ToString("n0") + " kB");
            yield return ("", "");

            yield return ("Words in book", Info.ChapterInfo.Sum(p => p.Words).ToString("n0"));
            yield return ("Characters in book", Info.ChapterInfo.Sum(p => p.Letters).ToString("n0"));
        }
    }
}

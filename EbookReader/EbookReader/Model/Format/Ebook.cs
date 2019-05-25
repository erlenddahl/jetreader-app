using Autofac;
using EbookReader.BookLoaders;
using EbookReader.Books;
using EbookReader.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EbookReader.Model.Format
{
    public class Ebook
    {
        public BookInfo Info { get; private set; }

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

        public Ebook(string path, BookInfo info = null)
        {
            Path = path;
            Info = info;
        }

        public async Task GenerateInfo()
        {
            if (Info == null)
                Info = new BookInfo
                {
                    Id = System.IO.Path.GetFileName(Path),
                    Title = Title,
                    Format = Format,
                    BookLocation = Path,
                    CoverFilename = CoverFilename
                };
            await Info.ProcessBook(this);
        }

        public async Task<List<(string title, string value)>> GetInfo()
        {
            await Info.WaitForProcessingToFinish();

            return new List<(string title, string value)>()
            {
                ("Title", Title),
                ("Author", Author),
                ("Language", Language),
                ("Description", Description),
                ("", ""),

                ("Book file path", Path),
                ("Book file size", (Info.BookFileSize / 1000d).ToString("n0") + " kB"),
                ("", ""),

                ("Words in book", Info.ChapterInfo.Sum(p => p.Words).ToString("n0")),
                ("Characters in book", Info.ChapterInfo.Sum(p => p.Letters).ToString("n0"))
            };
        }
    }
}

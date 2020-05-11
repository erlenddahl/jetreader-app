using Autofac;
using JetReader.BookLoaders;
using JetReader.Books;
using JetReader.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Extensions.TimeSpanExtensions;

namespace JetReader.Model.Format
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

            var secondsRead = Info.ReadStats.Dates.Sum(p => p.Seconds);
            var wordsRead = Info.ReadStats.Dates.Sum(p => p.Words);
            var totalWords = Info.ChapterInfo.Sum(p => p.Words);
            var wordsPerSecond = wordsRead / secondsRead;

            return new List<(string title, string value)>()
            {
                ("Description", Description),
                ("", ""),

                ("Book file path", Path),
                ("Book file size", (Info.BookFileSize / 1000d).ToString("n0") + " kB"),
                ("", ""),

                ("Words in book", totalWords.ToString("n0")),
                ("Characters in book", Info.ChapterInfo.Sum(p => p.Letters).ToString("n0")),
                ("", ""),

                ("Total reading time", TimeSpan.FromSeconds(secondsRead).ToShortPrettyFormat()),
                ("Total words read", $"{wordsRead:n0} ({(wordsRead / totalWords * 100):n1}%)"),
                ("Average reading speed", $"{(wordsPerSecond * 60):n0} words/min"),
                ("Estimated remaining reading time", TimeSpan.FromSeconds((totalWords - wordsRead)/wordsPerSecond).ToShortPrettyFormat()),
            };
        }
    }
}

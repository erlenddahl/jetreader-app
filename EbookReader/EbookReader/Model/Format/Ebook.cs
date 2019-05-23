using Autofac;
using EbookReader.BookLoaders;
using EbookReader.Books;
using EbookReader.Service;
using System;
using System.Collections.Generic;
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
            return new BookInfo
            {
                Id = IocManager.Container.Resolve<FileService>().GetFileHash(Path).Result,
                Title = Title,
                Format = Format,
                BookLocation = Path,
                CoverFilename = CoverFilename
            };
        }
    }
}

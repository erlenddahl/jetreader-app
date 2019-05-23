using EbookReader.BookLoaders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using EbookReader.Model.Bookshelf;

namespace EbookReader.Model.Format
{
    public class Ebook : BookData {
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

        public virtual Book ToBookshelf()
        {
            return new Book
            {
                Id = Id,
                Title = Title,
                Format = Format,
                BookLocation = Path,
                CoverFilename = CoverFilename
            };
        }
    }
}

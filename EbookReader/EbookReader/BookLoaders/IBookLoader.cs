using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Format;

namespace EbookReader.BookLoaders
{
    public interface IBookLoader
    {
        Task<Ebook> OpenBook(string filePath, string id = null);
        Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter);
    }
}

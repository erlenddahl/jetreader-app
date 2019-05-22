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
        Task<Ebook> OpenBook(string filePath);
        Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter);
    }
}

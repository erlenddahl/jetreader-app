﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using JetReader.Model.Format;

namespace JetReader.BookLoaders
{
    public interface IBookLoader
    {
        Task<Ebook> OpenBook(BookInfo book);
        Task<Ebook> OpenBook(string filePath, BookInfo book = null);
        Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter);
    }
}

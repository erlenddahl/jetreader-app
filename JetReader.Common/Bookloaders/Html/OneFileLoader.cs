﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using JetReader.Helpers;
using JetReader.Model.Format;
using JetReader.Service;
using EpubSharp;
using HtmlAgilityPack;

namespace JetReader.BookLoaders.Html
{
    public abstract class OneFileLoader : IBookLoader
    {

        protected string[] Extensions;

        protected EbookFormat EbookFormat;
        private FileService _fileService;

        public OneFileLoader(FileService fileService)
        {
            _fileService = fileService;
        }

        public virtual async Task<Ebook> OpenBook(string filePath, BookInfo info = null)
        {
            var title = Path.GetFileName(filePath);
            var data = await _fileService.ReadAllTextAsync(filePath);

            var book = new Ebook(filePath, info)
            {
                Title = title,
                HtmlFiles = new List<EbookChapter>() { new EbookChapter(title, data) },
            };
            book.GenerateInfo();

            return book;
        }

        public virtual async Task<Ebook> OpenBook(BookInfo info)
        {
            return await OpenBook(info.BookLocation, info);
        }

        public virtual async Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {
            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlHelper.StripHtmlTags(doc, new[] { "script", "style", "iframe" });

                return HtmlHelper.GetBody(doc).InnerHtml;
            });
        }
    }
}

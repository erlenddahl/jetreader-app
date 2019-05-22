using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.BookLoaders.Html;
using EbookReader.Model.EpubLoader;
using EbookReader.Model.Format;
using EbookReader.Service;

namespace EbookReader.BookLoaders.Txt
{
    public class TxtLoader : OneFileLoader
    {

        public TxtLoader(FileService fileService) : base(fileService)
        {
            Extensions = new[] { "txt" };
            EbookFormat = EbookFormat.Txt;
        }

        public override async Task<HtmlResult> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {
            html = $"<body><p>{html}</p></body>".Replace("\n", "</p><p>");
            return await base.PrepareHtml(html, book, chapter);
        }
    }
}
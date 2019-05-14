﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.EpubLoader;
using EbookReader.Model.Format;

namespace EbookReader.Service {
    public class TxtLoader : OneFileLoader {
        
        public TxtLoader(IFileService fileService) : base(fileService) {
            Extensions = new[] { "txt" };
            EbookFormat = EbookFormat.Txt;
        }

        public override async Task<HtmlResult> PrepareHtml(string html, Ebook book, File chapter) {
            html = $"<body><p>{html}</p></body>".Replace("\n","</p><p>");
            return await base.PrepareHtml(html, book, chapter);
        }
    }
}
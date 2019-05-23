using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using EbookReader.Books;
using EbookReader.Helpers;
using EbookReader.Model.Format;
using EbookReader.Service;
using EpubSharp;
using HtmlAgilityPack;

namespace EbookReader.BookLoaders.Epub
{
    public class EpubLoader : IBookLoader
    {
        private readonly FileService _fileService;

        public EpubLoader(FileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<Ebook> OpenBook(BookInfo info) {
            return await OpenBook(info.BookLocation, info);
        }

        public async Task<Ebook> OpenBook(string filePath, BookInfo info = null)
        {
            return await Task.Run(() => {
                var book = EpubReader.Read(_fileService.LoadFileStream(filePath), false);
                var epub = new EpubEbook(book);
                epub.Path = filePath;
                info = epub.Info; // Force generation
                return epub;
            });
        }

        public async Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {

            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlHelper.StripHtmlTags(doc, new[] { "iframe" });

                var data = ((EpubEbook) book);
                InlineCss(data, doc, chapter);
                InlineImages(data, doc, chapter);

                return HtmlHelper.GetBody(doc).InnerHtml;
            });
        }

        private void InlineCss(EpubEbook book, HtmlDocument doc, EbookChapter chapter)
        {
            var internalStyles = doc.DocumentNode.Descendants("style").ToList();
            var body = HtmlHelper.GetBody(doc);
            foreach (var s in internalStyles)
            {
                s.Remove();
                body.PrependChild(s);
            }

            var externalStyles = doc.DocumentNode
                .Descendants("link")
                .Where(p => p.Attributes["rel"].Value.ToLower() == "stylesheet")
                .Select(p => p.Attributes["href"].Value)
                .ToList();

            foreach (var s in externalStyles)
            {
                var style = book.Data.Resources.Css.FirstOrDefault(p => p.FileName == chapter.ResolveRelativePath(s));
                if (style != null)
                {
                    var css = style.TextContent;

                    foreach(var font in book.Data.Resources.Fonts.Union(book.Data.Resources.Other))
                    {
                        var fontPath = PathHelper.CombinePath(style.FileName, font.FileName);
                        var extractedFontPath = book.Info.GetTempPath(fontPath);
                        css = css.Replace(fontPath, extractedFontPath);
                    }

                    body.PrependChild(HtmlNode.CreateNode("<style>" + css + "</style>"));
                }
            }
        }

        /// <summary>
        /// Enumerates all img tags and svg.image tags and extracts image paths. Each image is
        /// converted to base64, and inserted directly into the HTML code.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="doc"></param>
        /// <param name="chapter"></param>
        /// <returns></returns>
        private void InlineImages(EpubEbook book, HtmlDocument doc, EbookChapter chapter) {

            var imageData = book.Data.Resources.Images.ToDictionary(k => k.FileName, v => v);

            // Find all img and image nodes, and extract their source path
            var images = doc.DocumentNode.Descendants("img")
                .Select(p => new { Node = p, Src = p.Attributes.FirstOrDefault(c => c.Name == "src") })
                .Union(doc.DocumentNode.Descendants("image")
                .Select(p => new { Node = p, Src = p.Attributes.FirstOrDefault(c => c.Name == "xlink:href") }))
                // Remove images without source
                .Where(p => p.Src != null)
                // Convert the relative HTML path to an absolute (inside local folder) path
                .Select(p => new
                {
                    p.Node,
                    p.Src,
                    Path = chapter.ResolveRelativePath(p.Src.Value)
                })
                // Group by image path to avoid loading and transferring the same image more than once
                .GroupBy(p=>p.Path);

            foreach (var image in images)
            {
                var imgData = imageData[image.Key];
                //var base64 = Base64Helper.Encode(imgData.Content);
                //var ctype = imgData.ContentType.ToString().ToLower().Replace("image", "image/"); // From enum "ImageJpeg" to "image/jpeg".

                foreach (var element in image)
                {
                    //element.Node.Attributes["src"].Value = $"data:{ctype};base64,{base64}";
                    var tempPath = book.Info.GetTempPath(imgData.FileName);
                    if(element.Node.Attributes.Contains("src"))
                        element.Node.Attributes["src"].Value = tempPath;
                    if(element.Node.Attributes.Contains("xlink:href"))
                        element.Node.Attributes["xlink:href"].Value = tempPath;
                }
            }
        }
    }
}

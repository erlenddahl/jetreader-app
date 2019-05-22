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
using EbookReader.Helpers;
using EbookReader.Model.Bookshelf;
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


        public async Task<Ebook> OpenBook(string filePath) {
            return await Task.Run(() => {
                var book = EpubReader.Read(_fileService.LoadFileStream(filePath), false);
                return new EpubEbook(filePath, book);
            });
        }

        public async Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {

            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                StripHtmlTags(doc);

                var data = ((EpubEbook) book).Data;
                InlineCss(data, doc);
                InlineImages(data, doc, chapter);

                //TODO: Handle book CSS
                return doc.DocumentNode.Descendants("body").First().InnerHtml;
            });
        }

        private void StripHtmlTags(HtmlDocument doc) {
            var tagsToRemove = new[] { "script", "style", "iframe" };
            var nodesToRemove = doc.DocumentNode
                .Descendants()
                .Where(o => tagsToRemove.Contains(o.Name))
                .ToList();

            foreach (var node in nodesToRemove) {
                node.Remove();
            }
        }

        private void InlineCss(EpubBook data, HtmlDocument doc)
        {
            var body = doc.DocumentNode.Descendants("body").First();
            var internalStyles = doc.DocumentNode.Descendants("style");
            foreach (var s in internalStyles)
            {
                s.Remove();
                body.AppendChild(s);
            }

            var externalStyles = doc.DocumentNode
                .Descendants("link")
                .Where(p => p.Attributes["rel"].Value.ToLower() == "stylesheet")
                .Select(p => p.Attributes["href"].Value)
                .ToList();

            foreach (var s in externalStyles)
            {
                var style = data.Resources.Css.FirstOrDefault(p => p.FileName.Contains(s.Replace("../", "")));
                if (style != null)
                    body.AppendChild(HtmlNode.CreateNode("<style>" + style.TextContent + "</style>"));
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
        private void InlineImages(EpubBook data, HtmlDocument doc, EbookChapter chapter) {

            var imageData = data.Resources.Images.ToDictionary(k => k.FileName, v => v);

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
                    Path = PathHelper.NormalizePath(PathHelper.CombinePath(chapter.Href, p.Src.Value).Replace("../", ""))
                })
                // Group by image path to avoid loading and transferring the same image more than once
                .GroupBy(p=>p.Path);

            foreach (var image in images)
            {
                var imgData = imageData[image.Key];
                var base64 = Base64Helper.Encode(imgData.Content);
                var ctype = imgData.ContentType.ToString().ToLower().Replace("image", "image/"); // From enum "ImageJpeg" to "image/jpeg".

                foreach (var element in image)
                    element.Node.Attributes["src"].Value = $"data:{ctype};base64,{base64}";
            }
        }
    }
}

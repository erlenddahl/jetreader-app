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
using EbookReader.Model.EpubLoader;
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

        public async Task<HtmlResult> PrepareHtml(string html, Ebook book, EbookChapter chapter) { 

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            StripHtmlTags(doc);

            var images = await GetImages(book, doc, chapter);

            //TODO: Handle book CSS
            var result = new Model.EpubLoader.HtmlResult {
                Html = doc.DocumentNode.Descendants("body").First().InnerHtml,
                Images = images,
                Title = doc.DocumentNode.Descendants("title").FirstOrDefault().InnerHtml
            };

            return result;
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

        /// <summary>
        /// Enumerates all img tags and svg.image tags and extracts image paths. Each tag is given an ID, and
        /// unique images are read as base64 encoded images encoded with the same id. The returned list of 
        /// Image models contains the base64 data and the id.
        /// </summary>
        /// <param name="epub"></param>
        /// <param name="doc"></param>
        /// <param name="chapter"></param>
        /// <returns></returns>
        private async Task<List<Model.EpubLoader.Base64Image>> GetImages(Ebook epub, HtmlDocument doc, EbookChapter chapter) {

            var data = ((EpubEbook)epub).Data;
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

            var models = new List<Base64Image>();
            foreach (var image in images)
            {
                // Create a new image model
                var img = new Base64Image
                {
                    Id = models.Count + 1,
                    FileName = Path.Combine(image.Key)
                };

                var imgData = imageData[img.FileName];
                var base64 = Base64Helper.Encode(imgData.Content);
                var ctype = imgData.ContentType.ToString().ToLower().Replace("image", "image/"); // From enum "ImageJpeg" to "image/jpeg".
                img.Data = $"data:{ctype};base64,{base64}";

                // Give each HTML node the current image id in order to let the JS reader load the
                // base64 image into the node after the HTML has been transferred.
                foreach (var element in image)
                    //element.Node.Attributes.Add(doc.CreateAttribute("data-js-ebook-image-id", img.Id.ToString()));
                    element.Node.Attributes["src"].Value = img.Data;

                models.Add(img);
            }

            return models;
        }
    }
}

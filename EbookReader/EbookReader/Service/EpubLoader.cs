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
using EbookReader.Model.Format.EpubFormat;
using EbookReader.Service.Epub;
using HtmlAgilityPack;

namespace EbookReader.Service {
    public class EpubLoader : IBookLoader {

        private readonly IFileService _fileService;

        public EpubLoader(IFileService fileService) {
            _fileService = fileService;
        }

        public Book CreateBookshelfBook(Ebook book) {
            return new Book {
                Title = book.Title,
                Path = book.Folder,
                Cover = book.Cover,
                Format = EbookFormat.Epub,
            };
        }

        public async Task<Ebook> GetBook(string filename, byte[] fileData, string bookId) {
            var folder = await LoadEpub(bookId, fileData);

            return await OpenBook(folder);
        }

        /// <summary>
        /// Reads an Epub book from the given Epub folder (a folder containing an unzipped epub file).
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public async Task<Ebook> OpenBook(string bookId)
        {
            var contentFileData = await _fileService.ReadAllTextAsync(await GetContentFilePath(bookId));

            var xml = XDocument.Parse(contentFileData);

            var package = xml.Root;

            var epubVersion = GetEpubVersion(package);

            var epubParser = IocManager.Container.ResolveKeyed<EpubParser>(epubVersion, new NamedParameter("package", package), new NamedParameter("path", bookId));

            var epub = new Ebook() {
                Title = epubParser.GetTitle(),
                Author = epubParser.GetAuthor(),
                Description = epubParser.GetDescription(),
                Language = epubParser.GetLanguage(),
                Spines = epubParser.GetSpines(),
                Files = epubParser.GetFiles(),
                Folder = bookId,
                Navigation = await epubParser.GetNavigation(),
                Cover = epubParser.GetCover()
            };

            return epub;
        }

        public async Task<string> GetChapter(Ebook epub, Spine chapter) {
            var filename = epub.Files.First(o => o.Id == chapter.Idref);
            return await _fileService.ReadAllTextAsync(Path.Combine(epub.Folder, filename.Href));
        }

        public async Task<Model.EpubLoader.HtmlResult> PrepareHtml(string html, Ebook epub, Model.Format.File chapter) {

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            StripHtmlTags(doc);

            var images = await GetImages(epub, doc, chapter);

            var result = new Model.EpubLoader.HtmlResult {
                Html = doc.DocumentNode.Descendants("body").First().InnerHtml,
                Images = images
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
        private async Task<List<Model.EpubLoader.Image>> GetImages(Ebook epub, HtmlDocument doc, Model.Format.File chapter) {

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
                // Group by image path to avoid loading and transfering the same image more than once
                .GroupBy(p=>p.Path);

            var models = new List<Model.EpubLoader.Image>();
            foreach (var image in images)
            {
                // Create a new image model
                var img = new Model.EpubLoader.Image
                {
                    Id = models.Count + 1,
                    FileName = Path.Combine(epub.Folder, image.Key)
                };

                // Load the base64 image
                using (var stream = await _fileService.LoadFileStreamAsync(img.FileName))
                {
                    var base64 = Base64Helper.GetFileBase64(stream);
                    img.Data = $"data:image/{Path.GetExtension(img.FileName).Replace(".", "")};base64,{base64}";
                }

                // Give each HTML node the current image id in order to let the JS reader load the
                // base64 image into the node after the HTML has been transferred.
                foreach (var element in image)
                    element.Node.Attributes.Add(doc.CreateAttribute("data-js-ebook-image-id", img.Id.ToString()));

                models.Add(img);
            }

            return models;
        }

        private EpubVersion GetEpubVersion(XElement package) {
            var version = package.Attributes().First(o => o.Name.LocalName == "version").Value;
            return EpubVersionHelper.ParseVersion(version);
        }

        /// <summary>
        /// Reads the Epub's meta data and extracts the filename of the content file (for example "content.opf").
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        private async Task<string> GetContentFilePath(string bookId)
        {
            var containerFileContent = await _fileService.ReadAllTextAsync(Path.Combine(bookId, "META-INF", "container.xml"));
            var xmlContainer = XDocument.Parse(containerFileContent);
            var contentFilePath = xmlContainer.Root
                .Descendants()
                .First(o => o.Name.LocalName == "rootfiles")
                .Descendants()
                .First(o => o.Name.LocalName == "rootfile")
                .Attributes()
                .First(o => o.Name.LocalName == "full-path")
                .Value;
            return Path.Combine(bookId, contentFilePath);
        }

        /// <summary>
        /// Stores the fileData byte array as a zip file, and unpacks it in a local folder using the bookId as name.
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        private async Task<string> LoadEpub(string bookId, byte[] fileData) {
            var folder = await _fileService.CreateDirectoryAsync(bookId, true);

            var zipFile = await _fileService.WriteBytesAsync(Path.Combine(bookId, "temp.zip"), fileData);

            ZipFile.ExtractToDirectory(zipFile, folder);

            var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
                Debug.WriteLine(file);

            return bookId;
        }
    }
}

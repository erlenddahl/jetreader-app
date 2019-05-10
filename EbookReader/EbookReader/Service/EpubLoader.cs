﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using EbookReader.Helpers;
using EbookReader.Model.Bookshelf;
using EbookReader.Model.Format.EpubFormat;
using EbookReader.Service.Epub;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.Zip;
using PCLStorage;

namespace EbookReader.Service {
    public class EpubLoader : IBookLoader {

        private IFileService _fileService;

        public EpubLoader(IFileService fileService) {
            _fileService = fileService;
        }

        public Book CreateBookshelfBook(Model.Format.Ebook book) {
            return new Book {
                Title = book.Title,
                Path = book.Folder,
                Cover = book.Cover,
                Format = Model.Format.EbookFormat.Epub,
            };
        }

        public async Task<Model.Format.Ebook> GetBook(string filename, byte[] filedata, string bookID) {
            var folder = await LoadEpub(bookID, filedata);

            return await OpenBook(folder);
        }

        public async Task<Model.Format.Ebook> OpenBook(string path) {
            var epubFolder = await FileSystem.Current.LocalStorage.GetFolderAsync(path);

            var contentFilePath = await GetContentFilePath(epubFolder);
            var contentFilePathParts = contentFilePath.Split('/');
            var contentBasePath = string.Join("/", contentFilePathParts.Take(contentFilePathParts.Length - 1));
            if (!string.IsNullOrEmpty(contentBasePath)) {
                contentBasePath += "/";
            }

            var contentFileData = await _fileService.ReadFileData(contentFilePath, epubFolder);

            var xml = XDocument.Parse(contentFileData);

            var package = xml.Root;

            var epubVersion = GetEpubVersion(package);

            var epubParser = IocManager.Container.ResolveKeyed<EpubParser>(
                epubVersion,
                new NamedParameter("package", package),
                new NamedParameter("folder", epubFolder),
                new NamedParameter("contentBasePath", contentBasePath)
            );

            var epub = new Model.Format.Ebook() {
                Title = epubParser.GetTitle(),
                Author = epubParser.GetAuthor(),
                Description = epubParser.GetDescription(),
                Language = epubParser.GetLanguage(),
                Spines = epubParser.GetSpines(),
                Files = epubParser.GetFiles(),
                Folder = path,
                ContentBasePath = contentBasePath,
                Navigation = await epubParser.GetNavigation(),
                Cover = epubParser.GetCover()
            };

            return epub;
        }

        public async Task<string> GetChapter(Model.Format.Ebook epub, Model.Format.Spine chapter) {
            var filename = epub.Files.Where(o => o.Id == chapter.Idref).First();
            var folder = await FileSystem.Current.LocalStorage.GetFolderAsync(epub.Folder);
            return await _fileService.ReadFileData($"{epub.ContentBasePath}{filename.Href}", folder);
        }

        public async Task<Model.EpubLoader.HtmlResult> PrepareHTML(string html, Model.Format.Ebook epub, Model.Format.File chapter) {

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            StripHtmlTags(doc);

            var images = await PrepareHtmlImages(doc, epub, chapter);

            var result = new Model.EpubLoader.HtmlResult {
                Html = doc.DocumentNode.Descendants("body").First().InnerHtml,
                Images = images,
            };

            return result;
        }

        private void StripHtmlTags(HtmlDocument doc) {
            var tagsToRemove = new string[] { "script", "style", "iframe" };
            var nodesToRemove = doc.DocumentNode
                .Descendants()
                .Where(o => tagsToRemove.Contains(o.Name))
                .ToList();

            foreach (var node in nodesToRemove) {
                node.Remove();
            }
        }

        private async Task<List<Model.EpubLoader.Image>> PrepareHtmlImages(HtmlDocument doc, Model.Format.Ebook epub, Model.Format.File chapter) {
            var imagesModel = GetImages(doc, chapter);

            return await ReplaceImagesWithBase64(imagesModel, epub);
        }

        private async Task<List<Model.EpubLoader.Image>> ReplaceImagesWithBase64(List<Model.EpubLoader.Image> imagesModel, Model.Format.Ebook epub) {
            var epubFolder = await FileSystem.Current.LocalStorage.GetFolderAsync(epub.Folder);

            foreach (var imageModel in imagesModel) {
                var extension = imageModel.FileName.Split('.').Last();

                var fileName = PathHelper.NormalizePath($"{epub.ContentBasePath}/{imageModel.FileName.Replace("../", "")}");
                var file = await _fileService.OpenFile(fileName, epubFolder);

                using (var stream = await file.OpenAsync(PCLStorage.FileAccess.Read)) {
                    var base64 = Base64Helper.GetFileBase64(stream);

                    imageModel.Data = $"data:image/{extension};base64,{base64}";
                }

            }

            return imagesModel;
        }

        private List<Model.EpubLoader.Image> GetImages(HtmlDocument doc, Model.Format.File chapter) {
            var images = doc.DocumentNode.Descendants("img").ToList();
            var imagesModel = new List<Model.EpubLoader.Image>();

            var cnt = 1;
            foreach (var image in images) {
                var srcAttribute = image.Attributes.FirstOrDefault(o => o.Name == "src");

                if (srcAttribute != null) {
                    int id;

                    var existingImageModel = imagesModel.FirstOrDefault(o => o.FileName == srcAttribute.Value);

                    if (existingImageModel != null) {
                        id = existingImageModel.ID;
                    } else {
                        id = cnt;
                        var path = PathHelper.CombinePath(chapter.Href, srcAttribute.Value);
                        imagesModel.Add(new Model.EpubLoader.Image {
                            ID = id,
                            FileName = path,
                        });

                        cnt++;
                    }

                    image.Attributes.Add(doc.CreateAttribute("data-js-ebook-image-id", id.ToString()));
                }
            }

            return imagesModel;
        }

        private EpubVersion GetEpubVersion(XElement package) {
            var version = package.Attributes().First(o => o.Name.LocalName == "version").Value;
            return EpubVersionHelper.ParseVersion(version);
        }

        private async Task<string> GetContentFilePath(IFolder epubFolder) {
            var containerFile = await _fileService.OpenFile("META-INF/container.xml", epubFolder);
            var containerFileContent = await containerFile.ReadAllTextAsync();
            var xmlContainer = XDocument.Parse(containerFileContent);
            var contentFilePath = xmlContainer.Root
                .Descendants()
                .First(o => o.Name.LocalName == "rootfiles")
                .Descendants()
                .First(o => o.Name.LocalName == "rootfile")
                .Attributes()
                .First(o => o.Name.LocalName == "full-path")
                .Value;
            return contentFilePath;
        }

        private async Task<string> LoadEpub(string bookID, byte[] filedata) {
            var rootFolder = FileSystem.Current.LocalStorage;
            var folder = await rootFolder.CreateFolderAsync(bookID, CreationCollisionOption.ReplaceExisting);
            var file = await folder.CreateFileAsync("temp.zip", CreationCollisionOption.OpenIfExists);

            using (Stream stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite)) {
                await stream.WriteAsync(filedata, 0, filedata.Length);
                using (var zf = new ZipFile(stream)) {
                    foreach (ZipEntry zipEntry in zf) {

                        if (zipEntry.IsFile) {
                            var zipEntryStream = zf.GetInputStream(zipEntry);

                            var name = _fileService.GetLocalFileName(zipEntry.Name);

                            var fileFolder = await _fileService.GetFileFolder(zipEntry.Name, folder);

                            IFile zipEntryFile = await fileFolder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
                            var str = zf.GetInputStream(zipEntry);
                            using (Stream outPutFileStream = await zipEntryFile.OpenAsync(PCLStorage.FileAccess.ReadAndWrite)) {
                                await str.CopyToAsync(outPutFileStream);
                            }
                        }
                    }
                }
            }

            return folder.Name;
        }
    }
}

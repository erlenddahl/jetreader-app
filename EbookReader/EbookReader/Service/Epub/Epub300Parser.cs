using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EbookReader.Model.Navigation;

namespace EbookReader.Service.Epub {
    public class Epub300Parser : EpubParser {
        readonly IFileService _fileService;

        public Epub300Parser(IFileService fileService, XElement package, string path) : base(package, path) {
            _fileService = fileService;
        }

        public override async Task<List<Item>> GetNavigation() {

            var navigation = new List<Item>();
            var tocFilename = GetAttributeOnElementWithAttributeValue(GetManifest(), "href", "properties", "nav", "item");

            if (!string.IsNullOrEmpty(tocFilename))
            {
                var tocFileData = await _fileService.ReadAllTextAsync(Path.Combine(BookPath, tocFilename));
                var xmlContainer = XDocument.Parse(tocFileData);

                var navItem = xmlContainer.Root.Descendants()
                    .FirstOrDefault(o => o.Name.LocalName == "nav" && o.Attributes().Any(i => i.Name.LocalName == "type" && i.Value == "toc"));

                if (navItem != null) {
                    var olItem = navItem.Elements().FirstOrDefault(o => o.Name.LocalName == "ol");

                    if (olItem != null) {
                        foreach (var item in olItem.Elements().Where(o => o.Name.LocalName == "li")) {
                            var a = item.Elements().FirstOrDefault(o => o.Name.LocalName == "a");
                            if (a != null) {
                                var href = a.Attributes().FirstOrDefault(o => o.Name.LocalName == "href");

                                if (href != null) {
                                    navigation.Add(new Item {
                                        Id = href.Value,
                                        Title = a.Value
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return navigation;
        }

        public override string GetCover() {
            var cover = string.Empty;

            var href = GetAttributeOnElementWithAttributeValue(GetManifest(), "href", "properties", "cover-image", "item");

            if (!string.IsNullOrEmpty(href)) {
                cover = Path.Combine(BookPath, href);
            }

            return cover;
        }
    }
}

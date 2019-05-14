using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EbookReader.Service.Epub {
    public abstract class EpubParser {

        protected XElement Package { get; set; }
        protected string BookPath { get; set; }

        public EpubParser(XElement package, string path) {
            Package = package;
            BookPath = path;
        }

        public virtual string GetTitle() {
            return GetMandatoryElementValue("title", GetMetadata().Descendants());
        }

        public virtual string GetLanguage() {
            return GetMandatoryElementValue("language", GetMetadata().Descendants());
        }

        public virtual string GetAuthor() {
            return GetOptionalElementValue("creator", GetMetadata().Descendants());
        }

        public virtual string GetDescription() {
            return GetOptionalElementValue("description", GetMetadata().Descendants());
        }

        public virtual List<Model.Format.Spine> GetSpines() {
            return GetSpine()
                .Descendants()
                .Where(o => o.Name.LocalName == "itemref")
                .Select(o => new Model.Format.Spine {
                    Idref = o.Attributes().First(i => i.Name.LocalName == "idref").Value
                })
                .ToList();
        }

        public virtual IEnumerable<Model.Format.File> GetFiles() {
            return GetManifest()
                .Descendants()
                .Where(o => o.Name.LocalName == "item")
                .Select(o => new Model.Format.File {
                    Id = o.Attributes().First(i => i.Name.LocalName == "id").Value,
                    Href = o.Attributes().First(i => i.Name.LocalName == "href").Value,
                    MediaType = o.Attributes().First(i => i.Name.LocalName == "media-type").Value
                })
                .ToList();
        }

        public abstract Task<List<Model.Navigation.Item>> GetNavigation();

        public abstract string GetCover();

        protected XElement GetMetadata() {
            return Package.Descendants().First(o => o.Name.LocalName == "metadata");
        }

        protected XElement GetManifest() {
            return Package.Descendants().First(o => o.Name.LocalName == "manifest");
        }

        private XElement GetSpine() {
            return Package.Descendants().First(o => o.Name.LocalName == "spine");
        }

        private string GetMandatoryElementValue(string localName, IEnumerable<XElement> elements) {
            return elements.First(o => o.Name.LocalName == localName).Value;
        }

        private string GetOptionalElementValue(string localName, IEnumerable<XElement> elements) {
            var element = elements.FirstOrDefault(o => o.Name.LocalName == localName);
            return element != null ? element.Value : string.Empty;
        }

        protected string GetAttributeOnElementWithAttributeValue(XElement parent, string attributeName, string attributeFilterName, string attributeFilterValue, string elementName = "") {
            return parent
                .Elements()
                .Where(o => string.IsNullOrEmpty(elementName) || o.Name.LocalName == elementName)
                .Where(o => o.Attributes().Any(i => i.Name.LocalName == attributeFilterName && i.Value == attributeFilterValue))
                .Select(o => o.Attributes().First(i => i.Name.LocalName == attributeName).Value)
                .FirstOrDefault();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PCLStorage;

namespace EbookReader.Service.Epub {
    public abstract class EpubParser {

        protected XElement Package { get; set; }
        protected IFolder Folder { get; set; }
        protected string ContentBasePath { get; set; }

        public EpubParser(XElement package, IFolder folder, string contentBasePath) {
            Package = package;
            Folder = folder;
            ContentBasePath = contentBasePath;
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
                    Idref = o.Attributes().Where(i => i.Name.LocalName == "idref").First().Value
                })
                .ToList();
        }

        public virtual IEnumerable<Model.Format.File> GetFiles() {
            return GetManifest()
                .Descendants()
                .Where(o => o.Name.LocalName == "item")
                .Select(o => new Model.Format.File {
                    Id = o.Attributes().Where(i => i.Name.LocalName == "id").First().Value,
                    Href = o.Attributes().Where(i => i.Name.LocalName == "href").First().Value,
                    MediaType = o.Attributes().Where(i => i.Name.LocalName == "media-type").First().Value
                })
                .ToList();
        }

        public abstract Task<List<Model.Navigation.Item>> GetNavigation();

        public abstract string GetCover();

        protected XElement GetMetadata() {
            return Package.Descendants().Where(o => o.Name.LocalName == "metadata").First();
        }

        protected XElement GetManifest() {
            return Package.Descendants().Where(o => o.Name.LocalName == "manifest").First();
        }

        private XElement GetSpine() {
            return Package.Descendants().Where(o => o.Name.LocalName == "spine").First();
        }

        private string GetMandatoryElementValue(string localName, IEnumerable<XElement> elements) {
            return elements.Where(o => o.Name.LocalName == localName).First().Value;
        }

        private string GetOptionalElementValue(string localName, IEnumerable<XElement> elements) {
            var element = elements.Where(o => o.Name.LocalName == localName).FirstOrDefault();
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

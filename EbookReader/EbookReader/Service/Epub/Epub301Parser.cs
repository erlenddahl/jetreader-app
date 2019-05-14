using System.Xml.Linq;

namespace EbookReader.Service.Epub {
    public class Epub301Parser : Epub300Parser {
        public Epub301Parser(IFileService fileService, XElement package,string path) : base(fileService, package, path) {
        }
    }
}

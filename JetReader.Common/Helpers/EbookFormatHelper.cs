using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.BookLoaders;
using JetReader.Exceptions;
using JetReader.Model.Format;
using JetReader.Service;

namespace JetReader.Helpers {
    public static class EbookFormatHelper {

        public static IBookLoader GetBookLoader(string filename) {
            if (string.IsNullOrEmpty(filename)) throw new UnknownFileFormatException(filename);

            var ebookFormat = EbookFormat.Epub;

            if (filename.EndsWith(".txt")) {
                ebookFormat = EbookFormat.Txt;
            }

            if (filename.EndsWith(".html") || filename.EndsWith(".htm") || filename.EndsWith(".xhtml") || filename.EndsWith(".xhtm")) {
                ebookFormat = EbookFormat.Html;
            }

            return GetBookLoader(ebookFormat);

        }

        public static IBookLoader GetBookLoader(EbookFormat format) {
            return IocManager.Container.ResolveKeyed<IBookLoader>(format);
        }

    }
}

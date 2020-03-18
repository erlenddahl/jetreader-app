using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JetReader.Helpers {
    public static class PathHelper {
        public static string CombinePath(string path1, string path2) {
            var dummyDriveLetter = "C:/";
            var absolutePath1 = dummyDriveLetter + path1;

            var path1Uri = new Uri(absolutePath1, UriKind.Absolute);
            var path2Uri = new Uri(path2, UriKind.Relative);
            var diff = new Uri(path1Uri, path2Uri);
            var newpath = diff.OriginalString.Replace(dummyDriveLetter, "");
            if (newpath.StartsWith("/")) newpath = newpath.Substring(1);
            return newpath;
        }
    }
}

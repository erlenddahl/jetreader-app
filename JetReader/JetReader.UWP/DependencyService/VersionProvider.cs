using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.DependencyService;
using Windows.ApplicationModel;

namespace JetReader.UWP.DependencyService {
    public class VersionProvider : IVersionProvider {

        public string AppVersion {
            get {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
    }
}

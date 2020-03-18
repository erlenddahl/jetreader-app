using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Model {
    public class OAuth2RequestData {
        public string Provider { get; set; }
        public string ClientId { get; set; }
        public string Scope { get; set; }
        public string AuthorizeUrl { get; set; }
        public string RedirectUrl { get; set; }
    }
}

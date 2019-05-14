using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.DependencyService;

namespace EbookReader.UWP.DependencyService {
    public class CryptoService : ICryptoService {
        public string GetMd5(byte[] bytes) {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                var hashBytes = md5.ComputeHash(bytes);

                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}

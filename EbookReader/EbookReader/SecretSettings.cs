﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader {
    public static partial class SecretSettings {
#if DEBUG
        public static class Synchronization {
            public static class Firebase {
                public static string BaseUrl = "https://ebook-reader-b6053.firebaseio.com/";
                public static string ApiKey = "AIzaSyA4TOO3_Pa1kb_s6zjBMqpehPLrTk8SrLA";
            }

            public static class Dropbox {
                public static string ClientID = "wk719mekght88r6";
            }
        }

        public static class AppCenter {
            public static string Android = "3409ff3e-0819-467d-b9a0-69f611f33bcb";
            public static string UWP = "737f997b-2a89-441d-8248-7f1f1fc889e3";
        }
#endif
    }
}
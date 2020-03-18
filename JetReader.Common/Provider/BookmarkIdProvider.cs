﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Provider {
    public static class BookmarkIdProvider {
        public static long Id => (long)(DateTime.UtcNow - new DateTime(2017, 1, 1, 0, 0, 0)).TotalSeconds;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Model.WebViewMessages {
    public class PageChange
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int Position { get; set; }
    }
}

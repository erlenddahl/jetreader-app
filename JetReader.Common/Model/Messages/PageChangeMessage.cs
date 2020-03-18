﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Model.Messages {
    public class PageChangeMessage {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int Position { get; set; }
    }
}

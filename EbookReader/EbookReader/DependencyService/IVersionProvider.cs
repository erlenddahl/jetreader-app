﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.DependencyService {
    public interface IVersionProvider {
        string AppVersion { get; }
    }
}

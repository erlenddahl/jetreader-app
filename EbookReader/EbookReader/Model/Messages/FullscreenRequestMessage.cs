using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.Messages {
    public class FullscreenRequestMessage {

        public bool? Fullscreen { get; private set; }
        public string Caller { get; }

        public FullscreenRequestMessage(bool? setFullscreen, [CallerMemberName]string callerName = "") {
            Fullscreen = setFullscreen;
            Caller = callerName;
        }
    }
}

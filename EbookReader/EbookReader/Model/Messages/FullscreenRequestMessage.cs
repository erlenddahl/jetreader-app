using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.Messages {
    public class FullscreenRequestMessage {

        public bool Fullscreen => _setFullscreen && UserSettings.Reader.Fullscreen;
        public string Caller { get; }

        readonly bool _setFullscreen;

        public FullscreenRequestMessage(bool setFullscreen, [CallerMemberName]string callerName = "") {
            _setFullscreen = setFullscreen;
            Caller = callerName;
        }
    }
}

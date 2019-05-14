using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.Messages {
    public class FullscreenRequestMessage {

        public bool Fullscreen => _setFullscreen && UserSettings.Reader.Fullscreen;

        readonly bool _setFullscreen;

        public FullscreenRequestMessage(bool setFullscreen) {
            this._setFullscreen = setFullscreen;
        }
    }
}

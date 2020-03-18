using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Model.Messages {
    public class FullscreenRequestMessage {

        /// <summary>
        /// If it has a value, fullscreen will be set to this value.
        /// If set to null, fullscreen will be toggled.
        /// </summary>
        public bool? Fullscreen { get; private set; }

        public string Caller { get; }

        public FullscreenRequestMessage(bool? setFullscreen, [CallerMemberName]string callerName = "") {
            Fullscreen = setFullscreen;
            Caller = callerName;
        }
    }
}

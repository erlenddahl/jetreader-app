using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Provider;
using Xamarin.Forms;

namespace EbookReader.Model.View {
    public class SettingsReaderVm {

        public ScrollSpeedVm ScrollSpeed { get; set; } = new ScrollSpeedVm();
        public FullscreenVm Fullscreen { get; set; } = new FullscreenVm();
    }
}

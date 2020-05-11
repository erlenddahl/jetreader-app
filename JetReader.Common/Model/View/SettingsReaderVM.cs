using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetReader.Provider;
using Xamarin.Forms;

namespace JetReader.Model.View {
    public class SettingsReaderVm {
        public ScrollSpeedVm ScrollSpeed { get; set; } = new ScrollSpeedVm();
        public FontSizeVm FontSize { get; set; } = new FontSizeVm();
        public MarginVm Margins { get; set; } = new MarginVm();
        public FullscreenVm Fullscreen { get; set; } = new FullscreenVm();
    }
}

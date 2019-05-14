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

        public FontSizeVm FontSize { get; set; } = new FontSizeVm();

        public MarginVm Margin { get; set; } = new MarginVm();

        public ScrollSpeedVm ScrollSpeed { get; set; } = new ScrollSpeedVm();

        public NightModeVm NightMode { get; set; } = new NightModeVm();

        public FullscreenVm Fullscreen { get; set; } = new FullscreenVm();
    }
}

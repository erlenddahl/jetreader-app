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
    public class SettingsReaderVM {

        public FontSizeVM FontSize { get; set; } = new FontSizeVM();

        public MarginVM Margin { get; set; } = new MarginVM();

        public ScrollSpeedVM ScrollSpeed { get; set; } = new ScrollSpeedVM();

        public NightModeVM NightMode { get; set; } = new NightModeVM();

        public FullscreenVM Fullscreen { get; set; } = new FullscreenVM();
    }
}

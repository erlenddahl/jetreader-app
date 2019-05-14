using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Provider;

namespace EbookReader.Model.View {
    public class QuickPanelSettingsVM
    {
        public FontSizeVM FontSize { get; set; } = new FontSizeVM();
        public MarginVM Margin { get; set; } = new MarginVM();
        public NightModeVM NightMode { get; set; } = new NightModeVM();

    }
}

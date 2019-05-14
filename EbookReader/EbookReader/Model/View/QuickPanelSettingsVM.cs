using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Provider;

namespace EbookReader.Model.View {
    public class QuickPanelSettingsVm
    {
        public FontSizeVm FontSize { get; set; } = new FontSizeVm();
        public MarginVm Margin { get; set; } = new MarginVm();
        public NightModeVm NightMode { get; set; } = new NightModeVm();

    }
}

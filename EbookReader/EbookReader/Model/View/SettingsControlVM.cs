using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.View {
    public class SettingsControlVm : BaseVm {

        public PanBrightnessChangeVm PanBrightnessChange { get; set; }
        public VolumeButtonVm VolumeButton { get; set; }

        public SettingsControlVm() {
            PanBrightnessChange = new PanBrightnessChangeVm();
            VolumeButton = new VolumeButtonVm();
        }

        public bool ClickEverywhere {
            get => UserSettings.Control.ClickEverywhere;
            set {
                if (UserSettings.Control.ClickEverywhere == value)
                    return;

                UserSettings.Control.ClickEverywhere = value;
                OnPropertyChanged();
            }
        }

        public bool DoubleSwipe {
            get => UserSettings.Control.DoubleSwipe;
            set {
                if (UserSettings.Control.DoubleSwipe == value)
                    return;

                UserSettings.Control.DoubleSwipe = value;
                OnPropertyChanged();
            }
        }
    }
}

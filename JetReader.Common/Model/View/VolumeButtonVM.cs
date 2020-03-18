using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace JetReader.Model.View {
    public class VolumeButtonVm : BaseVm {
        public bool Show => Device.RuntimePlatform == Device.Android;

        public bool Enabled {
            get => UserSettings.Control.VolumeButtons;
            set {
                if (UserSettings.Control.VolumeButtons == value)
                    return;

                UserSettings.Control.VolumeButtons = value;
                OnPropertyChanged();
            }
        }
    }
}

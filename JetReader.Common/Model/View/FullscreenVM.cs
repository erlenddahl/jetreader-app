using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace JetReader.Model.View {
    public class FullscreenVm : BaseVm {

        public bool Enabled {
            get => UserSettings.Reader.Fullscreen;
            set {
                if (UserSettings.Reader.Fullscreen == value)
                    return;

                UserSettings.Reader.Fullscreen = value;
                OnPropertyChanged();
            }
        }
    }
}

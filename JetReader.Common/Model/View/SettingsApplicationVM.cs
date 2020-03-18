using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;

namespace JetReader.Model.View {
    public class SettingsApplicationVm : BaseVm {
        public bool AnalyticsAgreement {
            get => UserSettings.AnalyticsAgreement;
            set {
                if (UserSettings.AnalyticsAgreement == value)
                    return;

                UserSettings.AnalyticsAgreement = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenUrlCommand { get; set; }

        public SettingsApplicationVm() {
            OpenUrlCommand = new Command(OpenUrl);
        }

        private void OpenUrl(object url) {
            if (url != null) {
                try {
                    var uri = new Uri(url.ToString());
                    Device.OpenUri(uri);
                } catch (Exception e) {
                    Crashes.TrackError(e, new Dictionary<string, string> {
                        {"Url", url.ToString() }
                    });
                }
            }
        }
    }
}
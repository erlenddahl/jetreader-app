using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.DependencyService;
using EbookReader.Model.Messages;
using EbookReader.Service;
using Xamarin.Forms;

namespace EbookReader.Model.View {
    public class InfoPanelVm : BaseVm {
        readonly IBatteryProvider _batteryProvider;

        string _pages;

        public string Pages {
            get => _pages;
            set {
                _pages = value;
                OnPropertyChanged();
            }
        }

        string _clock;

        public string Clock {
            get => _clock;
            set {
                _clock = value;
                OnPropertyChanged();
            }
        }

        string _batteryIcon;

        public string BatteryIcon {
            get => _batteryIcon;
            set {
                _batteryIcon = value;
                OnPropertyChanged();
            }
        }

        public bool ShowBattery => Device.RuntimePlatform == Device.Android;

        public string TextColor => UserSettings.Reader.NightMode ? "#eff2f7" : "#000000";

        public InfoPanelVm() {
            IocManager.Container.Resolve<IMessageBus>().Subscribe<PageChangeMessage>(HandlePageChange);
            IocManager.Container.Resolve<IMessageBus>().Subscribe<BatteryChangeMessage>(HandleBatteryChange);
            _batteryProvider = IocManager.Container.Resolve<IBatteryProvider>();

            SetClock();
            SetBattery();

            Device.StartTimer(new TimeSpan(0, 0, 10), () => {
                SetClock();

                return true;
            });
        }

        private void HandlePageChange(PageChangeMessage msg) {
            Pages = $"{msg.CurrentPage} / {msg.TotalPages}";
        }

        private void HandleBatteryChange(BatteryChangeMessage msg) {
            SetBattery();
        }

        private void SetClock() {
            Clock = DateTime.Now.ToString("HH:mm");
        }

        private void SetBattery() {
            var percent = _batteryProvider.RemainingChargePercent;

            var icon = string.Empty;
            if (percent < 5) {
                icon = "empty_battery";
            } else if (percent <= 30) {
                icon = "low_battery";
            } else if (percent <= 55) {
                icon = "half_battery";
            } else if (percent <= 85) {
                icon = "battery_almost_full";
            } else {
                icon = "full_battery";
            }

            if (UserSettings.Reader.NightMode) {
                icon += "_white";
            }

            icon += ".png";

            if (BatteryIcon != icon) {
                BatteryIcon = icon;
            }
        }
    }
}

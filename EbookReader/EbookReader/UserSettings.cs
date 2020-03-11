using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.Config.CommandGrid;
using EbookReader.DependencyService;
using EbookReader.Provider;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace EbookReader {
    public static class UserSettings {
        private static ISettings AppSettings => CrossSettings.Current;

        public static bool FirstRun {
            get => AppSettings.GetValueOrDefault(CreateKey(nameof(FirstRun)), true);
            set => AppSettings.AddOrUpdateValue(CreateKey(nameof(FirstRun)), value);
        }

        public static bool AnalyticsAgreement {
            get => AppSettings.GetValueOrDefault(CreateKey(nameof(AnalyticsAgreement)), false);
            set {
                SetAnalytics(value);
                AppSettings.AddOrUpdateValue(CreateKey(nameof(AnalyticsAgreement)), value);
            }
        }

        private static async void SetAnalytics(bool enabled) {
            await Analytics.SetEnabledAsync(enabled);
        }

        public static class Reader {
            private static readonly double FontSizeDefault = Device.RuntimePlatform == Device.Android ? 20 : 40;
            private const int MarginDefault = 30;
            private const int ScrollSpeedDefault = 200;

            public static bool OpenPreviousBookOnLaunch
            {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(OpenPreviousBookOnLaunch)), true);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(OpenPreviousBookOnLaunch)), value);
            }

            public static double FontSize
            {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(FontSize)), FontSizeDefault);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(FontSize)), value);
            }

            public static Margin Margins
            {
                get => JsonConvert.DeserializeObject<Margin>(AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(Margins)), JsonConvert.SerializeObject(new Margin(MarginDefault))));
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(Margins)), JsonConvert.SerializeObject(value));
            }


            public static int ScrollSpeed
            {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(ScrollSpeed)), ScrollSpeedDefault);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(ScrollSpeed)), value);
            }


            public static Theme Theme
            {
                get => JsonConvert.DeserializeObject<Theme>(AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(Theme)), JsonConvert.SerializeObject(Theme.DefaultTheme)));
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(Theme)), JsonConvert.SerializeObject(value));
            }


            public static double Brightness
            {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(Brightness)), 1d);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(Brightness)), value);
            }

            public static bool Fullscreen {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(Fullscreen)), Device.RuntimePlatform == Device.Android);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(Fullscreen)), value);
            }
        }

        public static class Synchronization {

            public static bool Enabled {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(Enabled)), true);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(Enabled)), value);
            }

            public static long DeviceId {
                get {
                    var id = AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(DeviceId)), default(long));
                    if (id == default(long)) {
                        id = DeviceIdProvider.Id;
                        AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(DeviceId)), id);
                    }
                    return id;
                }
            }

            public static string DeviceName {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(DeviceName)), DeviceNameProvider.Name);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(DeviceName)), value);
            }

            public static string Service {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(Service)), SynchronizationServicesProvider.Dumb);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(Service)), value);
            }

            public static bool OnlyWifi {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(OnlyWifi)), false);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(OnlyWifi)), value);
            }

            public static class Dropbox {
                public static string AccessToken {
                    get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(Dropbox), nameof(AccessToken)), "");
                    set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(Dropbox), nameof(AccessToken)), value);
                }
            }

            public static class Firebase {
                public static string Email {
                    get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(Firebase), nameof(Email)), "");
                    set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(Firebase), nameof(Email)), value);
                }

                public static string Password {
                    get => AppSettings.GetValueOrDefault(CreateKey(nameof(Synchronization), nameof(Firebase), nameof(Password)), "");
                    set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Synchronization), nameof(Firebase), nameof(Password)), value);
                }
            }

        }

        public static class Control {

            public static bool DoubleSwipe {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Control), nameof(DoubleSwipe)), false);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Control), nameof(DoubleSwipe)), value);
            }

            public static BrightnessChange BrightnessChange {
                get => (BrightnessChange)AppSettings.GetValueOrDefault(CreateKey(nameof(Control), nameof(BrightnessChange)), 
                    Device.RuntimePlatform == Device.Android ? (int)BrightnessChange.Left : (int)BrightnessChange.None);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Control), nameof(BrightnessChange)), (int)value);
            }

            public static bool VolumeButtons
            {
                get => AppSettings.GetValueOrDefault(CreateKey(nameof(Control), nameof(VolumeButtons)), false);
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Control), nameof(VolumeButtons)), value);
            }

            public static CommandGrid CommandGrid
            {
                get => JsonConvert.DeserializeObject<CommandGrid>(AppSettings.GetValueOrDefault(CreateKey(nameof(Reader), nameof(CommandGrid)), JsonConvert.SerializeObject(GridConfig.DefaultGrids[0])));
                set => AppSettings.AddOrUpdateValue(CreateKey(nameof(Reader), nameof(CommandGrid)), JsonConvert.SerializeObject(value));
            }
        }

        private static string CreateKey(params string[] names) {
            return string.Join(".", names);
        }
    }
}

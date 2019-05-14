using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using EbookReader.DependencyService;
using EbookReader.Droid.DependencyService;
using Autofac;
using Xam.Plugin.WebView.Droid;
using EbookReader.Service;
using EbookReader.Model.Messages;
using Android.Content;
using EbookReader.Page;
using Plugin.Permissions;

namespace EbookReader.Droid {
    [Activity(Label = "JetReader", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

        BatteryBroadcastReceiver _batteryBroadcastReceiver;
        private bool _disposed = false;

        protected override void OnCreate(Bundle bundle) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            SetUpIoc();

            FormsWebViewRenderer.Initialize();

            FormsWebViewRenderer.OnControlChanged += (sender, webView) => {
                webView.SetLayerType(LayerType.Software, null);
                webView.Settings.LoadWithOverviewMode = true;
                webView.Settings.UseWideViewPort = true;
            };

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

            global::Xamarin.Forms.Forms.SetFlags("FastRenderers_Experimental");
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            Window.SetSoftInputMode(SoftInput.AdjustResize);

            _batteryBroadcastReceiver = new BatteryBroadcastReceiver();
            Application.Context.RegisterReceiver(_batteryBroadcastReceiver, new IntentFilter(Intent.ActionBatteryChanged));
        }

        protected override void OnStart() {
            base.OnStart();

            SetUpSubscribers();
        }

        protected override void OnStop() {
            base.OnStop();

            IocManager.Container.Resolve<IMessageBus>().UnSubscribe("MainActivity");
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e) {
            if (UserSettings.Control.VolumeButtons && (keyCode == Keycode.VolumeDown || keyCode == Keycode.VolumeUp) && App.IsCurrentPageType(typeof(ReaderPage))) {
                var messageBus = IocManager.Container.Resolve<IMessageBus>();
                messageBus.Send(new GoToPageMessage { Next = keyCode == Keycode.VolumeDown, Previous = keyCode == Keycode.VolumeUp });

                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override void OnBackPressed() {
            IocManager.Container.Resolve<IMessageBus>().Send(new BackPressedMessage());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void SetUpIoc() {
            IocManager.ContainerBuilder.RegisterType<AndroidAssetsManager>().As<IAssetsManager>();
            IocManager.ContainerBuilder.RegisterType<BrightnessProvider>().As<IBrightnessProvider>();
            IocManager.ContainerBuilder.RegisterInstance(new BrightnessProvider { Brightness = Android.Provider.Settings.System.GetFloat(ContentResolver, Android.Provider.Settings.System.ScreenBrightness) / 255 }).As<IBrightnessProvider>();
            IocManager.ContainerBuilder.RegisterType<CryptoService>().As<ICryptoService>();
            IocManager.ContainerBuilder.RegisterType<BatteryProvider>().As<IBatteryProvider>();
            IocManager.ContainerBuilder.RegisterType<FileHelper>().As<IFileHelper>();
            IocManager.ContainerBuilder.RegisterType<ToastService>().As<IToastService>();
            IocManager.ContainerBuilder.RegisterType<VersionProvider>().As<IVersionProvider>();
            IocManager.Build();
        }

        private void SetUpSubscribers() {
            var messageBus = IocManager.Container.Resolve<IMessageBus>();
            messageBus.Subscribe<ChangesBrightnessMessage>(ChangeBrightness, new[] { "MainActivity" });
            messageBus.Subscribe<FullscreenRequestMessage>(ToggleFullscreen, new[] { "MainActivity" });
            messageBus.Subscribe<CloseAppMessage>(CloseAppMessageSubscriber, new[] { "MainActivity" });
            messageBus.Subscribe<InteractionMessage>(OnInteraction, new[] { "MainActivity" });
        }

        private void OnInteraction(InteractionMessage msg)
        {
            ToggleFullscreen(new FullscreenRequestMessage(true));
        }

        private void CloseAppMessageSubscriber(CloseAppMessage msg) {
            Finish();
        }

        private void ChangeBrightness(ChangesBrightnessMessage msg) {
            RunOnUiThread(() => {
                var brightness = Math.Min(msg.Brightness, 1);
                brightness = Math.Max(brightness, 0);

                var attributesWindow = new WindowManagerLayoutParams();
                attributesWindow.CopyFrom(Window.Attributes);
                attributesWindow.ScreenBrightness = brightness;
                Window.Attributes = attributesWindow;
            });
        }

        private void ToggleFullscreen(FullscreenRequestMessage msg)
        {
            //TODO: See https://stackoverflow.com/questions/7692789/toggle-fullscreen-mode for more robust function (for more Android versions)
            RunOnUiThread(() =>
            {
                var decorView = Window.DecorView;
                var newUiOptions = (int)decorView.SystemUiVisibility;

                if (msg.Fullscreen)
                {
                    newUiOptions |= (int)SystemUiFlags.Fullscreen;
                    newUiOptions |= (int)SystemUiFlags.HideNavigation;
                    newUiOptions |= (int)SystemUiFlags.Immersive;
                }
                else
                {
                    newUiOptions &= ~(int)SystemUiFlags.Fullscreen;
                    newUiOptions &= ~(int)SystemUiFlags.HideNavigation;
                    newUiOptions &= ~(int)SystemUiFlags.Immersive;
                }

                decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
            });
        }

        protected override void Dispose(bool disposing) {

            if (!_disposed) {
                if (disposing) {
                    if (_batteryBroadcastReceiver != null) {
                        Application.Context.UnregisterReceiver(_batteryBroadcastReceiver);
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}


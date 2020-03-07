using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using EbookReader.Model.Messages;
using EbookReader.Page;
using EbookReader.Service;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using EbookReader.DependencyService;
using System.Reflection;
using EbookReader.Repository;
using PCLAppConfig;

namespace EbookReader {
    public class App : Application {

        private readonly IMessageBus _messageBus;

        private bool _doubleBackToExitPressedOnce = false;

        public static bool HasMasterDetailPage => Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android;

        public App() {
            LoadConfig();

            _messageBus = IocManager.Container.Resolve<IMessageBus>();

            if (UserSettings.Reader.OpenPreviousBookOnLaunch)
            {
                var repo = IocManager.Container.Resolve<IBookRepository>();
                var book = repo.GetMostRecentBook().Result;

                if (book != null)
                {

                    var page = new ReaderPage();
                    page.LoadBook(book);

                    if (HasMasterDetailPage)
                        MainPage = new MainPage {Detail = new NavigationPage(page)};
                    else
                        MainPage = new NavigationPage(page);

                    return;
                }
            }

            if (HasMasterDetailPage)
                MainPage = new MainPage { Detail = new NavigationPage(new HomePage()) };
            else
                MainPage = new NavigationPage(new HomePage());
        }

        public static bool IsCurrentPageType(Type type) {
            var currentPage = Current.MainPage;

            if (currentPage.GetType() == type) {
                return true;
            }

            var lastPage = currentPage.Navigation.NavigationStack.LastOrDefault();
            if (lastPage != null && lastPage.GetType() == type) {
                return true;
            }

            if (currentPage is MainPage masterDetail) {
                var lastDetailPage = masterDetail.Detail.Navigation.NavigationStack.LastOrDefault();
                if (lastDetailPage != null && lastDetailPage.GetType() == type) {
                    return true;
                }
            }

            return false;
        }

        protected override void OnStart() {
            // Handle when your app starts
            AppCenter.Start($"android={AppSettings.AppCenter.Android};uwp={AppSettings.AppCenter.Uwp};", typeof(Analytics), typeof(Crashes));
            Analytics.SetEnabledAsync(UserSettings.AnalyticsAgreement);

            _messageBus.UnSubscribe("App");
            _messageBus.Subscribe<BackPressedMessage>(BackPressedMessageSubscriber, "App");
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
            _messageBus.Send(new AppSleepMessage());
        }

        protected override void OnResume() {
            // Handle when your app resumes
        }

        private async void BackPressedMessageSubscriber(BackPressedMessage msg) {
            if (MainPage is MainPage master)
            {
                var detailPage = master.Detail.Navigation.NavigationStack.LastOrDefault();

                if (detailPage is HomePage)
                {
                    if (_doubleBackToExitPressedOnce)
                    {
                        _messageBus.Send(new CloseAppMessage());
                    }
                    else
                    {
                        IocManager.Container.Resolve<IToastService>().Show("Press once again to exit!");
                        _doubleBackToExitPressedOnce = true;
                        Xamarin.Forms.Device.StartTimer(new TimeSpan(0, 0, 2), () =>
                        {
                            _doubleBackToExitPressedOnce = false;
                            return false;
                        });
                    }
                }
                else
                {
                    await master.Detail.Navigation.PopAsync();
                }
            }
        }

        private void LoadConfig() {
            if (ConfigurationManager.AppSettings == null) {
                var assembly = typeof(App).GetTypeInfo().Assembly;
#if DEBUG
                ConfigurationManager.Initialise(assembly.GetManifestResourceStream("EbookReader.ReaderApp.config"));
#else
                ConfigurationManager.Initialise(assembly.GetManifestResourceStream("EbookReader.ReaderApp.Release.config"));
#endif
            }
        }
    }
}

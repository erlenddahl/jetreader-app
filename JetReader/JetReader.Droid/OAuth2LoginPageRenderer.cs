using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using JetReader.Droid;
using JetReader.Model;
using JetReader.Model.Messages;
using JetReader.Page;
using JetReader.Service;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(OAuth2LoginPage), typeof(OAuth2LoginPageRenderer))]
namespace JetReader.Droid {
    public class OAuth2LoginPageRenderer : PageRenderer {
        bool _done = false;

        public OAuth2LoginPageRenderer(Context context) : base(context) {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e) {
            base.OnElementChanged(e);

            if (!_done) {

                var activity = Context as Activity;

                var oAuth2Data = Xamarin.Forms.Application.Current.Properties["OAuth2Data"] as OAuth2RequestData;

                var auth = new OAuth2Authenticator(
                    oAuth2Data.ClientId,
                    oAuth2Data.Scope,
                    new Uri(oAuth2Data.AuthorizeUrl),
                    new Uri(oAuth2Data.RedirectUrl)
                );

                auth.Completed += (sender, arg) => {
                    if (arg.IsAuthenticated) {
                        IocManager.Container.Resolve<IMessageBus>().Send(new OAuth2AccessTokenObtainedMessage {
                            AccessToken = arg.Account.Properties["access_token"],
                            Provider = oAuth2Data.Provider,
                        });
                    }

                    IocManager.Container.Resolve<IMessageBus>().Send(new OAuth2LoginPageClosed {
                        Provider = oAuth2Data.Provider,
                    });
                };

                auth.Error += (sender, arg) => {
                    IocManager.Container.Resolve<IMessageBus>().Send(new OAuth2LoginPageClosed {
                        Provider = oAuth2Data.Provider,
                    });
                };

                activity.StartActivity(auth.GetUI(activity));
                _done = true;
            }
        }
    }
}
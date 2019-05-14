﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using EbookReader.DependencyService;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;

namespace EbookReader.Model.View {
    public class AboutVm : BaseVm {
        public string Version => IocManager.Container.Resolve<IVersionProvider>().AppVersion;

        public string Copyright => $"Created by Jan Bareš, 2017-{DateTime.Now.Year}";

        public ICommand OpenUrlCommand { get; set; }

        public AboutVm() {
            OpenUrlCommand = new Command((url) => OpenUrl(url));
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

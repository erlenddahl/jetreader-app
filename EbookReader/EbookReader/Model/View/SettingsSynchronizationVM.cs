﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using EbookReader.DependencyService;
using EbookReader.Model.Messages;
using EbookReader.Page.Settings;
using EbookReader.Service;
using Microsoft.AppCenter.Analytics;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace EbookReader.Model.View {
    public class SettingsSynchronizationVm : BaseVm {

        public SynchronizationServiceVm SynchronizationService { get; set; }

        public FirebaseVm Firebase { get; set; }

        public bool Enabled {
            get => UserSettings.Synchronization.Enabled;
            set {
                if (UserSettings.Synchronization.Enabled == value)
                    return;

                UserSettings.Synchronization.Enabled = value;
                OnPropertyChanged();
            }
        }

        public bool OnlyWifi {
            get => UserSettings.Synchronization.OnlyWifi;
            set {
                if (UserSettings.Synchronization.OnlyWifi == value)
                    return;

                UserSettings.Synchronization.OnlyWifi = value;
                OnPropertyChanged();
            }
        }

        public string DeviceName {
            get => UserSettings.Synchronization.DeviceName;
            set {
                if(UserSettings.Synchronization.DeviceName == value)
                    return;

                UserSettings.Synchronization.DeviceName = value;
                OnPropertyChanged();
            }
        }

        public string DropboxAccessToken {
            get => UserSettings.Synchronization.Dropbox.AccessToken;
            set {
                if(UserSettings.Synchronization.Dropbox.AccessToken == value)
                    return;

                UserSettings.Synchronization.Dropbox.AccessToken = value;
                OnPropertyChanged();
                OnPropertyChanged("IsConnected");
            }
        }
        
        public bool IsConnected => !string.IsNullOrEmpty(DropboxAccessToken);

        public ICommand ConnectToDropboxCommand { get; set; }
        public ICommand DisconnectDropboxCommand { get; set; }

        public SettingsSynchronizationVm() {
            SynchronizationService = new SynchronizationServiceVm();
            Firebase = new FirebaseVm();
            ConnectToDropboxCommand = new Command(ConnectToDropbox);
            DisconnectDropboxCommand = new Command(DisconnectDropboxAsync);

            IocManager.Container.Resolve<IMessageBus>().Subscribe<OAuth2AccessTokenObtainedMessage>((msg) => {
                if(msg.Provider == "Dropbox") {
                    DropboxAccessToken = msg.AccessToken;

                    Analytics.TrackEvent("Dropbox login successful");
                }
            });
        }

        void ConnectToDropbox() {
            if (!CrossConnectivity.Current.IsConnected) {
                IocManager.Container.Resolve<IToastService>().Show("There is no Internet connection.");

                return;
            } 

            IocManager.Container.Resolve<IMessageBus>().Send(new OpenDropboxLoginMessage());
        }

        void DisconnectDropboxAsync() {
            DropboxAccessToken = string.Empty;
        }
    }
}
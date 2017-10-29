﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EbookReader.Page.Reader {
    public class QuickPanel : StackLayout {

        StackLayout contentLayout;

        StackLayout tabSettings;
        StackLayout tabContents;

        Button buttonSettings;
        Button buttonContents;
        Button buttonClose;

        public QuickPanelTab.Content PanelContent;
        private QuickPanelTab.Settings PanelSettings;

        public QuickPanel() : base() {

            PanelContent = new QuickPanelTab.Content();
            PanelContent.OnChapterChange += PanelContent_OnChapterChange;

            PanelSettings = new QuickPanelTab.Settings();
            PanelSettings.OnSet += PanelSettings_OnSet;

            Orientation = StackOrientation.Vertical;
            IsVisible = false;
            BackgroundColor = Color.LightGray;

            buttonSettings = new Button {
                Text = "Nastavení",
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            buttonSettings.Clicked += ButtonSettings_Clicked;

            buttonContents = new Button {
                Text = "Obsah",
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            buttonContents.Clicked += ButtonContents_Clicked;

            buttonClose = new Button {
                Text = "Zavřít"
            };
            buttonClose.Clicked += ButtonClose_Clicked;

            var buttonsLayout = new StackLayout {
                Orientation = StackOrientation.Horizontal,
                Children = {
                    buttonSettings,
                    buttonContents,
                    buttonClose,
                }
            };

            contentLayout = new StackLayout {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            this.OpenSettings();

            Children.Add(buttonsLayout);
            Children.Add(contentLayout);
        }

        private void PanelSettings_OnSet(object sender, EventArgs e) {
            this.Hide();
        }

        private void PanelContent_OnChapterChange(object sender, Model.Navigation.Item e) {
            this.Hide();
        }

        public void Show() {
            Device.BeginInvokeOnMainThread(() => {
                IsVisible = true;
            });
        }

        public void Hide() {
            Device.BeginInvokeOnMainThread(() => {
                IsVisible = false;
            });
        }

        private void ButtonClose_Clicked(object sender, EventArgs e) {
            this.Hide();
        }

        private void ButtonContents_Clicked(object sender, EventArgs e) {
            this.OpenContents();
        }

        private void ButtonSettings_Clicked(object sender, EventArgs e) {
            this.OpenSettings();
        }

        private void OpenTab(StackLayout tab) {
            Device.BeginInvokeOnMainThread(() => {
                contentLayout.Children.Clear();
                contentLayout.Children.Add(tab);
            });
        }

        private void OpenContents() {
            if (tabContents == null) {
                tabContents = this.CreateContentsTab();
            }

            this.OpenTab(tabContents);
        }

        private void OpenSettings() {
            if (tabSettings == null) {
                tabSettings = this.CreateSettingsTab();
            }

            this.OpenTab(tabSettings);
        }

        private StackLayout CreateSettingsTab() {
            var tab = new StackLayout {
                Children = {
                    PanelSettings
                }
            };

            return tab;
        }

        private StackLayout CreateContentsTab() {
            var tab = new StackLayout {
                Children = {
                    PanelContent
                }
            };

            return tab;
        }

    }
}

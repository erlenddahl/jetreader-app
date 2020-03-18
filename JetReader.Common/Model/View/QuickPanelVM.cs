using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using JetReader.Model.Messages;
using JetReader.Service;
using Xamarin.Forms;

namespace JetReader.Model.View {
    public class QuickPanelVm : BaseVm {
        bool _tabContentVisible;
        public bool TabContentVisible {
            get => _tabContentVisible;
            set {
                _tabContentVisible = value;
                OnPropertyChanged();
            }
        }

        bool _tabBookmarksVisible;
        public bool TabBookmarksVisible {
            get => _tabBookmarksVisible;
            set {
                _tabBookmarksVisible = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand TabContentTappedCommand { get; set; }
        public ICommand TabBookmarksTappedCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public QuickPanelVm() {
            TabContentVisible = true;

            TabContentTappedCommand = new Command(() => {
                TabContentVisible = true;
                TabBookmarksVisible = false;
            });

            TabBookmarksTappedCommand = new Command(() => {
                TabBookmarksVisible = true;
                TabContentVisible = false;
            });

            CloseCommand = new Command(() => {
                IocManager.Container.Resolve<IMessageBus>().Send(new CloseQuickPanelMessage());
            });
        }
    }
}

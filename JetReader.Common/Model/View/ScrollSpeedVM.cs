using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Model.Messages;
using JetReader.Provider;
using JetReader.Service;

namespace JetReader.Model.View {
    public class ScrollSpeedVm : BaseVm {

        IMessageBus _messageBus;

        public ScrollSpeedVm() {
            _messageBus = IocManager.Container.Resolve<IMessageBus>();
        }

        public List<int> Items => ScrollSpeedProvider.Items;

        public int Value {
            get => UserSettings.Reader.ScrollSpeed;
            set {
                if (UserSettings.Reader.ScrollSpeed == value)
                    return;

                UserSettings.Reader.ScrollSpeed = value;
                OnPropertyChanged();
            }
        }
    }
}

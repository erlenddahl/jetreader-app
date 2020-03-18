using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Provider;

namespace JetReader.Model.View {
    public class SynchronizationServiceVm : BaseVm {
        
        public List<string> Items => SynchronizationServicesProvider.Items;

        public string Value {
            get => UserSettings.Synchronization.Service;
            set {
                if (UserSettings.Synchronization.Service == value)
                    return;

                UserSettings.Synchronization.Service = value;
                OnPropertyChanged();
            }
        }
    }
}

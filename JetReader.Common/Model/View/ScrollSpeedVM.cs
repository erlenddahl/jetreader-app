using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Extensions.IntExtensions;
using JetReader.Model.Messages;
using JetReader.Provider;
using JetReader.Service;

namespace JetReader.Model.View {
    public class ScrollSpeedVm : BaseVm
    {
        public List<int> Items => 0.To(50, 5).ToList();

        public int Value
        {
            get => UserSettings.Reader.ScrollSpeed;
            set
            {
                if (UserSettings.Reader.ScrollSpeed == value)
                    return;

                UserSettings.Reader.ScrollSpeed = value;
                OnPropertyChanged();
            }
        }
    }
}

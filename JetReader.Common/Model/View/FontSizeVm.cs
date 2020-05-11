using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.IntExtensions;

namespace JetReader.Model.View
{
    public class FontSizeVm : BaseVm
    {
        public List<double> Items => 5.To(50).Select(p => (double)p).ToList();

        public double Value
        {
            get => UserSettings.Reader.FontSize;
            set
            {
                if (Math.Abs(UserSettings.Reader.FontSize - value) < 0.5)
                    return;

                UserSettings.Reader.FontSize = value;
                OnPropertyChanged();
            }
        }
    }
}
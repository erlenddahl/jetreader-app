using System;
using System.Linq;
using JetReader.Config.CommandGrid;

namespace JetReader.Model.View
{
    public class MarginVm : BaseVm
    {
        public string Summary
        {
            get
            {
                var v = new[] {Value.Left, Value.Top, Value.Right, Value.Bottom};
                if (v.Distinct().Count() == 1) return $"All: {v[0]:n0}";
                if (Math.Abs(v[0] - v[2]) < 0.1 && Math.Abs(v[1] - v[3]) < 0.1) return $"\u2191\u2193 {v[1]:n0}, \u2190\u2192 {v[0]:n0}";

                return $"\u2190 {v[0]:n0} \u2191 {v[1]:n0} \u2192 {v[2]:n0} \u2193 {v[3]:n0}";
            }
        }

        public Margin Value
        {
            get => UserSettings.Reader.Margins;
            set
            {
                if (UserSettings.Reader.Margins?.Equals(value) == true)
                    return;

                UserSettings.Reader.Margins = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Summary));
            }
        }
    }
}
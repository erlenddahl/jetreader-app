using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace JetReader.View {
    public class MyFloatButton : Xamarin.Forms.View {

        public event EventHandler OnClicked;

        public string ButtonBackgroundColor { get; set; }

        public MyFloatButton() {
            ButtonBackgroundColor = AppSettings.Color;
            Margin = new Thickness(0, 0, 20, 20);
        }

        public void TriggerClicked() {
            OnClicked?.Invoke(this, new EventArgs());
        }
    }
}

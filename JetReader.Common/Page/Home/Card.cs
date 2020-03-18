using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace JetReader.Page.Home {
    public static class Card {
        public static int CardHeight => Device.RuntimePlatform == Device.Android ? 200 : 480;
        public static int CardWidth => (int)Math.Round(CardHeight / 1.33);
    }
}

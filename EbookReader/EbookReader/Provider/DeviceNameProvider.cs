﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EbookReader.Provider {
    public static class DeviceNameProvider {
        public static string Name => Device.RuntimePlatform == Device.UWP ? "Computer" : "Phone";
    }
}

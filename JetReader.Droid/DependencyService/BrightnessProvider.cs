﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JetReader.DependencyService;

namespace JetReader.Droid.DependencyService {
    public class BrightnessProvider : IBrightnessProvider {
        public float Brightness { get; set; }
    }
}
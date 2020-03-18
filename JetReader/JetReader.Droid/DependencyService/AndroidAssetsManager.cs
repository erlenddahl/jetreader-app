﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JetReader.DependencyService;
using JetReader.Droid.DependencyService;

namespace JetReader.Droid.DependencyService {
    public class AndroidAssetsManager : IAssetsManager {

        public async Task<string> GetFileContentAsync(string filename) {
            var content = string.Empty;

            return await Task.Run(() => {
                var file = Application.Context.Assets.Open(filename, Access.Buffer);
                using (var sr = new StreamReader(file)) {
                    content = sr.ReadToEnd();
                }

                return content;
            });

        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JetReader.Service;
using Plugin.FilePicker;

namespace JetReader.Droid.DependencyService
{
    public class AndroidFileService : FileService
    {
        public override async Task<Stream> LoadFileStreamAsync(string filePath)
        {
            if (IOUtil.IsMediaStore(filePath))
            {
                var contentUri = Android.Net.Uri.Parse(filePath);
                return Application.Context.ContentResolver.OpenInputStream(contentUri);
            }

            return File.OpenRead(filePath);
        }

        public override async Task<long> GetFileSizeInBytes(string filePath)
        {
            if (IOUtil.IsMediaStore(filePath))
            {
                var contentUri = Android.Net.Uri.Parse(filePath);
                return Application.Context.ContentResolver.OpenAssetFileDescriptor(contentUri, "r").Length;
            }

            return await base.GetFileSizeInBytes(filePath);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using EbookReader.Model.Messages;
using EbookReader.Service;
using Xamarin.Forms;

namespace EbookReader.Model.View {
    public class BookmarksVm {
        public ICommand AddBookmarkCommand { get; set; }

        public BookmarksVm() {
            AddBookmarkCommand = new Command(AddBookmark);
        }

        void AddBookmark() {
            IocManager.Container.Resolve<IMessageBus>().Send(new AddBookmarkMessage());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.DependencyService;
using EbookReader.Service;
using Plugin.HybridWebView.Shared;

namespace EbookReader.View {
    public class ReaderWebView : HybridWebViewControl {

        public WebViewMessages Messages { get; }

        private IAssetsManager _assetsManager;

        public static ReaderWebView Factory() {
            return IocManager.Container.Resolve<ReaderWebView>();
        }

        public ReaderWebView(IAssetsManager assetsManager) : base() {
            _assetsManager = assetsManager;
            Messages = new WebViewMessages(this);
        }
    }
}

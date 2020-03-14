using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Helpers;
using EbookReader.Model.Messages;
using Newtonsoft.Json;
using EbookReader.View;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;

namespace EbookReader.Service {
    public class WebViewMessages {
        readonly ReaderWebView _webView;
        bool _webViewLoaded = false;
        bool _webViewReaderInit = false;
        readonly List<Model.WebViewMessages.Message> _queue;

        public event EventHandler<Model.WebViewMessages.PageChange> OnPageChange;
        public event EventHandler<Model.WebViewMessages.ReadStats> OnReadStats;
        public event EventHandler<Model.WebViewMessages.NextChapterRequest> OnNextChapterRequest;
        public event EventHandler<Model.WebViewMessages.PrevChapterRequest> OnPrevChapterRequest;
        public event EventHandler<Model.WebViewMessages.LinkClicked> OnLinkClicked;
        public event EventHandler<Model.WebViewMessages.PanEvent> OnPanEvent;
        public event EventHandler<Model.WebViewMessages.KeyStroke> OnKeyStroke;
        public event EventHandler<Model.WebViewMessages.CommandRequest> OnCommandRequest;
        public event EventHandler<JObject> OnInteraction;
        public event Action<Model.WebViewMessages.Message, string> OnMessageReturned;

        public WebViewMessages(ReaderWebView webView) {
            _webView = webView;
            _queue = new List<Model.WebViewMessages.Message>();

            _webView.AddLocalCallback("csCallback", Parse);
            _webView.OnContentLoaded += WebView_OnContentLoaded;
        }

        public void Send(string action, object data) {

            var message = new Model.WebViewMessages.Message {
                Action = action,
                Data = data,
            };

            _queue.Add(message);
            ProcessQueue();
        }

        private void DoSendMessage(Model.WebViewMessages.Message message) {
            if (!_webViewLoaded || (message.Action != "init" && !_webViewReaderInit)) return;

            message.IsSent = true;

            var json = JsonConvert.SerializeObject(new {
                message.Action, message.Data,
            });

            var toSend = Base64Helper.Encode(json);

            Device.BeginInvokeOnMainThread(async () => {
                // Exception "HRESULT: 0x80020101" on this line means that there is a syntax error in the JavaScript. Typically on Windows, when using IE.
                var res = await _webView.InjectJavascriptAsync($"Messages.parse('{toSend}')");
                OnMessageReturned?.Invoke(message, res);
            });

            if (message.Action == "init") {
                _webViewReaderInit = true;
            }
        }

        private void Raise<T>(object source, string eventName, T eventArgs)
        {
            var eventDelegate = (MulticastDelegate)source.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(source);
            if (eventDelegate != null)
                eventDelegate.Method.Invoke(eventDelegate.Target, new[] { source, eventArgs });
        }

        private void Parse(string data) {
            var json = JsonConvert.DeserializeObject<Model.WebViewMessages.Message>(Base64Helper.Decode(data));

            var messageType = Type.GetType($"EbookReader.Model.WebViewMessages.{json.Action}");
            var msg = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(json.Data), messageType);

            // Use reflection to enumerate all webview messages, and call the appropriate event.
            foreach (var cls in Assembly.GetExecutingAssembly().GetTypes().Where(p => p.IsClass && p.Namespace == "EbookReader.Model.WebViewMessages"))
                if (cls.Name == json.Action)
                {
                    Raise(this, "On" + cls.Name, msg);
                    //Debug.WriteLine("Raising event " + "On" + cls.Name);
                    return;
                }

            // If no existing webview messages were found, this is a manual event. Handle it accordingly.
            switch (json.Action) {
                case "Interaction":
                    OnInteraction?.Invoke(this, msg as JObject);
                    break;
                case "Debug":
                    Debug.WriteLine(msg);
                    break;
            }

        }

        private void ProcessQueue() {

            var messages = _queue.Where(o => !o.IsSent).OrderBy(o => o.Action == "init" ? 0 : 1).ToList();

            foreach (var msg in messages) {
                DoSendMessage(msg);
                if (msg.IsSent) {
                    _queue.Remove(msg);
                }
            }
        }

        private void WebView_OnContentLoaded(object sender, EventArgs e) {
            _webViewLoaded = true;
            ProcessQueue();
        }

    }
}
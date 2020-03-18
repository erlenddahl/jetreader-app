using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Rg.Plugins.Popup.Pages;

namespace JetReader.Page.Popups
{
    public partial class LoadingPopup : PopupPage
    {
        public string Owner { get; set; }

        private static int _instanceNumber = 0;

        public LoadingPopup([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0, int owner = 0)
        {
            // Used for debugging when multiple loading screens were present.
            Owner = sourceFilePath + ":" + sourceLineNumber + " (" + memberName + ")" + " -- " + (_instanceNumber++) + " (from: " + owner + ")";
            Debug.WriteLine("LOADINGINIT: " + DateTime.Now.ToString("HH:mm:ss.fff") + " - " + Owner);

            InitializeComponent();

            BindingContext = this;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using EbookReader.Droid;
using EbookReader.View;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MyFloatButton), typeof(FloatingActionButtonRenderer))]
namespace EbookReader.Droid {
    [Preserve]
    public class FloatingActionButtonRenderer : ViewRenderer<MyFloatButton, FloatingActionButton> {

        private FloatingActionButton _fab;

        public FloatingActionButtonRenderer(Context context) : base(context) {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MyFloatButton> e) {
            base.OnElementChanged(e);


            if (Control == null) {
                _fab = new FloatingActionButton(Context);
                _fab.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                _fab.Clickable = true;
                _fab.SetImageDrawable(ContextCompat.GetDrawable(Context, Resource.Drawable.add));
                _fab.BackgroundTintList = new ColorStateList(new[] { new int[0] }, new int[] { Android.Graphics.Color.ParseColor(e.NewElement.ButtonBackgroundColor) });
                SetNativeControl(_fab);
            }

            if (e.NewElement != null) {
                _fab.Click += Fab_Click;
            }

            if (e.OldElement != null) {
                _fab.Click -= Fab_Click;
            }
        }

        private void Fab_Click(object sender, EventArgs e) {
            if (Element != null) {
                Element.TriggerClicked();
            }
        }
    }
}
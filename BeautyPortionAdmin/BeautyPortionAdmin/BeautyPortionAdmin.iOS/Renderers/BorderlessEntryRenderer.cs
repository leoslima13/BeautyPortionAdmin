using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeautyPortionAdmin.Controls;
using BeautyPortionAdmin.iOS.Renderers;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace BeautyPortionAdmin.iOS.Renderers
{
    public class BorderlessEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null) return;

            Control.BorderStyle = UITextBorderStyle.None;
        }
    }
}
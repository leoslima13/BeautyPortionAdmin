using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Controls
{
    public class BorderlessEntry : Entry
    {
        public readonly BindableProperty OnFocusedProperty =
            BindableProperty.Create(nameof(OnFocused), typeof(Action<bool>), typeof(BorderlessEntry), null, propertyChanged: OnFocusedPropertyChanged);

        public BorderlessEntry()
        {
            FontFamily = "GillSans";
        }

        public Action<bool> OnFocused
        {
            get => (Action<bool>)GetValue(OnFocusedProperty);
            set => SetValue(OnFocusedProperty, value);
        }

        private static void OnFocusedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as BorderlessEntry;

            entry.OnFocused = (Action<bool>)newValue;
            entry.Focused += (s, e) => entry.OnFocused?.Invoke(true);
            entry.Unfocused += (s, e) => entry.OnFocused?.Invoke(false);
        }

    }
}

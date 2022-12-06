using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace BeautyPortionAdmin.AttachedProperties
{
    public class TappedGestureAttached
    {
        public static readonly TimeSpan DeBounceTimeSpan = TimeSpan.FromMilliseconds(500);
        public static DateTimeOffset LastTap = DateTimeOffset.MinValue;

        public static readonly BindableProperty CommandProperty =
            BindableProperty.CreateAttached("Command", typeof(ICommand), typeof(View), null, BindingMode.OneWay, propertyChanged: OnItemTappedChanged);

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.CreateAttached("CommandParameter", typeof(object), typeof(View), null);


        public static object GetCommandParameter(BindableObject bindable)
        {
            return bindable.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(BindableObject bindable, object value)
        {
            bindable.SetValue(CommandParameterProperty, value);
        }

        public static ICommand GetCommand(BindableObject bindable)
        {
            return (ICommand)bindable.GetValue(CommandProperty);
        }

        public static void SetCommand(BindableObject bindable, ICommand value)
        {
            bindable.SetValue(CommandProperty, value);
        }

        public static void OnItemTappedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is View control))
                return;

            var cmd = new Command(o => OnItemTapped(control));

            control.GestureRecognizers.Clear();
            control.GestureRecognizers.Add(new TapGestureRecognizer { Command = cmd });
        }

        private static void OnItemTapped(View control)
        {
            var command = GetCommand(control);

            if (command != null
                && command.CanExecute(control.GetValue(CommandParameterProperty))
                && !ShouldThrottleTap())
            {
                LastTap = DateTimeOffset.Now;
                command.Execute(control.GetValue(CommandParameterProperty));
            }

            bool ShouldThrottleTap()
            {
                var throttleTaps = DateTimeOffset.Now - LastTap < DeBounceTimeSpan;
                if (!throttleTaps) return false;

                return true;
            }
        }
    }
}

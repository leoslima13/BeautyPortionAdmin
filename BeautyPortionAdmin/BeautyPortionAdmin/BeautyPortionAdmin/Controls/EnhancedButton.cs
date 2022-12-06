using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace BeautyPortionAdmin.Controls
{
    public class EnhancedButton : PancakeView
    {
        readonly Grid _rootLayout = new Grid { VerticalOptions = LayoutOptions.Fill, HorizontalOptions = LayoutOptions.Fill };
        readonly TapGestureRecognizer _tapGestureRecognizer = new TapGestureRecognizer();
        readonly Label _textLabel = new Label { VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.CenterAndExpand };
        readonly ActivityIndicator _busyIndicator = new ActivityIndicator { VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.Center };

        public EnhancedButton()
        {
            _rootLayout.Children.Add(_busyIndicator);
            _rootLayout.Children.Add(_textLabel);

            Content = _rootLayout;

            GestureRecognizers.Add(_tapGestureRecognizer);
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(EnhancedButton), string.Empty, propertyChanged: OnTextChanged);

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(EnhancedButton), Color.Default, propertyChanged: OnTextColorChanged);

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EnhancedButton), null, propertyChanged: OnCommandChanged);

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(EnhancedButton), null, propertyChanged: OnCommandParameterChanged);

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(EnhancedButton), false, propertyChanged: OnIsBusyChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(EnhancedButton), default, propertyChanged: OnFontSizeChanged);

        public static readonly BindableProperty FontFamilyProperty =
         BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(EnhancedButton), string.Empty, propertyChanged: OnFontFamilyChanged);

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public string FontFamily
        {
            get => (string)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            string newText = (string)newValue;

            enhancedButton._textLabel.Text = newText;
        }

        private static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            enhancedButton._textLabel.TextColor = (Color)newValue;
            enhancedButton._busyIndicator.Color = (Color)newValue;
        }

        private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            enhancedButton._tapGestureRecognizer.Command = (ICommand)newValue;
            if (newValue == null)
                return;

            CheckCanExecuteCommand(enhancedButton);
            enhancedButton.Command.CanExecuteChanged += (s, e) =>
            {
                if (enhancedButton.Command != null && !enhancedButton.Command.CanExecute(enhancedButton.CommandParameter))
                    VisualStateManager.GoToState(enhancedButton, "Disabled");
                else
                    VisualStateManager.GoToState(enhancedButton, "Normal");
            };
        }

        private static void OnCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            enhancedButton._tapGestureRecognizer.CommandParameter = newValue;
            CheckCanExecuteCommand(enhancedButton);
        }

        private static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            bool isBusy = (bool)newValue;
            if (isBusy)
                VisualStateManager.GoToState(enhancedButton, "Disabled");
            else
                VisualStateManager.GoToState(enhancedButton, "Normal");
            enhancedButton.WidthRequest = enhancedButton._textLabel.Width;
            enhancedButton._textLabel.IsVisible = !isBusy;
            enhancedButton._busyIndicator.IsVisible = enhancedButton._busyIndicator.IsRunning = isBusy;
        }

        private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            enhancedButton._textLabel.FontSize = (double)newValue;
        }

        private static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var enhancedButton = bindable as EnhancedButton;
            enhancedButton._textLabel.FontFamily = (string)newValue;
        }

        private static void CheckCanExecuteCommand(EnhancedButton enhancedButton)
        {
            if (enhancedButton.Command == null)
            {
                VisualStateManager.GoToState(enhancedButton, "Disabled");
                return;
            }

            var canExecute = enhancedButton.Command.CanExecute(enhancedButton.CommandParameter);

            if (!canExecute)
                VisualStateManager.GoToState(enhancedButton, "Disabled");
            else
                VisualStateManager.GoToState(enhancedButton, "Normal");
        }
    }
}

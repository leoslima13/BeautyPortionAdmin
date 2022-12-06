using BeautyPortionAdmin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BeautyPortionAdmin.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomEntry : ContentView
    {
        public CustomEntry()
        {
            InitializeComponent();

            MyEntry = borderlessEntry;
            UnderlineEntry = boxView;
            MessageLabel = messageLabel;
            StackEntry = stkEntry;

            messageLabel.Text = " "; //This is necessary to label always occupy your space on view

            this.Focused += OnFocused;
            this.Unfocused += OnUnfocused;

            borderlessEntry.OnFocused += OnEntryFocused;
            borderlessEntry.TextChanged += OnEntryTextChanged;

            borderlessEntry.TextColor = TextColor;
            borderlessEntry.Placeholder = Placeholder;
            borderlessEntry.PlaceholderColor = PlaceholderColor;

            boxView.Color = UnfocusedLineColor;
            stkEntry.BackgroundColor = Color.FromRgba(UnfocusedLineColor.R, UnfocusedLineColor.G, UnfocusedLineColor.B, .16);
        }

        ~CustomEntry()
        {
            this.Focused -= OnFocused;
            this.Unfocused -= OnUnfocused;
            borderlessEntry.OnFocused -= OnEntryFocused;
            borderlessEntry.TextChanged -= OnEntryTextChanged;
        }

        public BorderlessEntry MyEntry { get; }

        public BoxView UnderlineEntry { get; }

        public StackLayout StackEntry { get; }

        public Label MessageLabel { get; }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomEntry), propertyChanged: OnPlaceholderChanged);

        public static readonly BindableProperty UnfocusedLineColorProperty =
            BindableProperty.Create(nameof(UnfocusedLineColor), typeof(Color), typeof(CustomEntry), Colors.PrimaryColor, propertyChanged: OnFocusedLineChanged);

        public static readonly BindableProperty FocusedLineColorProperty =
            BindableProperty.Create(nameof(FocusedLineColor), typeof(Color), typeof(CustomEntry), Colors.PrimaryDarkColor, propertyChanged: OnFocusedLineChanged);

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CustomEntry), Colors.PrimaryTextColor, propertyChanged: OnTextColorChanged);

        public static readonly BindableProperty IsPasswordProperty =
            BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(CustomEntry), false, propertyChanged: OnIsPasswordChanged);

        public static readonly BindableProperty MaxLengthProperty =
            BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(CustomEntry), 50, propertyChanged: OnMaxLengthChanged);

        public static readonly BindableProperty KeyboardProperty =
            BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(CustomEntry), Keyboard.Default, propertyChanged: OnKeyboardChanged);

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomEntry), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnTextPropertyChanged);

        public static readonly BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(CustomEntry), Colors.SecondaryTextColor, propertyChanged: OnPlaceholderColorChanged);

        public static readonly BindableProperty FloatLabelTextProperty =
            BindableProperty.Create(nameof(FloatLabelText), typeof(string), typeof(CustomEntry), propertyChanged: OnFloatLabelTextChanged);

        public static readonly BindableProperty FloatLabelTextColorProperty =
            BindableProperty.Create(nameof(FloatLabelTextColor), typeof(Color), typeof(CustomEntry), Colors.PrimaryTextColor, propertyChanged: OnFloatLabelTextColorChanged);

        public static readonly BindableProperty FontFamilyProperty =
            BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(CustomEntry), propertyChanged: OnFontFamilyChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CustomEntry), propertyChanged: OnFontSizeChanged);

        public static readonly BindableProperty IsRequiredProperty =
            BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(CustomEntry), propertyChanged: OnIsRequiredChanged);

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string FontFamily
        {
            get => (string)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public Color UnfocusedLineColor
        {
            get => (Color)GetValue(UnfocusedLineColorProperty);
            set => SetValue(UnfocusedLineColorProperty, value);
        }

        public Color FocusedLineColor
        {
            get => (Color)GetValue(FocusedLineColorProperty);
            set => SetValue(FocusedLineColorProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }

        public string FloatLabelText
        {
            get => (string)GetValue(FloatLabelTextProperty);
            set => SetValue(FloatLabelTextProperty, value);
        }

        public string FloatLabelTextColor
        {
            get => (string)GetValue(FloatLabelTextColorProperty);
            set => SetValue(FloatLabelTextColorProperty, value);
        }

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.Text = (string)newValue;
        }

        private static void OnKeyboardChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.Keyboard = (Keyboard)newValue;
        }

        private static void OnMaxLengthChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.MaxLength = (int)newValue;
        }

        private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.IsPassword = (bool)newValue;
        }

        private static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.TextColor = (Color)newValue;
        }

        private static void OnFocusedLineChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            var newColor = (Color)newValue;

            entry.boxView.Color = newColor;
            entry.stkEntry.BackgroundColor = Color.FromRgba(newColor.R, newColor.G, newColor.B, .16);
        }

        private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.Placeholder = (string)newValue;
        }

        private static void OnPlaceholderColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;
            entry.borderlessEntry.PlaceholderColor = (Color)newValue;
        }

        private static void OnFloatLabelTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;

            if (entry.IsRequired)
            {
                entry.floatLabel.FormattedText = new FormattedString
                {
                    Spans =
                    {
                        new Span { Text = (string)newValue },
                        new Span { Text = " *", TextColor=Colors.AccentColor, FontFamily="GillSansMedium" }
                    }
                };
            }
            else
                entry.floatLabel.Text = (string)newValue;
        }

        private static void OnFloatLabelTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;

            entry.floatLabel.TextColor = (Color)newValue;
        }

        private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;

            entry.borderlessEntry.FontSize= (double)newValue;
        }

        private static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;

            entry.borderlessEntry.FontFamily = (string)newValue;
        }

        private static void OnIsRequiredChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var entry = bindable as CustomEntry;

            bool isRequired = (bool)newValue;

            if (isRequired)
            {
                entry.floatLabel.FormattedText = new FormattedString
                {
                    Spans =
                    {
                        new Span { Text = entry.FloatLabelText },
                        new Span { Text = " *", TextColor=Colors.AccentColor, FontFamily="GillSansMedium" }
                    }
                };
            }
            else
                entry.floatLabel.Text = entry.FloatLabelText;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            Text = e.NewTextValue;
        }

        private void OnEntryFocused(bool isFocused)
        {
            boxView.Color = isFocused ? FocusedLineColor : UnfocusedLineColor;
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            borderlessEntry.Unfocus();
            borderlessEntry.OnFocused?.Invoke(false);
        }

        private void OnFocused(object sender, FocusEventArgs e)
        {
            borderlessEntry.Focus();
            borderlessEntry.OnFocused?.Invoke(true);
        }
    }
}
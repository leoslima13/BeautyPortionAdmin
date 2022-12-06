using System;
using System.Collections.Generic;
using System.Windows.Input;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Services;
using BeautyPortionAdmin.Views.Dialogs.Picker;
using Prism.Navigation;
using Reactive.Bindings;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Controls
{
    public enum PickerSelectionMode
    {
        Single,
        Multiple
    }

    public partial class CustomPicker : ContentView
    {
        private ReactiveCommand _tappedGestureCommand;

        public CustomPicker()
        {
            InitializeComponent();

            var tappedGesture = new TapGestureRecognizer();
            _tappedGestureCommand = new ReactiveCommand();
            _tappedGestureCommand.ThrottleFirst()
                .Subscribe(_ => OnCustomPickerTapped());
            tappedGesture.Command = _tappedGestureCommand;

            GestureRecognizers.Add(tappedGesture);
        }

        public static readonly BindableProperty FloatLabelTextProperty =
            BindableProperty.Create(nameof(FloatLabelText), typeof(string), typeof(CustomPicker), propertyChanged: OnFloatLabelTextPropertyChanged);

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomPicker), propertyChanged: OnTextPropertyChanged);

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomPicker));

        public static readonly BindableProperty SearchPlaceholderProperty =
            BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(CustomPicker));

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomPicker), propertyChanged: OnPlaceholderPropertyChanged);

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<IPickerItem>), typeof(CustomPicker), default);

        public static readonly BindableProperty SelectedItemsProperty =
            BindableProperty.Create(nameof(SelectedItems), typeof(IEnumerable<IPickerItem>), typeof(CustomPicker), default, BindingMode.OneWayToSource);

        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CustomPicker));

        public static readonly BindableProperty SelectionModeProperty =
            BindableProperty.Create(nameof(SelectionMode), typeof(PickerSelectionMode), typeof(CustomPicker), default);

        public static readonly BindableProperty PopupPageServiceProperty =
            BindableProperty.Create(nameof(PopupPageService), typeof(IPopupPageService), typeof(CustomPicker));

        public static readonly BindableProperty OpenPickerCommandProperty =
            BindableProperty.Create(nameof(OpenPickerCommand), typeof(ReactiveCommand), typeof(CustomPicker), defaultBindingMode: BindingMode.OneTime, propertyChanged: OnOpenPickerCommandChanged);

        public string FloatLabelText
        {
            get => (string)GetValue(FloatLabelTextProperty);
            set => SetValue(FloatLabelTextProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string SearchPlaceholder
        {
            get => (string)GetValue(SearchPlaceholderProperty);
            set => SetValue(SearchPlaceholderProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public IEnumerable<IPickerItem> ItemsSource
        {
            get => (IEnumerable<IPickerItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public IEnumerable<IPickerItem> SelectedItems
        {
            get => (IEnumerable<IPickerItem>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public PickerSelectionMode SelectionMode
        {
            get => (PickerSelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public IPopupPageService PopupPageService
        {
            get => (IPopupPageService)GetValue(PopupPageServiceProperty);
            set => SetValue(PopupPageServiceProperty, value);
        }

        public ReactiveCommand OpenPickerCommand
        {
            get => (ReactiveCommand)GetValue(OpenPickerCommandProperty);
            set => SetValue(OpenPickerCommandProperty, value);
        }

        private static void OnFloatLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var customPicker = bindable as CustomPicker;
            customPicker.floatLabel.Text = (string)newValue;
        }

        private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var customPicker = bindable as CustomPicker;
            customPicker.textEntry.Placeholder = (string)newValue;
        }

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var customPicker = bindable as CustomPicker;
            customPicker.textEntry.Text = (string)newValue;
        }

        private static void OnOpenPickerCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var customPicker = bindable as CustomPicker;
            var newCommand = (ReactiveCommand)newValue;
            newCommand.Subscribe(customPicker.OnCustomPickerTapped);
        }

        private void OnCustomPickerTapped()
        {
            var param = new PickerPageParameter(Title, ItemsSource, ItemTemplate, SelectionMode, (items) => SelectedItems = items, SearchPlaceholder);
            var parameters = new NavigationParameters
            {
                { nameof(PickerPageParameter), param }
            };

            PopupPageService.ShowPopup<PickerPageViewModel>(parameters);
        }
    }
}

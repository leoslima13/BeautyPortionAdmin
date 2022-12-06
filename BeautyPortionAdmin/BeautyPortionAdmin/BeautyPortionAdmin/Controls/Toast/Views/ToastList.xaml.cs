using BeautyPortionAdmin.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using DryIoc;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.Ioc;

namespace BeautyPortionAdmin.Controls.Toast.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ToastList : StackLayout
    {
        private const double MINIMUM_SWIPE_LEFT_OFFSET = 50;
        private const double MINIMUM_SWIPE_UP_OFFSET = 20;
        private double _swipeOffset;

        public ToastList()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create
            (nameof(ItemsSource), typeof(IEnumerable<object>), typeof(ToastList), default(IEnumerable<object>), BindingMode.OneWay, propertyChanged: ItemsSourceChanged);
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ToastList), new ToastTemplateSelector());

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
        protected override async void OnChildAdded(Element child)
        {
            var view = child as View;
            view.Opacity = 0;
            base.OnChildAdded(child);
            await view.FadeTo(1, 500, Easing.SinIn);
        }
        private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ToastList)bindable;
            if (oldValue is INotifyCollectionChanged oldObservableCollection)
            {
                oldObservableCollection.CollectionChanged -= control.OnItemsSourceCollectionChanged;
            }
            if (newValue is INotifyCollectionChanged newObservableCollection)
            {
                newObservableCollection.CollectionChanged += control.OnItemsSourceCollectionChanged;
            }
            control.Children.Clear();
            if (newValue != null)
            {
                control.Inflate((IEnumerable)newValue);
            }
        }
        private async void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null && e.OldItems == null)
            {
                Children.Clear();
                ForceInvalidateLayout();
                return;
            }
            if (e.OldItems != null && Children.Count > e.OldStartingIndex)
            {
                var view = Children[e.OldStartingIndex];
                await view.FadeTo(0, 500).ContinueWith(t =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            Children.RemoveAt(e.OldStartingIndex);
                            ForceInvalidateLayout();
                        }
                        catch (System.Exception ex)
                        {
                            //the view can already been removed by auto dismiss
                            Debug.WriteLine(ex.Message);
                        }
                    });
                });
            }
            if (e.NewItems == null) return;
            for (var i = 0; i < e.NewItems.Count; ++i)
            {
                var item = e.NewItems[i];
                var view = CreateView(item, i);
                Children.Insert(i + e.NewStartingIndex, view);
            }
            ForceInvalidateLayout();
        }
        private void ForceInvalidateLayout()
        {
            UpdateChildrenLayout();
            InvalidateLayout();
        }
        private void Inflate(IEnumerable items)
        {
            if (ItemsSource == null || ItemTemplate == null) return;
            var i = 0;
            foreach (var item in items)
            {
                var existingChild = Children.ElementAtOrDefault(i);
                if (existingChild != null)
                {
                    if (existingChild.BindingContext.GetType() == item.GetType())
                    {
                        existingChild.BindingContext = item;
                        i++;
                        continue;
                    }
                    Children.Remove(existingChild);
                    Children.Insert(i, CreateView(item, i));
                    i++;
                    continue;
                }
                Children.Add(CreateView(item, i));
                i++;
            }
            UpdateChildrenLayout();
            InvalidateLayout();
        }
        private View CreateView(object item, int index)
        {
            var template = GetDataTemplate(item);
            var element = (Element)template.CreateContent();
            var view = GetView(element);
            view.BindingContext = item;
            var swipe = new SwipeView
            {
                Content = view,
                BackgroundColor = Color.Transparent,
                LeftItems = new SwipeItems
                {
                    new SwipeItem { Text = "", BackgroundColor = Color.Transparent }
                },
                BottomItems = new SwipeItems { new SwipeItem { Text = "", BackgroundColor = Color.Transparent } }
            };
            swipe.SwipeEnded += Swipe_SwipeEnded;
            swipe.SwipeChanging += Swipe_SwipeChanging;
            var viewCommand = AttachedProperties.TappedGestureAttached.GetCommand((view as ContentView).Content);
            var viewCommandParameter = AttachedProperties.TappedGestureAttached.GetCommandParameter((view as ContentView).Content);
            if (viewCommand != null)
                AttachedProperties.TappedGestureAttached.SetCommand(swipe, viewCommand);
            if (viewCommandParameter != null)
                AttachedProperties.TappedGestureAttached.SetCommandParameter(swipe, viewCommandParameter);
            return swipe;
        }
        private void Swipe_SwipeChanging(object sender, SwipeChangingEventArgs e)
        {
            _swipeOffset = e.Offset;
        }
        private async void Swipe_SwipeEnded(object sender, SwipeEndedEventArgs e)
        {
            var swipeView = sender as SwipeView;
            if (e.SwipeDirection == SwipeDirection.Right)
            {
                if (Math.Abs(_swipeOffset) < MINIMUM_SWIPE_LEFT_OFFSET)
                    return;

                await swipeView.TranslateTo(swipeView.X + swipeView.Width + 100, 0, 250, Easing.SinOut)
                    .ContinueWith(t => { Device.BeginInvokeOnMainThread(() => DismissToast(swipeView)); });
                return;
            }

            if (Math.Abs(_swipeOffset) < MINIMUM_SWIPE_UP_OFFSET)
                return;

            await swipeView.TranslateTo(0, -swipeView.Y - swipeView.Height - 100, 250, Easing.SinOut)
                .ContinueWith(t => { Device.BeginInvokeOnMainThread(() => DismissToast(swipeView)); });
        }

        private void DismissToast(SwipeView swipeView)
        {
            var toastService = ((App)Application.Current).Container.Resolve<IToastService>();
            var itemToRemove = (ToastViewModel)swipeView.Content.BindingContext;
            toastService.Dismiss(itemToRemove);
            Children.Remove(swipeView);
        }

        private View GetView(Element element)
        {
            if (element is ViewCell viewCell)
            {
                return viewCell.View;
            }
            return (View)element;
        }
        private DataTemplate GetDataTemplate(object itemSource)
        {
            if (ItemTemplate is DataTemplateSelector selector)
            {
                return selector.SelectTemplate(itemSource, this);
            }
            return ItemTemplate;
        }
    }
}
using BeautyPortionAdmin.Extensions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Controls.Toast
{
    public enum ToastType
    {
        Information,
        Success,
        Warning,
        Error,
        Notification
    }

    public class ToastViewModel : IDisposable
    {
        private readonly Action _onTappedAction;
        private readonly Action<ToastViewModel> _onDismissAction;
        private readonly BehaviorSubject<bool> _canExecuteTapped = new BehaviorSubject<bool>(true);
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ToastViewModel(string title, ToastType type, Action onTappedAction, Action<ToastViewModel> dismissAction, LayoutOptions textAlignment)
        {
            _onTappedAction = onTappedAction;
            _onDismissAction = dismissAction;
            Title = title;
            Type = type;
            TextAlignment = textAlignment;

            TappedCommand = new ReactiveCommand(_canExecuteTapped)
                .WithSubscribe(OnTapped, _disposables);
        }

        public string Title { get; }
        public ToastType Type { get; }
        public LayoutOptions TextAlignment { get; }
        public ReactiveCommand TappedCommand { get; }

        private void OnTapped()
        {
            if (_onTappedAction != null)
            {
                _canExecuteTapped.OnNext(false);
                _onTappedAction?.Invoke();
                _onDismissAction?.Invoke(this);
                _canExecuteTapped.OnNext(true);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class NotificationToastViewModel : ToastViewModel
    {
        public NotificationToastViewModel(string title, string text, ToastType type, Action onTappedAction, Action<ToastViewModel> dismissAction, LayoutOptions textAlignment)
        : base(title, type, onTappedAction, dismissAction, textAlignment)
        {
            Text = text;
        }

        public string Text { get; }
    }
}

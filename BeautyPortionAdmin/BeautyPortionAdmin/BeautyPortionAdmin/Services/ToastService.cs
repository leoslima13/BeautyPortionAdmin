using BeautyPortionAdmin.Bootstraping;
using BeautyPortionAdmin.Controls.Toast;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Services
{
    public interface IToastService
    {
        ReactiveCollection<ToastViewModel> Items { get; }
        void Information(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null);
        void Success(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null);
        void Warning(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null);
        void Error(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null);
        void Notification(string title, string text, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null);
        void Dismiss(ToastViewModel viewModel);
    }

    [Singleton]
    public class ToastService : IToastService
    {
        private readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(4);

        public ReactiveCollection<ToastViewModel> Items { get; } = new ReactiveCollection<ToastViewModel>();

        public void Information(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null)
        {
            Show(new ToastViewModel(title, ToastType.Information, onTappedAction, Dismiss, textAlignment ?? LayoutOptions.Center), durationInSeconds, onDismiss);
        }

        public void Success(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null)
        {
            Show(new ToastViewModel(title, ToastType.Success, onTappedAction, Dismiss, textAlignment ?? LayoutOptions.Center), durationInSeconds, onDismiss);
        }

        public void Warning(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null)
        {
            Show(new ToastViewModel(title, ToastType.Warning, onTappedAction, Dismiss, textAlignment ?? LayoutOptions.Center), durationInSeconds, onDismiss);
        }

        public void Error(string title, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null)
        {
            Show(new ToastViewModel(title, ToastType.Error, onTappedAction, Dismiss, textAlignment ?? LayoutOptions.Center), durationInSeconds, onDismiss);
        }

        public void Notification(string title, string text, Action onTappedAction = null, double? durationInSeconds = null, LayoutOptions? textAlignment = null, Action onDismiss = null)
        {
            Show(new NotificationToastViewModel(title, text, ToastType.Notification, onTappedAction, Dismiss, textAlignment ?? LayoutOptions.Center), durationInSeconds, onDismiss);
        }

        private void Show(ToastViewModel viewModel, double? durationInSeconds = null, Action onDismiss = null)
        {
            Items.Insert(0, viewModel);
            AutoDismiss(viewModel, durationInSeconds, onDismiss);
        }

        private void AutoDismiss(ToastViewModel viewModel, double? durationInSeconds = null, Action onDismiss = null)
        {
            var duration = durationInSeconds.HasValue
                ? TimeSpan.FromSeconds(durationInSeconds.Value)
                : _defaultDuration;

            Observable.Timer(duration).Subscribe(x => { Dismiss(viewModel); onDismiss?.Invoke(); });
        }

        public void Dismiss(ToastViewModel viewModel)
        {
            Items.RemoveOnScheduler(viewModel);
        }
    }
}

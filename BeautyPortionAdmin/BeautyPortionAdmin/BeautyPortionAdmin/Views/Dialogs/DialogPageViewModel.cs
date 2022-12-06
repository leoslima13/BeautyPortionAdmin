using System;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Services;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Views.Dialogs
{
    public class DialogPageViewModel : ViewModelBase
    {
        private readonly IPopupPageService _dialogPageService;
        private DialogPageParameter _param;

        public DialogPageViewModel(IPopupPageService dialogPageService) : base(null)
        {
            _dialogPageService = dialogPageService;

            Message = new ReactiveProperty<string>().AddTo(Disposables);
            AcceptButtonText = new ReactiveProperty<string>().AddTo(Disposables);
            CancelButtonText = new ReactiveProperty<string>().AddTo(Disposables);
            CanAccept = new ReactiveProperty<bool>().AddTo(Disposables);
            CanCancel = new ReactiveProperty<bool>().AddTo(Disposables);
            CanClose = new ReactiveProperty<bool>().AddTo(Disposables);

            AcceptCommand = new ReactiveCommand(CanAccept);
            AcceptCommand.Subscribe(OnAccept).AddTo(Disposables);

            CancelCommand = new ReactiveCommand(CanCancel);
            CancelCommand.Subscribe(OnCancel).AddTo(Disposables);

            CloseCommand = new ReactiveCommand(CanClose);
            CloseCommand.Subscribe(OnClose).AddTo(Disposables);
        }

        public ReactiveProperty<string> Message { get; }
        public ReactiveProperty<string> AcceptButtonText { get; }
        public ReactiveProperty<string> CancelButtonText { get; }
        public ReactiveProperty<bool> CanAccept { get; }
        public ReactiveProperty<bool> CanCancel { get; }
        public ReactiveProperty<bool> CanClose { get; }

        public ReactiveCommand AcceptCommand { get; }
        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand CloseCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _param = parameters.GetValue<DialogPageParameter>(nameof(DialogPageParameter));

            Title = _param.Title;
            Message.Value = _param.Message;
            AcceptButtonText.Value = _param.AcceptButtonText;
            CancelButtonText.Value = _param.CancelButtonText;
            CanAccept.Value = _param.CanAccept;
            CanCancel.Value = _param.CanCancel;
            CanClose.Value = _param.CanClose;
        }

        private async void OnAccept()
        {
            if (CanAccept.Value != true)
                return;

            CanAccept.Value = false;

            await _dialogPageService.Close();
            _param.OnAccept?.Invoke();

        }
        private async void OnCancel()
        {
            if (CanCancel.Value != true)
                return;

            CanCancel.Value = false;

            await _dialogPageService.Close();
            _param.OnCancel?.Invoke();
        }

        private async void OnClose()
        {
            if (CanClose.Value != true)
                return;

            CanClose.Value = false;

            await _dialogPageService.Close();
            _param.OnClose?.Invoke();
        }

    }

    public class DialogPageParameter
    {
        public DialogPageParameter(string title,
            string message,
            string acceptButtonText,
            string cancelButtonText = null,
            Action onAccept = null,
            Action onCancel = null,
            Action onClose = null,
            bool canAccept = true,
            bool canCancel = true,
            bool canClose = true
        )
        {
            Title = title;
            Message = message;
            OnAccept = onAccept;
            OnCancel = onCancel;
            OnClose = onClose;
            AcceptButtonText = acceptButtonText;
            CancelButtonText = cancelButtonText;
            CanAccept = canAccept;
            CanCancel = canCancel;
            CanClose = canClose;
        }

        public Action OnAccept { get; }
        public Action OnCancel { get; }
        public Action OnClose { get; }
        public string AcceptButtonText { get; }
        public string CancelButtonText { get; }
        public bool CanAccept { get; }
        public bool CanCancel { get; }
        public bool CanClose { get; }
        public string Message { get; }
        public string Title { get; }
    }
}

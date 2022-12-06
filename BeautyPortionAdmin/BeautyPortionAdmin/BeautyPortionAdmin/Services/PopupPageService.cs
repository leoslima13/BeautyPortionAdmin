using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Threading.Tasks;
using BeautyPortionAdmin.Bootstraping;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Views.Dialogs;
using BeautyPortionAdmin.Views.Dialogs.ActionSheet;
using DryIoc;
using Prism.Navigation;
using Reactive.Bindings.Extensions;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

namespace BeautyPortionAdmin.Services
{
    public interface IPopupPageService : IDisposable
    {
        void DisplayAlert(string title,
            string message,
            string acceptButtonText,
            string cancelButtonText,
            Action onAccept,
            Action onCancel,
            bool canAccept = true,
            bool canCancel = true,
            bool canClose = true,
            Action onClose = null);

        void DisplayActionSheet(string title, Action<ActionSheetOption> onOptionSelected, params ActionSheetOption[] actionSheetOptions);
        Task Close();
        void ShowPopup<T>(INavigationParameters navigationParameters) where T : ViewModelBase;
    }

    [Singleton]
    public class PopupPageService : IPopupPageService
    {
        private readonly IContainer _container;
        private readonly IPopupNavigation _popupNavigation;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public PopupPageService(IContainer container,
                                IPopupNavigation popupNavigation)
        {
            _container = container;
            _popupNavigation = popupNavigation;
        }

        public void DisplayAlert(string title, string message, string acceptButtonText, string cancelButtonText, Action onAccept, Action onCancel, bool canAccept = true, bool canCancel = true, bool canClose = true, Action onClose = null)
        {
            acceptButtonText ??= "OK";
            cancelButtonText ??= "Cancel";

            var param = new DialogPageParameter(title, message, acceptButtonText, cancelButtonText, onAccept, onCancel, onClose, canAccept, canCancel, canClose);
            var navParam = new NavigationParameters { { nameof(DialogPageParameter), param } };

            var page = _container.Resolve<DialogPage>();
            var vm = _container.Resolve<DialogPageViewModel>();
            page.BindingContext = vm;

            vm.Initialize(navParam);

            _popupNavigation.PushAsync(page)
                .ToObservable()
                .Subscribe(x =>
                {
                    vm.OnNavigatedTo(navParam);
                })
                .AddTo(_disposables);
        }

        public void DisplayActionSheet(string title, Action<ActionSheetOption> onOptionSelected, params ActionSheetOption[] actionSheetOptions)
        {
            var param = new DialogActionSheetPageParameter(title, actionSheetOptions.ToList(), onOptionSelected);
            var navParam = new NavigationParameters { { nameof(DialogActionSheetPageParameter), param } };

            var page = _container.Resolve<DialogActionSheetPage>();
            var vm = _container.Resolve<DialogActionSheetPageViewModel>();
            page.BindingContext = vm;

            vm.Initialize(navParam);

            _popupNavigation.PushAsync(page)
                .ToObservable()
                .Subscribe(x =>
                {
                    vm.OnNavigatedTo(navParam);
                })
                .AddTo(_disposables);
        }

        public async Task Close()
        {
            var lastPopup = _popupNavigation.PopupStack.LastOrDefault();
            var lastVm = lastPopup.BindingContext as ViewModelBase;

            lastVm.OnNavigatedFrom(new NavigationParameters());

            await _popupNavigation.RemovePageAsync(lastPopup);

            lastVm.Destroy();
        }

        public void ShowPopup<T>(INavigationParameters navigationParameters) where T : ViewModelBase
        {
            var viewModel = _container.Resolve<T>();
            var vmTypeName = viewModel.GetType().FullName;

            var vmAssemblyName = viewModel.GetType().GetTypeInfo().Assembly.FullName;
            var viewName = vmTypeName.Substring(0, vmTypeName.Length - "ViewModel".Length);
            var viewTypeName = $"{viewName}, {vmAssemblyName}";
            var result = Type.GetType(viewTypeName);

            var page = _container.Resolve(result) as PopupPage;

            page.BindingContext = viewModel;

            viewModel.Initialize(navigationParameters);

            _popupNavigation.PushAsync(page)
                .ToObservable()
                .Subscribe(_ => { viewModel.OnNavigatedTo(navigationParameters); })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

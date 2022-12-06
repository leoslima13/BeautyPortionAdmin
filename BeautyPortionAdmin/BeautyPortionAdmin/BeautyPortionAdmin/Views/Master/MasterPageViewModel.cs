using BeautyPortionAdmin.Controls.Toast;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Services;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Text;

namespace BeautyPortionAdmin.Views.Master
{
    public interface IHasMasterPage
    {
        IMasterPageViewModel MasterPageViewModel { get; }
    }

    public interface IMasterPageViewModel
    {
        ReactiveProperty<bool> IsBackButtonVisible { get; }
        ReactiveProperty<bool> HasNavigationBar { get; }
        ReactiveCommand GoBackCommand { get; }
        ReactiveCollection<ToastViewModel> ToastItems { get; }
    }

    public class MasterPageViewModel : IMasterPageViewModel
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly INavigationService _navigationService;

        public MasterPageViewModel(INavigationService navigationService,
                                   IToastService toastService)
        {
            _navigationService = navigationService;

            ToastItems = toastService.Items;
            IsBackButtonVisible = new ReactiveProperty<bool>(true).AddTo(_disposables);
            HasNavigationBar = new ReactiveProperty<bool>(true).AddTo(_disposables);            

            GoBackCommand = new ReactiveCommand()
                .WithSubscribe(OnPageBackCommand, _disposables);
        }

        public ReactiveCollection<ToastViewModel> ToastItems { get; }
        public ReactiveProperty<bool> IsBackButtonVisible { get; }
        public ReactiveProperty<bool> HasNavigationBar { get; }        
        public ReactiveCommand GoBackCommand { get; }

        private void OnPageBackCommand()
        {
            _navigationService.GoBackAsync()
                .ToObservable()
                .ObserveOnUIDispatcher()
                .Subscribe()
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

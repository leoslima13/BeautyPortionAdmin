using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Views.Login
{
    public class LoginPageViewModel : ViewModelBase, IHasFieldValidators, IHasMasterPage
    {
        public LoginPageViewModel(INavigationService navigationService,
                                  IFieldValidatorsObservable fieldValidatorsObservable,
                                  IMasterPageViewModel masterPageViewModel) : base(navigationService)
        {
            FieldValidatorsObservable = fieldValidatorsObservable;
            MasterPageViewModel = masterPageViewModel;

            MasterPageViewModel.HasNavigationBar.Value = false;

            Username = new ReactiveProperty<string>().AddTo(Disposables);
            Password = new ReactiveProperty<string>().AddTo(Disposables);

            LoginCommand = FieldValidatorsObservable
                .ObserveFieldHasErrors
                .Inverse()
                .ToReactiveCommand()
                .AddTo(Disposables);

            LoginCommand
                .Subscribe(OnLoginCommand).AddTo(Disposables);

        }

        public IMasterPageViewModel MasterPageViewModel { get; }

        public IFieldValidatorsObservable FieldValidatorsObservable { get; }

        public ReactiveProperty<string> Username { get; }
        public ReactiveProperty<string> Password { get; }

        public ReactiveCommand LoginCommand { get; }

        void OnLoginCommand()
        {
            NavigationService.NavigateAsync(Pages.HomePage.AsNavigationAbsolute())
                .ToObservable()
                .Subscribe()
                .AddTo(Disposables);
        }
    }
}

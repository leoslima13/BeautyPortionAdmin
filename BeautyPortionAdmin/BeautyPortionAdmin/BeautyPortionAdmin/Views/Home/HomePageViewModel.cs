using System;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Views.Home.Tabs;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Views.Home
{
    public class HomePageViewModel : ViewModelBase, IHasMasterPage
    {
        private readonly Lazy<ClientsTabViewModel> _clientsTabViewModel;
        private readonly Lazy<OrdersTabViewModel> _ordersTabViewModel;
        private readonly Lazy<ProductsTabViewModel> _productsTabViewModel;
        private readonly Lazy<ReportsTabViewModel> _reportsTabViewModel;
        private ViewModelBase _currentTab;

        public HomePageViewModel(INavigationService navigationService,
                                 IMasterPageViewModel masterPageViewModel,
                                 Lazy<ClientsTabViewModel> clientsTabViewModel,
                                 Lazy<OrdersTabViewModel> ordersTabViewModel,
                                 Lazy<ProductsTabViewModel> productsTabViewModel,
                                 Lazy<ReportsTabViewModel> reportsTabViewModel) : base(navigationService)
        {
            MasterPageViewModel = masterPageViewModel;
            _clientsTabViewModel = clientsTabViewModel;
            _ordersTabViewModel = ordersTabViewModel;
            _productsTabViewModel = productsTabViewModel;
            _reportsTabViewModel = reportsTabViewModel;

            MasterPageViewModel.HasNavigationBar.Value = false;

            SelectedIndexView = new ReactiveProperty<int>(0)
                .AddTo(Disposables);

            ClientsTabViewModel = new ReactiveProperty<ClientsTabViewModel>().AddTo(Disposables);
            OrdersTabViewModel = new ReactiveProperty<OrdersTabViewModel>().AddTo(Disposables);
            ProductsTabViewModel = new ReactiveProperty<ProductsTabViewModel>().AddTo(Disposables);
            ReportsTabViewModel = new ReactiveProperty<ReportsTabViewModel>().AddTo(Disposables);

            SelectedIndexView
                .Subscribe(OnSelectedIndexChanged)
                .AddTo(Disposables);

            AddOrderCommand = new ReactiveCommand()
                .WithSubscribe(OnAddOrderCommand, Disposables);
        }

        public IMasterPageViewModel MasterPageViewModel { get; }

        public ReactiveProperty<int> SelectedIndexView { get; }
        public ReactiveProperty<ClientsTabViewModel> ClientsTabViewModel { get; }
        public ReactiveProperty<OrdersTabViewModel> OrdersTabViewModel { get; }
        public ReactiveProperty<ProductsTabViewModel> ProductsTabViewModel { get; }
        public ReactiveProperty<ReportsTabViewModel> ReportsTabViewModel { get; }

        public ReactiveCommand AddOrderCommand { get; }

        private void OnSelectedIndexChanged(int index)
        {
            var navigationParameters = new NavigationParameters();
            if (_currentTab != null)
                _currentTab.OnNavigatedFrom(navigationParameters);

            switch (index)
            {
                case 0:
                    InitializeViewModel(ClientsTabViewModel, _clientsTabViewModel, navigationParameters);
                    break;
                case 1:
                    InitializeViewModel(OrdersTabViewModel, _ordersTabViewModel, navigationParameters);
                    break;
                case 2:
                    InitializeViewModel(ProductsTabViewModel, _productsTabViewModel, navigationParameters);
                    break;
                case 3:
                    InitializeViewModel(ReportsTabViewModel, _reportsTabViewModel, navigationParameters);
                    break;
            }
        }

        private void InitializeViewModel<T>(ReactiveProperty<T> viewModelProperty, Lazy<T> viewModel, INavigationParameters navigationParameters) where T : ViewModelBase
        {
            if (!viewModel.IsValueCreated)
            {
                viewModelProperty.Value = viewModel.Value;
            }
            Title = viewModelProperty.Value.Title;
            viewModelProperty.Value.OnNavigatedTo(navigationParameters);
            _currentTab = viewModelProperty.Value;
        }

        void OnAddOrderCommand()
        {
            NavigationService.NavigateAsync(Pages.OrderDetailPage)
                .ToObservable()
                .Subscribe()
                .AddTo(Disposables);
        }
    }
}

using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Xamarin.Forms;
using System.Reactive.Threading.Tasks;

namespace BeautyPortionAdmin.Views.Home.Tabs
{
    public class OrdersTabViewModel : ViewModelBase
    {
        private readonly IClientObservable _clientObservable;
        private readonly BusyNotifier _busyNotifier = new BusyNotifier();
        private Func<OrderViewModel, bool> _collectionFilter => f => string.IsNullOrWhiteSpace(SearchCriteria.Value) ||
                                                                 f.ClientName.Value.ToLower().StartsWith(SearchCriteria.Value.ToLower());

        public OrdersTabViewModel(INavigationService navigationService,
                                  IOrderObservable orderObservable,
                                  IClientObservable clientObservable) : base(navigationService)
        {
            Title = "Pedidos";
            _clientObservable = clientObservable;

            SearchCriteria = new ReactiveProperty<string>().AddTo(Disposables);

            Orders = orderObservable.ObserveOrders
                .Select(CreateOrderViewModel)
                .ToReactiveProperty()
                .AddTo(Disposables);

            SearchCriteria.Throttle(TimeSpan.FromMilliseconds(250))
                .WhereNotNull()
                .Subscribe(_ => Orders.Value.Filtered?.Refresh(_collectionFilter))
                .AddTo(Disposables);

            ClearCriteriaCommand = new ReactiveCommand()
                .WithSubscribe(() => SearchCriteria.Value = string.Empty, Disposables);

            AddNewOrderCommand = new ReactiveCommand()
                .WithSubscribe(OnAddNewOrderCommand, Disposables);
        }

        public ReactiveProperty<bool> IsBusy { get; }
        public ReactiveProperty<FilteredCollection<OrderViewModel>> Orders { get; }
        public ReactiveProperty<string> SearchCriteria { get; }
        public ReactiveCommand ClearCriteriaCommand { get; }
        public ReactiveCommand AddNewOrderCommand { get; }

        private FilteredCollection<OrderViewModel> CreateOrderViewModel(IEnumerable<Order> orders)
        {
            return new FilteredCollection<OrderViewModel>(orders.Select(x => new OrderViewModel(x, _clientObservable, NavigationService).AddTo(Disposables)).ToList(), _collectionFilter);
        }

        private void OnAddNewOrderCommand()
        {
            var busy = _busyNotifier.ProcessStart();

            NavigationService.NavigateAsync(Pages.OrderDetailPage)
                .ToObservable()
                .Subscribe(_ => { busy.Dispose(); })
                .AddTo(Disposables);
        }
    }

    public class OrderViewModel : BindableObject, IDisposable, IHasAlternateRowStyle
    {
        private readonly Order _order;
        private readonly BusyNotifier _busyNotifier = new BusyNotifier();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IClientObservable _clientObservable;
        private readonly INavigationService _navigationService;

        public OrderViewModel(Order order,
                              IClientObservable clientObservable,
                              INavigationService navigationService)
        {
            _order = order;
            _clientObservable = clientObservable;
            _navigationService = navigationService;

            HasAlternateRowStyle = new ReactiveProperty<bool>().AddTo(_disposables);

            ClientName = _clientObservable.ObserveItems
                .Where(x => x.Any(y => y.Id == order.ClientId))
                .Select(x => x.FirstOrDefault(y => y.Id == order.ClientId))
                .Select(x => $"{x.FirstName} {x.LastName}")
                .ToReactiveProperty()
                .AddTo(_disposables);

            GoToDetailsCommand = _busyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnGoToDetailsCommand, _disposables);
        }

        public Guid Id => _order.Id;
        public ReactiveProperty<string> ClientName { get; }
        public OrderStatus OrderStatus => _order.OrderStatus;
        public PaymentStatus PaymentStatus => _order.PaymentStatus;

        public ReactiveProperty<bool> HasAlternateRowStyle { get; }
        public ReactiveCommand GoToDetailsCommand { get; }


        void OnGoToDetailsCommand()
        {

        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

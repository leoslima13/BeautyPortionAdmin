using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Xamarin.Forms;
using System.Reactive.Threading.Tasks;

namespace BeautyPortionAdmin.Views.Orders
{
    public class OrderSummaryStepPageViewModel : ViewModelBase, IHasMasterPage
    {
        private readonly IOrderObservable _orderObservable;

        public OrderSummaryStepPageViewModel(INavigationService navigationService,
                                             IMasterPageViewModel masterPageViewModel,
                                             IOrderObservable orderObservable)
            : base(navigationService)
        {
            Title = "Resumo do Pedido";
            MasterPageViewModel = masterPageViewModel;
            _orderObservable = orderObservable;

            BusyNotifier = new BusyNotifier();
            ClientInitials = new ReactiveProperty<string>().AddTo(Disposables);
            ClientName = new ReactiveProperty<string>().AddTo(Disposables);
            ClientPhoto = new ReactiveProperty<ImageSource>().AddTo(Disposables);

            orderObservable
                .ObserveSelectedClient
                .Take(1)
                .Subscribe(OnSelectedClient)
                .AddTo(Disposables);

            PaymentMethod = orderObservable
                .ObservePaymentType
                .Where(x => x.HasValue)
                .Take(1)
                .Select(x => x.Value.ToPaymentDescription())
                .ToReactiveProperty()
                .AddTo(Disposables);

            DeliveryMethod = orderObservable
                .ObserveDeliveryMode
                .Where(x => x.HasValue)
                .Take(1)
                .Select(x => x.Value.ToDeliveryDescription())
                .ToReactiveProperty()
                .AddTo(Disposables);

            Freight = orderObservable
                .ObserveFreight
                .Take(1)
                .ToReactiveProperty()
                .AddTo(Disposables);

            Address = orderObservable
                .ObserveAddress
                .Take(1)
                .ToReactiveProperty()
                .AddTo(Disposables);

            SelectedProducts = orderObservable
                .ObserveSelectedProducts
                .Where(x => x != null && x.Any())
                .Select(CreateSummaryProductsViewModel)
                .ToReactiveProperty()
                .AddTo(Disposables);

            TotalOrder = SelectedProducts
                .Select(x => x.Select(p => p.TotalFractions.Value))
                .CombineLatest(Freight, (totalFractions, freight) =>
                {
                    var f = freight ?? 0;
                    return totalFractions.Sum() + f;
                })
                .ToReactiveProperty()
                .AddTo(Disposables);

            ConfirmCommand = BusyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnConfirmCommand, Disposables);

        }

        public IMasterPageViewModel MasterPageViewModel { get; }
        public BusyNotifier BusyNotifier { get; }

        public ReactiveProperty<string> ClientInitials { get; }
        public ReactiveProperty<string> ClientName { get; }
        public ReactiveProperty<ImageSource> ClientPhoto { get; }
        public ReactiveProperty<IEnumerable<SummaryProductViewModel>> SelectedProducts { get; }
        public ReactiveProperty<string> PaymentMethod { get; }
        public ReactiveProperty<string> DeliveryMethod { get; }
        public ReactiveProperty<double?> Freight { get; set; }
        public ReactiveProperty<string> Address { get; }
        public ReactiveProperty<double> TotalOrder { get; set; }

        public ReactiveCommand ConfirmCommand { get; }

        IEnumerable<SummaryProductViewModel> CreateSummaryProductsViewModel(IEnumerable<Product> products)
        {
            return products.Select(x => new SummaryProductViewModel(x, _orderObservable).AddTo(Disposables)).ToList();
        }

        void OnSelectedClient(Models.Client client)
        {
            ClientInitials.Value = $"{client.FirstName[0]}{client.LastName[0]}".ToUpper();
            ClientName.Value = $"{client.FirstName} {client.LastName}";
            ClientPhoto.Value = client.Photo?.ToImageSource();
        }

        void OnConfirmCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            _orderObservable.CreateOrder();

            NavigationService.GoBackToRootAsync()
                .ToObservable()
                .Finally(busy.Dispose)
                .Subscribe()
                .AddTo(Disposables);
        }
    }

    public class SummaryProductViewModel : IDisposable
    {
        private readonly Product _product;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public SummaryProductViewModel(Product product,
                                       IOrderObservable orderObservable)
        {
            _product = product;

            SelectedFractions = orderObservable
                .ObserveSelectedFractions
                .Where(x => x != null && x.Any(f => f.ProductId == product.Id))
                .Select(x => x.Where(f => f.ProductId == product.Id))
                .ToReactiveProperty()
                .AddTo(_disposables);

            TotalFractions = SelectedFractions
                .WhereNotNull()
                .Select(x => x.Select(x => x.Price * x.Quantity).Sum())
                .ToReactiveProperty()
                .AddTo(_disposables);
        }

        public string ProductInitials => $"{_product.Name[0]}";
        public ImageSource ProductPhoto => _product.ProductPhoto?.ToImageSource();
        public string ProductName => _product.Name;

        public ReactiveProperty<IEnumerable<FractionInOrder>> SelectedFractions { get; }
        public ReactiveProperty<double> TotalFractions { get; }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

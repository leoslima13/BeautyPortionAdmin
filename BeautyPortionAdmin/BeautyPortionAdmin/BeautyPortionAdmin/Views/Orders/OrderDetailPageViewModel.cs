using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Services;
using BeautyPortionAdmin.Views.Dialogs.Picker;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Xamarin.Forms;
using System.Reactive.Threading.Tasks;
using System.Reactive.Disposables;
using BeautyPortionAdmin.Helpers;

namespace BeautyPortionAdmin.Views.Orders
{
    public class OrderDetailPageViewModel : ViewModelBase, IHasMasterPage
    {
        private readonly IOrderObservable _orderObservable;

        public OrderDetailPageViewModel(INavigationService navigationService,
                                        IMasterPageViewModel masterPageViewModel,
                                        IClientObservable clientObservable,
                                        IProductObservable productObservable,
                                        IOrderObservable orderObservable,
                                        IPopupPageService popupPageService) : base(navigationService)
        {
            Title = "Novo Pedido";
            MasterPageViewModel = masterPageViewModel;
            _orderObservable = orderObservable;
            PopupPageService = popupPageService;

            SelectedClients = new ReactiveProperty<IEnumerable<ISelectableItem>>().AddTo(Disposables);
            SelectedClient = new ReactiveProperty<SelectableClient>().AddTo(Disposables);
            SelectedProducts = new ReactiveProperty<IEnumerable<ISelectableItem>>().AddTo(Disposables);
            OpenPickerCommand = new ReactiveCommand().AddTo(Disposables);
            OpenPickerProductsCommand = new ReactiveCommand().AddTo(Disposables);

            ChangeSelectedClientCommand = new ReactiveCommand()
                .WithSubscribe(() => OpenPickerCommand.Execute(), Disposables);

            AddMoreProductsCommand = new ReactiveCommand()
                .WithSubscribe(() => OpenPickerProductsCommand.Execute(), Disposables);

            orderObservable.ObserveSelectedClient
                .Take(1)
                .Subscribe(x => OnSelectedClients(new List<ISelectableItem> { new SelectableClient(x).ToSelectableItem(x.FirstName, true) }))
                .AddTo(Disposables);

            Clients = clientObservable.ObserveItems
                .WhereNotNull()
                .Select(x => x.Select(y => new SelectableClient(y).ToSelectableItem($"{y.FirstName} {y.LastName}", false)).ToList())
                .ToReactiveProperty()
                .AddTo(Disposables);

            Products = productObservable.ObserveItems
                .WhereNotNull()
                .Select(x => x.Select(y => new SelectableProduct(y, OnProductRemoved).ToSelectableItem(y.Name, false).AddTo(Disposables)).ToList())
                .ToReactiveProperty()
                .AddTo(Disposables);

            SelectedClients
                .WhereNotNull()
                .Subscribe(OnSelectedClients)
                .AddTo(Disposables);

            NextCommand = SelectedClient
                .CombineLatest(SelectedProducts, (client, products) => client != null && products != null && products.Any())
                .ToReactiveCommand()
                .WithSubscribe(OnNextCommand, Disposables);
        }

        public IMasterPageViewModel MasterPageViewModel { get; }
        public IPopupPageService PopupPageService { get; }
        public ReactiveProperty<List<ISelectableItem>> Clients { get; }
        public ReactiveProperty<IEnumerable<ISelectableItem>> SelectedClients { get; }
        public ReactiveProperty<SelectableClient> SelectedClient { get; }
        public ReactiveProperty<List<ISelectableItem>> Products { get; }
        public ReactiveProperty<IEnumerable<ISelectableItem>> SelectedProducts { get; }

        public ReactiveCommand OpenPickerCommand { get; }
        public ReactiveCommand OpenPickerProductsCommand { get; }
        public ReactiveCommand ChangeSelectedClientCommand { get; }
        public ReactiveCommand AddMoreProductsCommand { get; }
        public ReactiveCommand NextCommand { get; }

        public override void Destroy()
        {
            _orderObservable.ResetValues();
            base.Destroy();
        }

        void OnSelectedClients(IEnumerable<ISelectableItem> selectedItems)
        {
            SelectedClient.Value = selectedItems.First().Value as SelectableClient;
            _orderObservable.SetClient(SelectedClient.Value.Client);
        }

        void OnProductRemoved(Product product)
        {
            var selectableItem = SelectedProducts.Value.FirstOrDefault(x => (x.Value as SelectableProduct).Id == product.Id);
            selectableItem.IsSelected.Value = false;
            var products = SelectedProducts.Value.ToList();
            products.Remove(selectableItem);
            SelectedProducts.Value = products;
            _orderObservable.RemoveProduct(product);
        }

        void OnNextCommand()
        {
            _orderObservable.AddProducts(SelectedProducts.Value.Select(x => (x.Value as SelectableProduct).Product).ToList());
            NavigationService.NavigateAsync(Pages.OrderFractionStepPage)
                .ToObservable()
                .Subscribe()
                .AddTo(Disposables);
        }
    }

    public class SelectableClient
    {
        public SelectableClient(Models.Client client)
        {
            Client = client;
        }

        public Models.Client Client { get; }
        public string ClientInitials => $"{Client.FirstName[0]}{Client.LastName[0]}".ToUpper();
        public ImageSource ClientPhoto => Client.Photo?.ToImageSource();
        public string ClientName => $"{Client.FirstName} {Client.LastName}";
    }

    public class SelectableProduct : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public SelectableProduct(Product product, Action<Product> onProductRemoved)
        {
            Product = product;

            RemoveCommand = new ReactiveCommand()
                .WithSubscribe(() => onProductRemoved(product), _disposables);
        }

        public Product Product { get; }
        public Guid Id => Product.Id;
        public string ProductInitials => $"{Product.Name[0]}".ToUpper();
        public ImageSource ProductPhoto => Product.ProductPhoto?.ToImageSource();
        public string ProductName => Product.Name;

        public ReactiveCommand RemoveCommand { get; }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

}

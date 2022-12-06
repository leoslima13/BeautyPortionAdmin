using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Home.Tabs
{
    public class ProductsTabViewModel : ViewModelBase
    {
        private readonly IProductObservable _productObservable;
        private readonly BusyNotifier _busyNotifier = new BusyNotifier();

        public ProductsTabViewModel(INavigationService navigationService,
                                    IProductObservable productObservable)
            : base(navigationService)
        {
            Title = "Produtos";

            _productObservable = productObservable;

            SearchCriteria = new ReactiveProperty<string>().AddTo(Disposables);

            Products = _productObservable.ObserveItems
                .Select(CreateProductViewModel)
                .ToReactiveProperty()
                .AddTo(Disposables);

            SearchCriteria.Throttle(TimeSpan.FromMilliseconds(250))
                .WhereNotNull()
                .Subscribe(_ => Products.Value.Filtered?.Refresh(_collectionFilter))
                .AddTo(Disposables);

            ClearCriteriaCommand = new ReactiveCommand()
                .WithSubscribe(() => SearchCriteria.Value = string.Empty, Disposables);

            AddNewProductCommand = new ReactiveCommand()
                .WithSubscribe(OnAddNewProductCommand, Disposables);
        }

        public ReactiveProperty<bool> IsBusy { get; }
        public ReactiveProperty<FilteredCollection<ProductViewModel>> Products { get; }
        public ReactiveProperty<string> SearchCriteria { get; }
        public ReactiveCommand ClearCriteriaCommand { get; }
        public ReactiveCommand AddNewProductCommand { get; }

        private Func<ProductViewModel, bool> _collectionFilter => f => string.IsNullOrWhiteSpace(SearchCriteria.Value) ||
                                                                 f.Name.ToLower().StartsWith(SearchCriteria.Value.ToLower());

        private FilteredCollection<ProductViewModel> CreateProductViewModel(IEnumerable<Product> products)
        {
            return new FilteredCollection<ProductViewModel>(products.Select(x => new ProductViewModel(x, NavigationService).AddTo(Disposables)).ToList(), _collectionFilter);
        }

        private void OnAddNewProductCommand()
        {
            var busy = _busyNotifier.ProcessStart();

            NavigationService.NavigateAsync(Pages.AddProductPage)
                .ToObservable()
                .Subscribe(_ => { busy.Dispose(); })
                .AddTo(Disposables);
        }
    }

    public class ProductViewModel : BindableBase, IDisposable
    {
        private readonly Product _product;
        private readonly INavigationService _navigationService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BusyNotifier _busyNotifier = new BusyNotifier();

        public ProductViewModel(Product product, INavigationService navigationService)
        {
            _product = product;
            _navigationService = navigationService;

            GoToDetailsCommand = _busyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnGoToDetailsCommand, _disposables);
        }

        public string Name => _product.Name;
        public string Initials => _product.Name[0].ToString();
        public int Quantity => _product.Quantity;
        public double Weight => _product.Weight;
        public ImageSource Photo => _product.ProductPhoto?.ToImageSource();

        public ReactiveCommand GoToDetailsCommand { get; }

        private void OnGoToDetailsCommand()
        {
            var busy = _busyNotifier.ProcessStart();

            var parameter = new NavigationParameters
            {
                { nameof(Product), _product }
            };

            _navigationService.NavigateAsync(Pages.ProductDetailPage, parameter)
                .ToObservable()
                .Subscribe(_ => { busy.Dispose(); })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

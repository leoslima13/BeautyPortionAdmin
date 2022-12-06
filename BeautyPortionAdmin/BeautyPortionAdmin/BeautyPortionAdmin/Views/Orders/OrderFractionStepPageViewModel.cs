using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Services;
using BeautyPortionAdmin.Views.Dialogs.Picker;
using BeautyPortionAdmin.Views.Master;
using BeautyPortionAdmin.Views.Orders.DataTemplates;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Orders
{
    public class OrderFractionStepPageViewModel : ViewModelBase, IHasMasterPage
    {
        private readonly IOrderObservable _orderObservable;
        private readonly IPopupPageService _popupPageService;
        private readonly IProductObservable _productObservable;

        public OrderFractionStepPageViewModel(INavigationService navigationService,
                                              IMasterPageViewModel masterPageViewModel,
                                              IOrderObservable orderObservable,
                                              IPopupPageService popupPageService,
                                              IProductObservable productObservable)
            : base(navigationService)
        {
            Title = "Seleção de Frações";

            _orderObservable = orderObservable;
            _popupPageService = popupPageService;
            _productObservable = productObservable;
            MasterPageViewModel = masterPageViewModel;

            SelectedProducts = orderObservable.ObserveSelectedProducts
                .WhereNotNull()
                .Select(CreateProductsViewModel)
                .ToReactiveProperty()
                .AddTo(Disposables);

            NextCommand = SelectedProducts
                .WhereNotNull()
                .Where(x => x != null && x.Any())
                .Select(ObserveSelectedFractions)
                .Switch()
                .Select(_ => SelectedProducts.Value.All(x => x.HasFractions.Value))
                .ToReactiveCommand()
                .WithSubscribe(OnNextCommand, Disposables);
        }

        public IMasterPageViewModel MasterPageViewModel { get; }

        public ReactiveProperty<IEnumerable<ProductViewModel>> SelectedProducts { get; }

        public ReactiveCommand NextCommand { get; }

        IEnumerable<ProductViewModel> CreateProductsViewModel(IEnumerable<Product> selectedProducts)
        {
            return selectedProducts.Select(x => new ProductViewModel(x, _popupPageService, _orderObservable, _productObservable).AddTo(Disposables)).ToList();
        }

        IObservable<bool> ObserveSelectedFractions(IEnumerable<ProductViewModel> items)
        {
            return items.Select(x => x.HasFractions).Merge();
        }

        void OnNextCommand()
        {
            NavigationService.NavigateAsync(Pages.OrderDetailStepPage)
                .ToObservable()
                .Subscribe()
                .AddTo(Disposables);
        }
    }

    public class ProductViewModel : IDisposable
    {
        private readonly Product _product;
        private readonly IPopupPageService _popupPageService;
        private readonly IOrderObservable _orderObservable;
        private List<IQuantifiableItem> _fractionsOfProduct = new List<IQuantifiableItem>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ProductViewModel(Product product,
                                IPopupPageService popupPageService,
                                IOrderObservable orderObservable,
                                IProductObservable productObservable)
        {
            _product = product;
            _popupPageService = popupPageService;
            _orderObservable = orderObservable;

            productObservable.ObserveFractions
                .Where(x => x != null && x.Any(f => f.ProductId == product.Id))
                .Subscribe(x => OnProductFractions(x.Where(f => f.ProductId == product.Id)))
                .AddTo(_disposables);

            Fractions = orderObservable
                .ObserveSelectedFractions
                .Where(x => x != null && x.Any(f => f.ProductId == product.Id))
                .Select(x => CreateFractionsViewModel(x.Where(f => f.ProductId == product.Id)))
                .ToReactiveProperty()
                .AddTo(_disposables);

            HasFractions = Fractions
                .Select(x => x != null && x.Any())
                .ToReactiveProperty()
                .AddTo(_disposables);

            AddMoreFractionsCommand = new ReactiveCommand()
                .WithSubscribe(OnAddMoreFractionsCommand, _disposables);
        }

        public string ProductInitials => $"{_product.Name[0]}".ToUpper();
        public ImageSource ProductPhoto => _product.ProductPhoto?.ToImageSource();
        public string ProductName => _product.Name;

        public ReactiveProperty<bool> HasFractions { get; }
        public ReactiveProperty<IEnumerable<FractionViewModel>> Fractions { get; }
        public ReactiveCommand AddMoreFractionsCommand { get; }

        void OnProductFractions(IEnumerable<Fraction> fractions)
        {
            _fractionsOfProduct = fractions.Select(x => x.ToFractionInOrder(0).ToQuantifiableItem(x.Weight.ToString(), 0)).ToList();
        }

        IEnumerable<FractionViewModel> CreateFractionsViewModel(IEnumerable<FractionInOrder> fractions)
        {
            return fractions.Select(x => new FractionViewModel(x, _popupPageService, _orderObservable)).ToList();
        }

        void OnAddMoreFractionsCommand()
        {
            var param = new PickerPageParameter("Selecione as Frações", _fractionsOfProduct, new FractionPickerTemplate(),
                                                Controls.PickerSelectionMode.Multiple, OnSelectedFractions, "Busque as frações pelo peso...");
            var parameters = new NavigationParameters
            {
                { nameof(PickerPageParameter), param }
            };

            _popupPageService.ShowPopup<PickerPageViewModel>(parameters);

            void OnSelectedFractions(IEnumerable<IPickerItem> selectedFractions)
            {
                var fractions = selectedFractions.OfType<IQuantifiableItem>().Select(x => new FractionInOrder
                {
                    Id = (x.Value as FractionInOrder).Id,
                    Price = (x.Value as FractionInOrder).Price,
                    ProductId = (x.Value as FractionInOrder).ProductId,
                    Quantity = x.Quantity.Value,
                    Weight = (x.Value as FractionInOrder).Weight
                }).ToList();
                _orderObservable.AddOrUpdateFractions(fractions);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

    public class FractionViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();        
        private readonly IPopupPageService _popupPageService;
        private readonly IOrderObservable _orderObservable;
        private readonly FractionInOrder _fractionInOrder;

        public FractionViewModel(FractionInOrder fractionInOrder,
                                 IPopupPageService popupPageService,
                                 IOrderObservable orderObservable)
        {
            _popupPageService = popupPageService;
            _orderObservable = orderObservable;
            _fractionInOrder = fractionInOrder;

            Price = new ReactiveProperty<double>(fractionInOrder.Price).AddTo(_disposables);
            Quantity = new ReactiveProperty<int>(_fractionInOrder.Quantity).AddTo(_disposables);

            EditFractionCommand = new ReactiveCommand()
                .WithSubscribe(OnEditFractionCommand, _disposables);

            RemoveCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                {
                    Quantity.Value = 0;
                    _orderObservable.RemoveFraction(_fractionInOrder);
                }, _disposables);
        }

        public double Weight => _fractionInOrder.Weight;

        public ReactiveProperty<double> Price { get; }
        public ReactiveProperty<int> Quantity { get; }
        public ReactiveCommand EditFractionCommand { get; }
        public ReactiveCommand RemoveCommand { get; }

        void OnEditFractionCommand()
        {
            var param = new NavigationParameters
            {
                { nameof(EditFractionPageParameter), new EditFractionPageParameter(Price.Value, OnPriceChanged) }
            };

            _popupPageService.ShowPopup<EditFractionPricePageViewModel>(param);

            void OnPriceChanged(double newPrice)
            {
                Price.Value = newPrice;
                _fractionInOrder.Price = newPrice;
                _orderObservable.AddOrUpdateFraction(_fractionInOrder);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

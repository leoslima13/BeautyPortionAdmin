using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Fonts;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Services;
using BeautyPortionAdmin.Views.Dialogs.ActionSheet;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Views.Products
{
    public class ProductDetailPageViewModel : ViewModelBase, IHasMasterPage
    {
        private readonly IProductObservable _productObservable;
        private readonly IPopupPageService _popupPageService;
        private Product _product;

        public ProductDetailPageViewModel(INavigationService navigationService,
                                          IMasterPageViewModel masterPageViewModel,
                                          IProductObservable productObservable,
                                          IPopupPageService popupPageService) : base(navigationService)
        {
            Title = "Detalhe do Produto";
            MasterPageViewModel = masterPageViewModel;
            _productObservable = productObservable;
            _popupPageService = popupPageService;

            Photo = new ReactiveProperty<ImageSource>().AddTo(Disposables);
            ProductName = new ReactiveProperty<string>().AddTo(Disposables);
            Quantity = new ReactiveProperty<int>().AddTo(Disposables);
            Weight = new ReactiveProperty<double>().AddTo(Disposables);
            TotalWeight = new ReactiveProperty<double>().AddTo(Disposables);
            Fractions = new ReactiveProperty<IEnumerable<FractionViewModel>>().AddTo(Disposables);

            Initials = ProductName
                .WhereNotNull()
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x[0].ToString())
                .ToReactiveProperty()
                .AddTo(Disposables);

            AddFractionCommand = new ReactiveCommand()
                .WithSubscribe(OnAddFractionCommand, Disposables);

            ActionMenuCommand = new ReactiveCommand()
                .WithSubscribe(OnActionMenuCommand, Disposables);
        }

        public IMasterPageViewModel MasterPageViewModel { get; }

        public ReactiveProperty<string> Initials { get; }
        public ReactiveProperty<ImageSource> Photo { get; }
        public ReactiveProperty<string> ProductName { get; }
        public ReactiveProperty<int> Quantity { get; }
        public ReactiveProperty<double> Weight { get; }
        public ReactiveProperty<double> TotalWeight { get; }
        public ReactiveProperty<IEnumerable<FractionViewModel>> Fractions { get; }

        public ReactiveCommand AddFractionCommand { get; }
        public ReactiveCommand ActionMenuCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _product = parameters.GetValue<Product>(nameof(Product));

            if (_product == null)
                throw new ArgumentNullException("Product can't be null");

            ProductName.Value = _product.Name;
            Quantity.Value = _product.Quantity;
            Weight.Value = _product.Weight;
            TotalWeight.Value = _product.Quantity * _product.Weight;

            _productObservable.ObserveFractions
                .Subscribe(x => CreateFractionViewModel(x.Where(f => f.ProductId == _product.Id)));
        }

        private void CreateFractionViewModel(IEnumerable<Fraction> items)
        {
            Fractions.Value = new List<FractionViewModel>(items.Select(x => new FractionViewModel(x, _popupPageService).AddTo(Disposables)).ToList());
        }

        void OnAddFractionCommand()
        {
            var param = new NavigationParameters
            {
                { nameof(AddFractionPageParameter), new AddFractionPageParameter(null, _product.Id) }
            };

            _popupPageService.ShowPopup<AddFractionPageViewModel>(param);
        }

        void OnActionMenuCommand()
        {
            _popupPageService.DisplayActionSheet("Ações para o Produto", OnOptionSelected,
                                                 new FaIconActionSheetOption("Editar", FaIcon.Edit),
                                                 new FaIconActionSheetOption("Excluir", FaIcon.Trash));

            void OnOptionSelected(ActionSheetOption selectedOption)
            {
                if(selectedOption.Option == "Editar")
                {
                    var addProductParam = new AddProductPageParameter(_product, OnProductChanged);
                    var navParam = new NavigationParameters
                    {
                        { nameof(AddProductPageParameter), addProductParam }
                    };
                    NavigationService.NavigateAsync(Pages.AddProductPage, navParam)
                        .ToObservable()
                        .Subscribe();
                    return;
                }
            }

            void OnProductChanged(Product product)
            {
                _product = product;

                ProductName.Value = _product.Name;
                Quantity.Value = _product.Quantity;
                Weight.Value = _product.Weight;
                TotalWeight.Value = _product.Quantity * _product.Weight;
                Photo.Value = _product.ProductPhoto?.ToImageSource();
            }
        }
    }

    public class FractionViewModel : IDisposable
    {
        private readonly Fraction _fraction;
        private readonly IPopupPageService _popupPageService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public FractionViewModel(Fraction fraction, IPopupPageService popupPageService)
        {
            _fraction = fraction;
            _popupPageService = popupPageService;

            EditFractionCommand = new ReactiveCommand()
                .WithSubscribe(OnEditFractionCommand, _disposables);
        }

        public double Weight => _fraction.Weight;
        public double Price => _fraction.Price;

        public ReactiveCommand EditFractionCommand { get; }

        void OnEditFractionCommand()
        {
            var param = new NavigationParameters
            {
                { nameof(AddFractionPageParameter), new AddFractionPageParameter(_fraction, _fraction.ProductId) }
            };

            _popupPageService.ShowPopup<AddFractionPageViewModel>(param);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

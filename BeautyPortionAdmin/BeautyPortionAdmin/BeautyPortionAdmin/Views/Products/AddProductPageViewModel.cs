using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Services;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System.Reactive.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media.Abstractions;
using System.IO;

namespace BeautyPortionAdmin.Views.Products
{
    public class AddProductPageViewModel : ViewModelBase, IHasFieldValidators, IHasMasterPage
    {
        private readonly IProductObservable _productObservable;
        private readonly IMediaPickerService _mediaPickerService;
        private readonly IToastService _toastService;
        private byte[] _fileBytes;
        private AddProductPageParameter _productParam;

        public AddProductPageViewModel(INavigationService navigationService,
                                      IMasterPageViewModel masterPageViewModel,
                                      IFieldValidatorsObservable fieldValidatorsObservable,
                                      IProductObservable productObservable,
                                      IMediaPickerService mediaPickerService,
                                      IToastService toastService) : base(navigationService)
        {
            FieldValidatorsObservable = fieldValidatorsObservable;
            MasterPageViewModel = masterPageViewModel;
            _productObservable = productObservable;
            _mediaPickerService = mediaPickerService;
            _toastService = toastService;

            Title = "Adicionar Produto";

            BusyNotifier = new BusyNotifier();
            AddingProduct = new ReactiveProperty<bool>(true).AddTo(Disposables);
            Photo = new ReactiveProperty<ImageSource>().AddTo(Disposables);
            Name = new ReactiveProperty<string>().AddTo(Disposables);
            Quantity = new ReactiveProperty<int?>().AddTo(Disposables);
            Weight = new ReactiveProperty<double?>().AddTo(Disposables);
            Price = new ReactiveProperty<double>().AddTo(Disposables);

            Initials = Name
                .WhereNotNull()
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x[0].ToString())
                .StartWith("BP")
                .ToReactiveProperty()
                .AddTo(Disposables);

            AddProductCommand = FieldValidatorsObservable.ObserveFieldHasErrors
                .Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnAddProductCommand, Disposables);

            EditProductCommand = FieldValidatorsObservable.ObserveFieldHasErrors
                .Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnEditProductCommand, Disposables);

            AddPhotoCommand = new ReactiveCommand()
                .WithSubscribe(OnAddPhotoCommand, Disposables);
        }

        public IFieldValidatorsObservable FieldValidatorsObservable { get; }
        public IMasterPageViewModel MasterPageViewModel { get; }
        public BusyNotifier BusyNotifier { get; }

        public ReactiveProperty<bool> AddingProduct { get; }
        public ReactiveProperty<string> Initials { get; }
        public ReactiveProperty<ImageSource> Photo { get; }
        public ReactiveProperty<string> Name { get; }
        public ReactiveProperty<int?> Quantity { get; }
        public ReactiveProperty<double?> Weight { get; }
        public ReactiveProperty<double> Price { get; }

        public ReactiveCommand AddProductCommand { get; }
        public ReactiveCommand EditProductCommand { get; }
        public ReactiveCommand AddPhotoCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _productParam = parameters.GetValue<AddProductPageParameter>(nameof(AddProductPageParameter));

            if (_productParam == null) return;

            var _product = _productParam.Product;

            AddingProduct.Value = false;
            Title = "Editar Produto";

            Name.Value = _product.Name;
            Quantity.Value = _product.Quantity;
            Weight.Value = _product.Weight;
            Price.Value = _product.Price;
            Photo.Value = _product.ProductPhoto?.ToImageSource();
        }

        private async void OnAddProductCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            _productObservable.AddItem(new Product
            {
                Name = Name.Value,
                Price = Price.Value,
                ProductPhoto = _fileBytes,
                Quantity = Quantity.Value.Value,
                Weight = Weight.Value.Value
            });

            busy.Dispose();
            _toastService.Success("Produto adicionado com sucesso!");
            NavigationService.GoBackAsync().ToObservable().Subscribe();
        }

        private async void OnEditProductCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            var _product = _productParam.Product;

            _product.Name = Name.Value;
            _product.Quantity = Quantity.Value.Value;
            _product.Price = Price.Value;
            _product.ProductPhoto = _fileBytes ?? _product.ProductPhoto;
            _product.Weight = Weight.Value.Value;

            _productObservable.UpdateItem(_product);

            busy.Dispose();

            _toastService.Success("Produto editado com sucesso!");
            NavigationService.GoBackAsync()
                .ToObservable()
                .Subscribe(_ => { _productParam?.OnProductChanged(_product); });
        }

        private void OnAddPhotoCommand()
        {
            _mediaPickerService.PickPhoto()
                .Subscribe(OnMediaPicked)
                .AddTo(Disposables);

            void OnMediaPicked(MediaPickerResult mediaPickerResult)
            {
                if (mediaPickerResult.Status == MediaPickerStatus.Success)
                {
                    Photo.Value = GetImageSource(mediaPickerResult.MediaFile);
                }
            }

            ImageSource GetImageSource(MediaFile mediaFile)
            {
                var stream = mediaFile.GetStreamWithImageRotatedForExternalStorage();
                _fileBytes = CopyStreamToByte(stream);
                mediaFile.Dispose();
                return _fileBytes.ToImageSource();
            }

            byte[] CopyStreamToByte(Stream stream)
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray().ResizeImage();
                }
            }
        }
    }

    public class AddProductPageParameter
    {
        public Product Product { get; }

        public AddProductPageParameter(Product product, Action<Product> onProductChanged)
        {
            Product = product;
            OnProductChanged = onProductChanged;
        }

        public Action<Product> OnProductChanged { get; }
    }
}

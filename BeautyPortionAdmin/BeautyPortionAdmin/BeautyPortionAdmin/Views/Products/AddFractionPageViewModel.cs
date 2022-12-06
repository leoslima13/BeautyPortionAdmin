using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Services;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

namespace BeautyPortionAdmin.Views.Products
{
    public class AddFractionPageViewModel : ViewModelBase, IHasFieldValidators
    {
        private readonly IPopupPageService _popupPageService;
        private readonly IProductObservable _productObservable;
        private Fraction _fraction;
        private Guid _productId;

        public AddFractionPageViewModel(IPopupPageService popupPageService,
                                        IProductObservable productObservable,
                                        IFieldValidatorsObservable fieldValidatorsObservable) : base(null)
        {
            _popupPageService = popupPageService;
            _productObservable = productObservable;
            FieldValidatorsObservable = fieldValidatorsObservable;

            Title = "Adicionar Fração";

            BusyNotifier = new BusyNotifier();
            Fraction = new ReactiveProperty<double?>().AddTo(Disposables);
            Price = new ReactiveProperty<double?>().AddTo(Disposables);
            CanRemove = new ReactiveProperty<bool>().AddTo(Disposables);

            RemoveCommand = BusyNotifier.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnRemoveCommand, Disposables);

            SaveCommand = BusyNotifier
                .CombineLatest(FieldValidatorsObservable.ObserveFieldHasErrors, (isBusy, hasErrors) => !isBusy && !hasErrors)
                .ToReactiveCommand()
                .WithSubscribe(OnEditCommand, Disposables);

            AddCommand = BusyNotifier
                .CombineLatest(FieldValidatorsObservable.ObserveFieldHasErrors, (isBusy, hasErrors) => !isBusy && !hasErrors)
                .ToReactiveCommand()
                .WithSubscribe(OnAddCommand, Disposables);
        }

        public IFieldValidatorsObservable FieldValidatorsObservable { get; }

        public BusyNotifier BusyNotifier { get; }
        public ReactiveProperty<double?> Fraction { get; }
        public ReactiveProperty<double?> Price { get; }
        public ReactiveProperty<bool> CanRemove { get; }

        public ReactiveCommand AddCommand { get; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand RemoveCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            var param = parameters.GetValue<AddFractionPageParameter>(nameof(AddFractionPageParameter));
            _productId = param.ProductId;

            if (param.Fraction == null) return;

            CanRemove.Value = true;
            Title = "Editar Fração";

            _fraction = param.Fraction;
            Fraction.Value = _fraction.Weight;
            Price.Value = _fraction.Price;
        }

        private async void OnRemoveCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            _productObservable.RemoveFraction(_fraction);

            busy.Dispose();

            await _popupPageService.Close();
        }

        private async void OnAddCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            _productObservable.AddFraction(new Fraction
            {
                ProductId = _productId,
                Weight = Fraction.Value.Value,
                Price = Price.Value.Value
            });

            busy.Dispose();

            await _popupPageService.Close();
        }

        private async void OnEditCommand()
        {
            var busy = BusyNotifier.ProcessStart();

            await Task.Delay(500); //Simulating api

            _fraction.Weight = Fraction.Value.Value;
            _fraction.Price = Price.Value.Value;
            _productObservable.UpdateFraction(_fraction);

            busy.Dispose();

            await _popupPageService.Close();
        }
    }

    public class AddFractionPageParameter
    {
        public AddFractionPageParameter(Fraction fraction, Guid productId)
        {
            Fraction = fraction;
            ProductId = productId;
        }

        public Fraction Fraction { get; }
        public Guid ProductId { get; }
    }
}

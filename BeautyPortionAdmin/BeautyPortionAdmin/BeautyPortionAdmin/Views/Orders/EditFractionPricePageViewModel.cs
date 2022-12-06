using System;
using System.Reactive.Linq;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Services;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

namespace BeautyPortionAdmin.Views.Orders
{
    public class EditFractionPricePageViewModel : ViewModelBase, IHasFieldValidators
    {
        private readonly IPopupPageService _popupPageService;
        private EditFractionPageParameter _parameter;

        public EditFractionPricePageViewModel(INavigationService navigationService,
                                         IFieldValidatorsObservable fieldValidatorsObservable,
                                         IPopupPageService popupPageService)
            : base(navigationService)
        {
            Title = "Editar Preço";
            _popupPageService = popupPageService;
            FieldValidatorsObservable = fieldValidatorsObservable;

            Price = new ReactiveProperty<double>().AddTo(Disposables);

            SaveCommand = 
                FieldValidatorsObservable.ObserveFieldHasErrors.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(OnEditCommand, Disposables);
        }

        public IFieldValidatorsObservable FieldValidatorsObservable { get; }

        public ReactiveProperty<double> Price { get; }
        public ReactiveCommand SaveCommand { get; }

        public override void Initialize(INavigationParameters parameters)
        {
            _parameter = parameters.GetValue<EditFractionPageParameter>(nameof(EditFractionPageParameter));
            Price.Value = _parameter.ActualPrice;
        }

        private async void OnEditCommand()
        {
            _parameter.OnPriceChanged(Price.Value);
            await _popupPageService.Close();
        }
    }

    public class EditFractionPageParameter
    {
        public EditFractionPageParameter(double actualPrice, Action<double> onPriceChanged)
        {
            ActualPrice = actualPrice;
            OnPriceChanged = onPriceChanged;
        }

        public double ActualPrice { get; }
        public Action<double> OnPriceChanged { get; }
    }
}

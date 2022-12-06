using System;
using System.Reactive.Linq;
using BeautyPortionAdmin.Extensions;
using BeautyPortionAdmin.Framework;
using BeautyPortionAdmin.Helpers;
using BeautyPortionAdmin.Models;
using BeautyPortionAdmin.Views.Master;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Threading.Tasks;

namespace BeautyPortionAdmin.Views.Orders
{
    public class OrderDetailStepPageViewModel : ViewModelBase, IHasMasterPage, IHasFieldValidators
    {
        private readonly IOrderObservable _orderObservable;

        public OrderDetailStepPageViewModel(INavigationService navigationService,
                                            IMasterPageViewModel masterPageViewModel,
                                            IFieldValidatorsObservable fieldValidatorsObservable,
                                            IOrderObservable orderObservable)
            : base(navigationService)
        {
            Title = "Finalização";

            MasterPageViewModel = masterPageViewModel;
            FieldValidatorsObservable = fieldValidatorsObservable;
            _orderObservable = orderObservable;

            PaymentMethod = new ReactiveProperty<PaymentType>().AddTo(Disposables);
            DeliveryMode = new ReactiveProperty<DeliveryMode>().AddTo(Disposables);
            Address = new ReactiveProperty<string>().AddTo(Disposables);
            Freight = new ReactiveProperty<double?>().AddTo(Disposables);

            orderObservable
                .ObserveAddress
                .Where(x => !string.IsNullOrEmpty(x))
                .Subscribe(x => Address.Value = x)
                .AddTo(Disposables);

            orderObservable
                .ObserveDeliveryMode
                .Where(x => x.HasValue)
                .Subscribe(x => DeliveryMode.Value = x.Value)
                .AddTo(Disposables);

            orderObservable
                .ObservePaymentType
                .Where(x => x.HasValue)
                .Subscribe(x => PaymentMethod.Value = x.Value)
                .AddTo(Disposables);

            orderObservable
                .ObserveFreight
                .Subscribe(x => Freight.Value = x)
                .AddTo(Disposables);

            _orderObservable.ObserveSelectedClient
                .WhereNotNull()
                .Take(1)
                .Subscribe(x => { Address.Value = x.Address; })
                .AddTo(Disposables);

            DeliveryMode
                .Where(x => x == Models.DeliveryMode.Withdrawal)
                .Subscribe(_ => Freight.Value = null)
                .AddTo(Disposables);

            NextCommand = FieldValidatorsObservable
                .ObserveFieldHasErrors
                .CombineLatest(DeliveryMode, (hasErrors, deliveryMode) =>
                               (deliveryMode == Models.DeliveryMode.Delivery && !hasErrors) ||
                               deliveryMode == Models.DeliveryMode.Withdrawal)
                .ToReactiveCommand()
                .WithSubscribe(OnNextCommand, Disposables);
        }

        public IMasterPageViewModel MasterPageViewModel { get; }
        public IFieldValidatorsObservable FieldValidatorsObservable { get; }

        public ReactiveProperty<PaymentType> PaymentMethod { get; }
        public ReactiveProperty<DeliveryMode> DeliveryMode { get; }
        public ReactiveProperty<string> Address { get; }
        public ReactiveProperty<double?> Freight { get; }

        public ReactiveCommand NextCommand { get; }

        void OnNextCommand()
        {
            _orderObservable.SetPaymentMethod(PaymentMethod.Value);
            _orderObservable.SetDeliveryMethod(DeliveryMode.Value);
            _orderObservable.SetAddress(Address.Value);
            _orderObservable.SetFreight(Freight.Value);

            NavigationService.NavigateAsync(Pages.OrderSummaryStepPage)
                .ToObservable()
                .Subscribe()
                .AddTo(Disposables);
        }
    }
}

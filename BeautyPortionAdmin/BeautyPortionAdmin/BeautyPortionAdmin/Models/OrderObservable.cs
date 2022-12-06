using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using BeautyPortionAdmin.Bootstraping;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Models
{
    public interface IOrderObservable : IDisposable
    {
        IObservable<IEnumerable<Order>> ObserveOrders { get; }
        IObservable<Client> ObserveSelectedClient { get; }
        IObservable<IEnumerable<Product>> ObserveSelectedProducts { get; }
        IObservable<IEnumerable<FractionInOrder>> ObserveSelectedFractions { get; }
        IObservable<PaymentType?> ObservePaymentType { get; }
        IObservable<DeliveryMode?> ObserveDeliveryMode { get; }
        IObservable<double?> ObserveFreight { get; }
        IObservable<string> ObserveAddress { get; }

        void CreateOrder();
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void RemoveOrder(Order order);
        void SetClient(Client client);
        void AddProduct(Product product);
        void AddProducts(IEnumerable<Product> products);
        void RemoveProduct(Product product);
        void AddOrUpdateFraction(FractionInOrder fraction);
        void AddOrUpdateFractions(IEnumerable<FractionInOrder> fractions);        
        void RemoveFraction(FractionInOrder fraction);
        void RemoveAllFractions();
        void SetPaymentMethod(PaymentType? paymentType);
        void SetDeliveryMethod(DeliveryMode? deliveryMode);
        void SetFreight(double? freight);
        void SetAddress(string address);
        void ResetValues();
    }

    [Singleton]
    public class OrderObservable : IOrderObservable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BehaviorSubject<Client> _selectedClient;
        private readonly BehaviorSubject<List<Product>> _selectedProducts;
        private readonly BehaviorSubject<List<FractionInOrder>> _selectedFractions;
        private readonly BehaviorSubject<List<Order>> _orders;
        private readonly BehaviorSubject<PaymentType?> _paymentType;
        private readonly BehaviorSubject<DeliveryMode?> _deliveryMode;
        private readonly BehaviorSubject<double?> _freight;
        private readonly BehaviorSubject<string> _address;

        public OrderObservable()
        {
            _selectedClient = new BehaviorSubject<Client>(null).AddTo(_disposables);
            _selectedProducts = new BehaviorSubject<List<Product>>(new List<Product>()).AddTo(_disposables);
            _selectedFractions = new BehaviorSubject<List<FractionInOrder>>(new List<FractionInOrder>()).AddTo(_disposables);
            _orders = new BehaviorSubject<List<Order>>(new List<Order>()).AddTo(_disposables);
            _paymentType = new BehaviorSubject<PaymentType?>(null).AddTo(_disposables);
            _deliveryMode = new BehaviorSubject<DeliveryMode?>(null).AddTo(_disposables);
            _freight = new BehaviorSubject<double?>(null).AddTo(_disposables);
            _address = new BehaviorSubject<string>(null).AddTo(_disposables);
        }

        public IObservable<IEnumerable<Order>> ObserveOrders => _orders;
        public IObservable<Client> ObserveSelectedClient => _selectedClient;
        public IObservable<IEnumerable<Product>> ObserveSelectedProducts => _selectedProducts;
        public IObservable<IEnumerable<FractionInOrder>> ObserveSelectedFractions => _selectedFractions;
        public IObservable<PaymentType?> ObservePaymentType => _paymentType;
        public IObservable<DeliveryMode?> ObserveDeliveryMode => _deliveryMode;
        public IObservable<double?> ObserveFreight => _freight;
        public IObservable<string> ObserveAddress => _address;

        public void CreateOrder()
        {
            var order = new Order
            {
                ClientId = _selectedClient.Value.Id,
                Fractions = _selectedFractions.Value,
                Freight = _freight.Value ?? 0,
                OrderDate = DateTime.Now,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaymentType = _paymentType.Value ?? PaymentType.Pix,
                ProductIds = _selectedProducts.Value.Select(x => x.Id).ToList()
            };

            var orders = _orders.Value;
            orders.Add(order);
            _orders.OnNext(orders);
        }

        public void AddOrder(Order order)
        {
            var orders = _orders.Value;
            if (orders.Any(x => x.Id == order.Id))
                return;

            orders.Add(order);
            _orders.OnNext(orders);
        }

        public void UpdateOrder(Order order)
        {
            var orderIndex = _orders.Value.FindIndex(x => x.Id == order.Id);
            _orders.Value[orderIndex] = order;
            _orders.OnNext(_orders.Value);
        }

        public void RemoveOrder(Order order)
        {
            _orders.Value.Remove(order);
            _orders.OnNext(_orders.Value);
        }

        public void AddOrUpdateFraction(FractionInOrder fraction)
        {            
            if(_selectedFractions.Value.Any(x => x.Id == fraction.Id))
            {
                var index = _selectedFractions.Value.FindIndex(f => f.Id == fraction.Id);
                _selectedFractions.Value[index] = fraction;
            }
            else
            {
                _selectedFractions.Value.Add(fraction);
            }

            _selectedFractions.OnNext(_selectedFractions.Value);
        }

        public void AddOrUpdateFractions(IEnumerable<FractionInOrder> fractions)
        {
            var fractionsAdded = _selectedFractions.Value;
            var newFractionsToAdd = fractions.Where(x => !fractionsAdded.Any(f => f.Id == x.Id));
            var fractionsToUpdate = fractions.Where(x => fractionsAdded.Any(f => f.Id == x.Id));

            foreach (var item in fractionsToUpdate)
            {
                var index = fractionsAdded.FindIndex(f => f.Id == item.Id);
                _selectedFractions.Value[index] = item;
            }

            if (newFractionsToAdd.Any())
                _selectedFractions.Value.AddRange(newFractionsToAdd);
            
            _selectedFractions.OnNext(_selectedFractions.Value);
        }

        public void AddProduct(Product product)
        {
            if (_selectedProducts.Value.Any(x => x.Id == product.Id))
                return;

            _selectedProducts.Value.Add(product);
            _selectedProducts.OnNext(_selectedProducts.Value);
        }

        public void AddProducts(IEnumerable<Product> products)
        {
            var productsAdded = _selectedProducts.Value;
            var productsToAdd = products.Where(x => !productsAdded.Any(p => p.Id == x.Id));

            if (!productsToAdd.Any()) return;
            _selectedProducts.Value.AddRange(productsToAdd);
            _selectedProducts.OnNext(_selectedProducts.Value);
        }

        public void RemoveFraction(FractionInOrder fraction)
        {
            if (!_selectedFractions.Value.Any(x => x.Id == fraction.Id))
                return;

            _selectedFractions.Value.Remove(fraction);
            _selectedFractions.OnNext(_selectedFractions.Value);
        }

        public void RemoveProduct(Product product)
        {
            if (!_selectedProducts.Value.Any(x => x.Id == product.Id))
                return;

            RemoveFractionsByProduct(product);
            _selectedProducts.Value.Remove(product);
            _selectedProducts.OnNext(_selectedProducts.Value);
        }

        public void RemoveAllProducts()
        {
            RemoveAllFractions();
            _selectedProducts.OnNext(new List<Product>());
        }

        public void RemoveAllFractions()
        {
            _selectedFractions.OnNext(new List<FractionInOrder>());
        }

        public void SetClient(Client client)
        {
            _selectedClient.OnNext(client);
        }

        public void SetPaymentMethod(PaymentType? paymentType)
        {
            _paymentType.OnNext(paymentType);
        }

        public void SetDeliveryMethod(DeliveryMode? deliveryMode)
        {
            _deliveryMode.OnNext(deliveryMode);
        }

        public void SetFreight(double? freight)
        {
            _freight.OnNext(freight);
        }

        public void SetAddress(string address)
        {
            _address.OnNext(address);
        }

        public void ResetValues()
        {
            SetClient(null);
            SetAddress(null);
            SetFreight(null);
            SetPaymentMethod(null);
            SetDeliveryMethod(null);
            RemoveAllProducts();
        }

        private void RemoveFractionsByProduct(Product product)
        {
            _selectedFractions.OnNext(_selectedFractions.Value.Where(f => f.ProductId != product.Id).ToList());
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

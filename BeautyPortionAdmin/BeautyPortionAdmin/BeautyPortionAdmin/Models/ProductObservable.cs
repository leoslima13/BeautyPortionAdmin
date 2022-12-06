using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using BeautyPortionAdmin.Bootstraping;
using BeautyPortionAdmin.Framework;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Models
{
    public interface IProductObservable : ICrudObservable<Product>
    {
        void AddFraction(Fraction fraction);
        void RemoveFraction(Fraction fraction);
        void UpdateFraction(Fraction fraction);

        IObservable<IEnumerable<Fraction>> ObserveFractions { get; }
    }

    [Singleton]
    public class ProductObservable : IProductObservable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly BehaviorSubject<List<Product>> _products;
        private readonly BehaviorSubject<List<Fraction>> _fractions;

        public ProductObservable()
        {
            _products = new BehaviorSubject<List<Product>>(new List<Product>(MockProducts())).AddTo(_disposables);
            _fractions = new BehaviorSubject<List<Fraction>>(new List<Fraction>()).AddTo(_disposables);
        }

        public IObservable<IEnumerable<Product>> ObserveItems => _products;

        public IObservable<IEnumerable<Fraction>> ObserveFractions => _fractions;

        private List<Product> MockProducts()
        {
            return new List<Product>
            {
                new Product { Name = "Truss", Quantity = 10, Price = 33.15, Weight = 1000 },
                new Product { Name = "Wella", Quantity = 10, Price = 33.15, Weight = 1000 },
                new Product { Name = "Uniq One", Quantity = 10, Price = 33.15, Weight = 1000 },
                new Product { Name = "L'oreal", Quantity = 10, Price = 33.15, Weight = 1000 },
            };
        }

        public void AddItem(Product item)
        {
            _products.Value.Add(item);
            _products.OnNext(_products.Value);
        }

        public void RemoveItem(Product item)
        {
            _products.Value.Remove(item);
            _fractions.Value.RemoveAll(x => x.ProductId == item.Id);
            _fractions.OnNext(_fractions.Value);
            _products.OnNext(_products.Value);
        }

        public void UpdateItem(Product item)
        {
            var productIndex = _products.Value.FindIndex(x => x.Id == item.Id);
            _products.Value[productIndex] = item;
            _products.OnNext(_products.Value);
        }

        public void AddFraction(Fraction fraction)
        {
            _fractions.Value.Add(fraction);
            _fractions.OnNext(_fractions.Value);
        }

        public void RemoveFraction(Fraction fraction)
        {
            _fractions.Value.Remove(fraction);
            _fractions.OnNext(_fractions.Value);
        }

        public void UpdateFraction(Fraction fraction)
        {
            var fractionIndex = _fractions.Value.FindIndex(x => x.Id == fraction.Id);
            _fractions.Value[fractionIndex] = fraction;
            _fractions.OnNext(_fractions.Value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

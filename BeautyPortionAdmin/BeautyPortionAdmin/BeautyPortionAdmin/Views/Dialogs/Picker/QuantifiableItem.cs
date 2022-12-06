using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BeautyPortionAdmin.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Views.Dialogs.Picker
{
    public interface IQuantifiableItem : IPickerItem
    {
        ReactiveProperty<int> Quantity { get; }
        ReactiveCommand AddQuantityCommand { get; }
        ReactiveCommand RemoveQuantityCommand { get; }
    }

    public class QuantifiableItem : BindableBase, IQuantifiableItem
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public QuantifiableItem(string name, object value, int quantity)
        {
            Name = name;
            Value = value;
            Quantity = new ReactiveProperty<int>(quantity).AddTo(_disposables);

            AddQuantityCommand = new ReactiveCommand()
                .WithSubscribe(() => Quantity.Value = Quantity.Value + 1, _disposables);

            RemoveQuantityCommand = Quantity
                .WhereNotNull()
                .Select(x => x > 0)
                .ToReactiveCommand()
                .WithSubscribe(() => Quantity.Value = Quantity.Value - 1, _disposables);
        }

        public string Name { get; }
        public object Value { get; }

        public ReactiveProperty<int> Quantity { get; }
        public ReactiveCommand AddQuantityCommand { get; }
        public ReactiveCommand RemoveQuantityCommand { get; }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

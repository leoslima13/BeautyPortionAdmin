using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BeautyPortionAdmin.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BeautyPortionAdmin.Views.Dialogs.Picker
{
    public interface IPickerItem : IDisposable, INotifyPropertyChanged
    {
        string Name { get; }
        object Value { get; }
    }

    public interface ISelectableItem : IPickerItem
    {
        ReactiveCommand SelectCommand { get; }
        ReactiveProperty<bool> IsSelected { get; }
    }

    public class SelectableItem : BindableBase, ISelectableItem
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public SelectableItem(string name, object value, bool isSelected)
        {
            Name = name;
            Value = value;
            IsSelected = new ReactiveProperty<bool>(isSelected).AddTo(_disposables);
            SelectCommand = new ReactiveCommand()
                .WithSubscribe(() => IsSelected.Value = !IsSelected.Value, _disposables);
        }

        public string Name { get; }
        public object Value { get; }
        public ReactiveProperty<bool> IsSelected { get; }
        public ReactiveCommand SelectCommand { get; }

        public void Dispose()
        {
            if (Value is IDisposable disposable)
                disposable.Dispose();
            _disposables.Dispose();
        }
    }

    public class SingleSelectManager : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IList<ISelectableItem> _items;

        public SingleSelectManager(IEnumerable<ISelectableItem> items)
        {
            _items = items.ToList();

            foreach (var item in _items)
            {
                item.IsSelected
                    .Where(x => x)
                    .Subscribe(_ => { OnIsSelectedChanged(item); })
                    .AddTo(_disposables);
            }
        }

        void OnIsSelectedChanged(ISelectableItem itemChanged)
        {
            foreach (var item in _items.Where(x => x != itemChanged))
                item.IsSelected.Value = false;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

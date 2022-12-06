using BeautyPortionAdmin.Controls;
using Reactive.Bindings.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BeautyPortionAdmin.Helpers
{
    public class FilteredCollection<T> where T : class, INotifyPropertyChanged
    {
        private readonly EnhancedReactiveCollection<T> _items;
        private readonly Func<T, bool> _filter;

        public FilteredCollection(IEnumerable<T> items, Func<T, bool> filter)
        {
            _items = new EnhancedReactiveCollection<T>(items.ToList());
            _filter = filter;
            Filtered = _items.ToFilteredReadOnlyObservableCollection(filter);
        }

        public IFilteredReadOnlyObservableCollection<T> Filtered { get; private set; }

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public bool Any(Func<T, bool> filter = null)
        {
            if(filter != null)
                return _items.Any(filter);

            return _items.Any();
        }

        public IEnumerable<TResult> Select<TResult>(Func<T, TResult> func)
        {
            return _items.Select(func);
        }

        public IEnumerable<T> Where(Func<T, bool> where)
        {
            return _items.Where(where);
        }

        public IEnumerable<TResult> OfType<TResult>()
        {
            return _items.OfType<TResult>();
        }
    }
}

using System;
using System.Collections.Generic;

namespace BeautyPortionAdmin.Framework
{
    public interface ICrudObservable<T> : IDisposable
    {
        IObservable<IEnumerable<T>> ObserveItems { get; }
        void AddItem(T item);
        void RemoveItem(T item);
        void UpdateItem(T item);
    }
}

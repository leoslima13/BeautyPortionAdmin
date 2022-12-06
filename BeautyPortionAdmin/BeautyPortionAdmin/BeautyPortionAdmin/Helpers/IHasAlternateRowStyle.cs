using System;
using Reactive.Bindings;

namespace BeautyPortionAdmin.Helpers
{
    public interface IHasAlternateRowStyle
    {
        public ReactiveProperty<bool> HasAlternateRowStyle { get; }
    }
}

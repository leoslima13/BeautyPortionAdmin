using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfReactiveCommand
    {
        public static ReactiveCommand WithSubscribe(this ReactiveCommand command, Action onNext, ICollection<IDisposable> disposables)
        {
            return command.WithSubscribe(onNext, x => x.AddTo(disposables));
        }
    }
}

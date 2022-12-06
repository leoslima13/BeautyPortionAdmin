using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfObservable
    {
        public static IObservable<T> ThrottleFirst<T>(this IObservable<T> observable, TimeSpan? delay = null, IScheduler scheduler = null)
        {
            delay ??= TimeSpan.FromMilliseconds(500);

            return observable.Publish(o => o.Take(1)
                .Concat(o.IgnoreElements().TakeUntil(Observable.Return(Unit.Default).Delay(delay.Value, scheduler ?? Scheduler.Default)))
                .Repeat()
                .TakeUntil(o.IgnoreElements().Concat(Observable.Return(default(T)))));
        }

        public static IObservable<T> WhereNotNull<T>(this IObservable<T> observable)
        {
            return observable.Where(x => x != null);
        }
    }
}

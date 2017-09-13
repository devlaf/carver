using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace Carver.Data.TokenStore
{
    // Because reactive async-compatable streams are not fully a thing in C# quite yet.
    public class AsyncEnumerable
    {
        public static IEnumerable<T> ToAsyncIEnumerable<T, Q>(
            Q seed,
            Func<List<T>, bool> endCondition, 
            Func<List<T>, Q> updateSeed, 
            Func<Q, Task<List<T>>> getPageAsync)
        {
            var collected = new BlockingCollection<T>();
            var observable = ToObservableEnumerable<T,Q>(seed, endCondition, updateSeed, getPageAsync);

            using (observable.Subscribe(
                onNext: x => { foreach (var val in x) {collected.Add(val); } },
                onCompleted: collected.CompleteAdding))
            {
                while (!collected.IsCompleted)
                {
                    if(collected.TryTake(out var val))
                    {
                        yield return val;
                    }
                }
            }
        }

         public static IObservable<T> ToObservable<T,Q>(
            Q seed,
            Func<List<T>, bool> endCondition, 
            Func<List<T>, Q> updateSeed, 
            Func<Q, Task<List<T>>> getPageAsync)
        {
            return Observable.Create<T>(observer => Task.Run(async () => {
                List<T> page;
                Q floor = seed;

                do
                {
                    page = await getPageAsync(floor);
                    floor = updateSeed(page);
                    foreach(var val in page)
                    {
                        observer.OnNext(val);
                    }
                }
                while(!endCondition(page));
                observer.OnCompleted();
            }));
        }

        private static IObservable<IEnumerable<T>> ToObservableEnumerable<T,Q>(
            Q seed,
            Func<List<T>, bool> endCondition, 
            Func<List<T>, Q> updateSeed, 
            Func<Q, Task<List<T>>> getPageAsync)
        {
            return Observable.Create<IEnumerable<T>>(observer => Task.Run(async () => {
                List<T> page;
                Q floor = seed;

                do
                {
                    page = await getPageAsync(floor);
                    floor = updateSeed(page);
                    observer.OnNext(page);
                }
                while(!endCondition(page));
                observer.OnCompleted();
            }));
        }
    }
}
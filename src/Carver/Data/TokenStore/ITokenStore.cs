using System;
using System.Threading.Tasks;

namespace Carver.Data.TokenStore
{
    public interface ITokenStore<T>
    {
        Task<string> Create(T data, DateTimeOffset? expiration);

        Task Invalidate(string token);

        Task<bool> Exists(string token);

        Task<T> Lookup(string token);

        IObservable<String> GetExpired();
    }
}
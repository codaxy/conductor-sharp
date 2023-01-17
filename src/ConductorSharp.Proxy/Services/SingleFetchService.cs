using System.Collections.Concurrent;

namespace ConductorSharp.Proxy.Services
{
    public class SingleFetchService : ISingleFetchService
    {
        private readonly ConcurrentDictionary<string, object?> _dict = new();
        private readonly SemaphoreSlim _semaphore = new(1);
        private int _cached { get; set; }
        private int _fetched { get; set; }

        public async Task<T> Fetch<T>(Func<Task<T>> method) where T : class
        {
            var myId = Guid.NewGuid().ToString();
            try
            {
                _dict[myId] = default;

                await _semaphore.WaitAsync();

                if (_dict[myId] == null)
                {
                    var response = await method.Invoke();

                    await Task.Delay(100);

                    foreach (var key in _dict.Keys)
                    {
                        _dict[key] = response;
                    }

                    _fetched++;
                }
                else
                {
                    _cached++;
                }
                return (T)_dict[myId]!;
            }
            finally
            {
                _dict.TryRemove(myId, out var _);
                _semaphore.Release();
            }
        }

        public int GetCachedCount() => _cached;

        public float GetCacheRatio() => _fetched / (float)_cached;

        public int GetPolledCount() => _fetched;
    }
}

namespace ConductorSharp.Proxy.Services
{
    public interface ISingleFetchService
    {
        public Task<T> Fetch<T>(Func<Task<T>> method) where T : class;
        public int GetPolledCount();
        public int GetCachedCount();
        public float GetCacheRatio();
    }
}

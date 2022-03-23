using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Comtrade
{
    public class FileDistributedCache : IDistributedCache
    {
        private readonly IOptions<FileDistributedCacheOptions> options;

        public FileDistributedCache(IOptions<FileDistributedCacheOptions> options)
            => this.options = options;

        public string GetCacheFilePath(string key)
            => Path.Combine(options.Value.CachePath, GetSanitizedFileName(key));

        private static string GetSanitizedFileName(string key) 
            => Path.GetInvalidFileNameChars().Aggregate(key, (k, c) => k.Replace(c, '_'));

        public byte[] Get(string key)
        {
            var filePath = GetCacheFilePath(key);

            if (!File.Exists(filePath))
                return null;

            return File.ReadAllBytes(filePath);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var filePath = GetCacheFilePath(key);
            
            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllBytesAsync(filePath, token);
        }

        public void Refresh(string key) 
            => throw new NotSupportedException();

        public Task RefreshAsync(string key, CancellationToken token = default) 
            => throw new NotSupportedException();

        public void Remove(string key)
            => File.Delete(GetCacheFilePath(key));

        public async Task RemoveAsync(string key, CancellationToken token = default)
            => File.Delete(GetCacheFilePath(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration != null || options.SlidingExpiration != null)
                throw new NotSupportedException();

            Directory.CreateDirectory(this.options.Value.CachePath);
            File.WriteAllBytes(GetCacheFilePath(key), value);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (options.AbsoluteExpiration != null || options.SlidingExpiration != null)
                throw new NotSupportedException();

            Directory.CreateDirectory(this.options.Value.CachePath);
            await File.WriteAllBytesAsync(GetCacheFilePath(key), value, token);
        }
    }
}
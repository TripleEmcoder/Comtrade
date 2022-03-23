namespace Comtrade
{
    public class FileDistributedCacheOptions
    {
        public string CachePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "cache");
    }
}
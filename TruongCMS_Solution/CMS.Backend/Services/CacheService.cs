// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: bo nho dem (muc 7 — Caching)

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace CMS.Backend.Services;

/// <summary>
/// BO NHO DEM cho du lieu "nong" (danh muc, trang chu, flash sale) — muc 7 bao cao.
/// Dung IDistributedCache: mac dinh chay RAM (AddDistributedMemoryCache);
/// khi co Redis chi can dien "Redis:ConnectionString" trong appsettings —
/// KHONG phai sua controller nao (mau Cache-Aside).
/// </summary>
public interface ICacheService
{
    /// <summary>Lay tu cache; chua co thi chay factory roi luu lai (Cache-Aside)</summary>
    Task<T?> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory);

    /// <summary>Xoa cac khoa cache khi admin sua du lieu (danh muc/san pham...)</summary>
    Task RemoveAsync(params string[] keys);
}

public class CacheService : ICacheService
{
    // Cac khoa cache dung chung — controller invalidate dung ten nay
    public const string KeyCategories  = "cache:categories";
    public const string KeyNewest      = "cache:products:newest";
    public const string KeyBestSellers = "cache:products:bestsellers";
    public const string KeyFlashSale   = "cache:flashsale:active";
    public const string KeyBrands      = "cache:brands";

    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
    {
        try
        {
            byte[]? cached = await _cache.GetAsync(key);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<T>(cached); // CACHE HIT — khong cham DB
            }
        }
        catch (Exception ex)
        {
            // Redis sap -> bo qua cache, van phuc vu tu DB (khong lam web chet)
            _logger.LogWarning(ex, "Doc cache {Key} loi — fallback DB", key);
        }

        T value = await factory();

        try
        {
            await _cache.SetAsync(key,
                JsonSerializer.SerializeToUtf8Bytes(value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ghi cache {Key} loi — bo qua", key);
        }

        return value;
    }

    public async Task RemoveAsync(params string[] keys)
    {
        foreach (string key in keys)
        {
            try { await _cache.RemoveAsync(key); }
            catch (Exception ex) { _logger.LogWarning(ex, "Xoa cache {Key} loi", key); }
        }
    }
}

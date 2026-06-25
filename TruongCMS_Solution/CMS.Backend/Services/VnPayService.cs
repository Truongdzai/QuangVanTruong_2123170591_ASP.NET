// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: cong thanh toan VNPAY + chu ky HMAC-SHA512
// (test case 10: huy thanh toan; test case 11: IPN idempotent)

using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CMS.Backend.Services;

/// <summary>
/// Tich hop cong thanh toan THEO CHUAN VNPAY (muc 3 bao cao — Payment Gateway):
///  - Tao URL thanh toan co ky HMAC-SHA512 (chong sua tham so tren duong truyen)
///  - Xac thuc chu ky cua return URL / IPN ngan hang goi ve (chong gia mao)
/// CHE DO DEMO: chua co tai khoan merchant (TmnCode trong) -> chuyen sang trang
/// "ngan hang gia lap" /payment-demo ngay tren Backend; trang nay ky tham so bang
/// CUNG mot HashSecret nen toan bo luong xu ly + kiem tra chu ky van chay y het that.
/// </summary>
public interface IVnPayService
{
    bool IsDemoMode { get; }

    /// <summary>Tao URL chuyen khach sang trang thanh toan (that hoac demo)</summary>
    string CreatePaymentUrl(string txnRef, decimal amount, string orderInfo, string ipAddress);

    /// <summary>Kiem tra chu ky HMAC cua bo tham so cong thanh toan goi ve</summary>
    bool ValidateSignature(IDictionary<string, string> queryParams);

    /// <summary>Ky bo tham so demo (trang ngan hang gia lap dung de tao callback hop le)</summary>
    string Sign(IDictionary<string, string> queryParams);
}

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _http;

    public VnPayService(IConfiguration config, IHttpContextAccessor http)
    {
        _config = config;
        _http = http;
    }

    private string TmnCode    => _config["VnPay:TmnCode"] ?? "";
    private string HashSecret => _config["VnPay:HashSecret"] ?? "TRUONGCMS_DEMO_HASH_SECRET_2026";
    private string GatewayUrl => _config["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    /// <summary>Chua dien TmnCode = chay che do demo offline</summary>
    public bool IsDemoMode => string.IsNullOrWhiteSpace(TmnCode);

    /// <summary>Origin cua Backend (https://localhost:7152) de tu tro ve return URL</summary>
    private string BackendOrigin
    {
        get
        {
            var req = _http.HttpContext?.Request;
            return req == null ? "https://localhost:7152" : $"{req.Scheme}://{req.Host}";
        }
    }

    public string CreatePaymentUrl(string txnRef, decimal amount, string orderInfo, string ipAddress)
    {
        // Return URL: cong thanh toan dua KHACH quay ve day sau khi bam Thanh toan/Huy
        string returnUrl = $"{BackendOrigin}/api/payments/vnpay-return";

        var p = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"]    = "2.1.0",
            ["vnp_Command"]    = "pay",
            ["vnp_TmnCode"]    = IsDemoMode ? "DEMO" : TmnCode,
            // VNPAY quy dinh so tien nhan 100 (khong co phan thap phan)
            ["vnp_Amount"]     = ((long)(amount * 100)).ToString(),
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"]   = "VND",
            ["vnp_IpAddr"]     = ipAddress,
            ["vnp_Locale"]     = "vn",
            ["vnp_OrderInfo"]  = orderInfo,
            ["vnp_OrderType"]  = "other",
            ["vnp_ReturnUrl"]  = returnUrl,
            ["vnp_TxnRef"]     = txnRef,
            ["vnp_ExpireDate"] = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
        };

        string query = BuildQuery(p);
        string hash  = HmacSha512(HashSecret, query);

        // DEMO: chuyen sang trang ngan hang gia lap tren chinh Backend
        string baseUrl = IsDemoMode ? $"{BackendOrigin}/payment-demo" : GatewayUrl;
        return $"{baseUrl}?{query}&vnp_SecureHash={hash}";
    }

    public bool ValidateSignature(IDictionary<string, string> queryParams)
    {
        if (!queryParams.TryGetValue("vnp_SecureHash", out var hashNhanDuoc)) return false;

        // Ky lai TOAN BO tham so vnp_* (tru chinh chu ky) roi so khop
        var p = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in queryParams)
        {
            if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                p[key] = value;
        }

        string hashTinhLai = HmacSha512(HashSecret, BuildQuery(p));
        return hashTinhLai.Equals(hashNhanDuoc, StringComparison.OrdinalIgnoreCase);
    }

    public string Sign(IDictionary<string, string> queryParams)
    {
        var p = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in queryParams)
        {
            if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                p[key] = value;
        }
        return HmacSha512(HashSecret, BuildQuery(p));
    }

    /// <summary>Ghep "key=value&key2=value2" theo thu tu alphabet, URL-encode gia tri (chuan VNPAY)</summary>
    private static string BuildQuery(SortedDictionary<string, string> p)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in p)
        {
            if (string.IsNullOrEmpty(value)) continue;
            if (sb.Length > 0) sb.Append('&');
            sb.Append(WebUtility.UrlEncode(key)).Append('=').Append(WebUtility.UrlEncode(value));
        }
        return sb.ToString();
    }

    private static string HmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash); // chu HOA — VNPAY so sanh khong phan biet hoa thuong
    }
}

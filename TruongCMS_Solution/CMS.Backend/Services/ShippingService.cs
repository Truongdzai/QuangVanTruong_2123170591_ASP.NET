// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: tich hop van chuyen (muc 2 — Logistics)

using CMS.Data.Entities;

namespace CMS.Backend.Services;

/// <summary>Ket qua day don sang hang van chuyen.</summary>
public record KetQuaDayDon(bool ThanhCong, string? TrackingCode, string? Provider, string? Loi);

/// <summary>
/// TICH HOP DON VI VAN CHUYEN (muc 2 bao cao — GHTK/GHN/Viettel Post).
/// Ban demo nay MO PHONG API cua GHTK: sinh ma van don, tinh phi ship;
/// khi co API key that chi can thay class nay bang ban goi HttpClient
/// (interface giu nguyen — controller khong phai sua dong nao).
/// Webhook trang thai giao hang nhan o POST /api/shipping/callback (idempotent).
/// </summary>
public interface IShippingService
{
    /// <summary>Tinh phi van chuyen theo gia tri don + so mon (mien phi cho don lon)</summary>
    decimal TinhPhiShip(decimal tamTinh, int soMon);

    /// <summary>Day don sang hang van chuyen, nhan ma van don</summary>
    Task<KetQuaDayDon> DayDonAsync(Order order);
}

public class MockGhtkShippingService : IShippingService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MockGhtkShippingService> _logger;

    public MockGhtkShippingService(IConfiguration config, ILogger<MockGhtkShippingService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public decimal TinhPhiShip(decimal tamTinh, int soMon)
    {
        decimal nguongMienPhi = decimal.TryParse(_config["Shipping:FreeThreshold"], out var n) ? n : 500;
        decimal phiCoBan      = decimal.TryParse(_config["Shipping:BaseFee"], out var b) ? b : 20;
        decimal phiMoiMon     = decimal.TryParse(_config["Shipping:FeePerItem"], out var f) ? f : 2;

        if (tamTinh >= nguongMienPhi) return 0; // don lon mien phi ship
        return phiCoBan + phiMoiMon * Math.Max(0, soMon - 1);
    }

    public Task<KetQuaDayDon> DayDonAsync(Order order)
    {
        // MO PHONG goi POST https://services.giaohangtietkiem.vn/services/shipment/order
        // That ra hang van chuyen tra ve label code — o day tu sinh theo dinh dang GHTK
        string trackingCode = $"GHTK{DateTime.Now:yyMMdd}{order.Id:D6}";

        _logger.LogInformation(
            "[SHIPPING MOCK] Day don #{OrderId} sang GHTK thanh cong — ma van don {Tracking}",
            order.Id, trackingCode);

        return Task.FromResult(new KetQuaDayDon(true, trackingCode, "GHTK", null));
    }
}

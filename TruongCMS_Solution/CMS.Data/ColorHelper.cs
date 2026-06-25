// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 11 (Bien the mau than thien — nhap ten tieng Viet, he thong tu doi ma hex)
// Ngay thuc hien: 12/06/2026
// Version: 2.0

using System.Globalization;
using System.Text;

namespace CMS.Data;

/// <summary>
/// Buoi 11 — Doi TEN MAU TIENG VIET sang ma hex chuan cua shop.
/// Admin chi can go "trắng, đen, đỏ" — khong can biet ma hex la gi.
/// Van chap nhan nhap thang ma hex (#FFFFFF) hoac ten tieng Anh (white).
/// </summary>
public static class ColorHelper
{
    // Bang mau khop voi 10 o tron o sidebar bo loc FrontEnd
    private static readonly Dictionary<string, string> TenMau = new()
    {
        // Tieng Viet (da bo dau, viet thuong)
        ["trang"]        = "#FFFFFF",
        ["den"]          = "#000000",
        ["do"]           = "#F50606",
        ["vang"]         = "#F5DD06",
        ["cam"]          = "#F57906",
        ["xanh"]         = "#063AF5",
        ["xanh duong"]   = "#063AF5",
        ["xanh da troi"] = "#06CAF5",
        ["xanh ngoc"]    = "#06CAF5",
        ["xanh la"]      = "#00C12B",
        ["xanh la cay"]  = "#00C12B",
        ["tim"]          = "#7D06F5",
        ["hong"]         = "#F506A4",
        // Tieng Anh thong dung
        ["white"]  = "#FFFFFF",
        ["black"]  = "#000000",
        ["red"]    = "#F50606",
        ["yellow"] = "#F5DD06",
        ["orange"] = "#F57906",
        ["blue"]   = "#063AF5",
        ["cyan"]   = "#06CAF5",
        ["green"]  = "#00C12B",
        ["purple"] = "#7D06F5",
        ["pink"]   = "#F506A4",
    };

    /// <summary>
    /// Chuan hoa chuoi mau admin nhap: "Trắng, đỏ, #063AF5" -> "#FFFFFF,#F50606,#063AF5".
    /// Gia tri khong nhan dien duoc se bi bo qua (tranh luu rac vao database).
    /// </summary>
    public static string NormalizeColors(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var ketQua = new List<string>();
        foreach (var phan in input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string? hex = ToHex(phan);
            if (hex != null && !ketQua.Contains(hex))
                ketQua.Add(hex);
        }
        return string.Join(",", ketQua);
    }

    /// <summary>Đổi 1 giá trị đơn lẻ sang hex; null nếu không nhận diện được.</summary>
    public static string? ToHex(string value)
    {
        string v = value.Trim();

        // Da la ma hex (#FFF hoac #FFFFFF) -> giu nguyen, viet hoa cho dong nhat
        if (v.StartsWith('#'))
        {
            string body = v[1..];
            bool hopLe = (body.Length == 3 || body.Length == 6)
                         && body.All(Uri.IsHexDigit);
            if (!hopLe) return null;
            if (body.Length == 3) // #FFF -> #FFFFFF
                body = string.Concat(body.Select(c => $"{c}{c}"));
            return "#" + body.ToUpperInvariant();
        }

        // Ten mau -> tra bang (bo dau tieng Viet + viet thuong truoc khi tra)
        return TenMau.TryGetValue(BoDau(v).ToLowerInvariant(), out var hex) ? hex : null;
    }

    // Tra nguoc ma hex -> ten tieng Viet hien thi (phuc vu BIEN THE SKU)
    private static readonly Dictionary<string, string> TenHienThi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["#FFFFFF"] = "Trắng",
        ["#000000"] = "Đen",
        ["#F50606"] = "Đỏ",
        ["#F5DD06"] = "Vàng",
        ["#F57906"] = "Cam",
        ["#063AF5"] = "Xanh dương",
        ["#06CAF5"] = "Xanh da trời",
        ["#00C12B"] = "Xanh lá",
        ["#7D06F5"] = "Tím",
        ["#F506A4"] = "Hồng",
    };

    /// <summary>Đổi mã hex sang tên màu tiếng Việt; không có trong bảng thì trả lại chính mã hex.</summary>
    public static string TenTiengViet(string hex)
        => TenHienThi.TryGetValue(hex.Trim(), out var ten) ? ten : hex;

    /// <summary>Bỏ dấu tiếng Việt: "Trắng" -> "Trang", "đỏ" -> "do".</summary>
    private static string BoDau(string text)
    {
        string formD = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (char c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        // Rieng chu d/D gach ngang khong nam trong NonSpacingMark
        return sb.ToString().Normalize(NormalizationForm.FormC)
                 .Replace('đ', 'd').Replace('Đ', 'D');
    }
}

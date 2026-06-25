// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 9 (Bao mat mat khau - Tieu chi 33)
// Ngay thuc hien: 11/06/2026
// Version: 1.9

using System.Security.Cryptography;
using System.Text;

namespace CMS.Data;

/// <summary>
/// Ma hoa mat khau MOT CHIEU bang SHA256 + chuoi Salt ngau nhien (Tieu chi 33).
/// Dinh dang luu tru: "{saltBase64}:{hashBase64}" — khong the giai nguoc ra mat khau goc.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16; // 128-bit salt ngau nhien cho moi mat khau

    /// <summary>Bam mat khau tho thanh chuoi "salt:hash" de luu vao SQL Server.</summary>
    public static string Hash(string password)
    {
        // Buoc 1: Sinh salt ngau nhien — 2 nguoi cung mat khau van ra hash khac nhau
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Buoc 2: Bam SHA256 tren (salt + password)
        byte[] hash = ComputeHash(salt, password);

        // Buoc 3: Ghep salt + hash de sau nay con doi chieu duoc
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Kiem tra mat khau nguoi dung nhap co khop voi chuoi da luu hay khong.
    /// Ho tro ca du lieu cu (mat khau tho chua bam) de demo khong bi khoa tai khoan.
    /// </summary>
    public static bool Verify(string password, string stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;

        // Du lieu cu (chua co dau ":") -> so sanh truc tiep, sau do nen nang cap hash
        if (!IsHashed(stored)) return stored == password;

        var parts = stored.Split(':', 2);
        try
        {
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expected = Convert.FromBase64String(parts[1]);
            byte[] actual = ComputeHash(salt, password);

            // So sanh thoi gian co dinh, chong timing attack
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            // Chuoi luu tru khong phai Base64 hop le -> coi nhu mat khau tho cu
            return stored == password;
        }
    }

    /// <summary>Nhan dien chuoi da duoc bam theo dinh dang "salt:hash" hay chua.</summary>
    public static bool IsHashed(string stored) =>
        !string.IsNullOrEmpty(stored) && stored.Contains(':');

    private static byte[] ComputeHash(byte[] salt, string password)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] input = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, input, salt.Length, passwordBytes.Length);
        return SHA256.HashData(input);
    }
}

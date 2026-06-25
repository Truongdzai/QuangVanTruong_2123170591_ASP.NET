// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: JWT + Refresh Token (test case 15)

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CMS.Backend.Services;

/// <summary>
/// Phat hanh ACCESS TOKEN (JWT, song ngan ~15 phut) va REFRESH TOKEN
/// (chuoi ngau nhien luu DB, song dai ~7 ngay) cho khach hang.
/// Test case 15: access token het han -> Frontend goi /refresh-token de
/// lay cap token moi NGAM, khach khong phai dang nhap lai.
/// </summary>
public interface ITokenService
{
    string CreateAccessToken(Customer customer);
    Task<RefreshToken> CreateRefreshTokenAsync(int customerId);
    /// <summary>Xoay vong: thu hoi token cu, phat token moi. Tra ve null neu token khong hop le.</summary>
    Task<(string accessToken, RefreshToken refreshToken)?> RotateAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
}

public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public TokenService(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    /// <summary>Khoa ky JWT doc tu appsettings (Jwt:Key) — toi thieu 32 ky tu cho HS256</summary>
    public static SymmetricSecurityKey GetSigningKey(IConfiguration config)
    {
        string key = config["Jwt:Key"] ?? "TruongCMS-Demo-Secret-Key-DoiTrongAppsettings-2026!";
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public string CreateAccessToken(Customer customer)
    {
        int phut = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

        var claims = new List<Claim>
        {
            // sub = Id khach hang — controller doc lai bang ClaimTypes.NameIdentifier
            new(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, customer.Email),
            new(JwtRegisteredClaimNames.Name, customer.FullName),
            new(ClaimTypes.Role, "Customer"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"]   ?? "TruongCMS",
            audience: _config["Jwt:Audience"] ?? "TruongCMS.Frontend",
            claims:   claims,
            notBefore: DateTime.UtcNow,
            expires:  DateTime.UtcNow.AddMinutes(phut),
            signingCredentials: new SigningCredentials(GetSigningKey(_config), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(int customerId)
    {
        int ngay = int.TryParse(_config["Jwt:RefreshTokenDays"], out var d) ? d : 7;

        var token = new RefreshToken
        {
            CustomerId = customerId,
            // 64 byte ngau nhien -> Base64 (88 ky tu): khong the doan duoc
            Token      = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiryDate = DateTime.Now.AddDays(ngay),
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<(string accessToken, RefreshToken refreshToken)?> RotateAsync(string refreshToken)
    {
        var tokenCu = _context.RefreshTokens
            .FirstOrDefault(t => t.Token == refreshToken);

        // Token khong ton tai / da thu hoi / het han -> tu choi, bat dang nhap lai
        if (tokenCu == null || !tokenCu.IsUsable) return null;

        var customer = await _context.Customers.FindAsync(tokenCu.CustomerId);
        if (customer == null) return null;

        // XOAY VONG: thu hoi token cu ngay khi dung (chong replay khi bi danh cap)
        tokenCu.RevokedDate = DateTime.Now;
        await _context.SaveChangesAsync();

        var tokenMoi = await CreateRefreshTokenAsync(customer.Id);
        return (CreateAccessToken(customer), tokenMoi);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var token = _context.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token != null && token.RevokedDate == null)
        {
            token.RevokedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}

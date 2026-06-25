// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: bao ve dashboard Hangfire (muc 7)

using Hangfire.Dashboard;

namespace CMS.Backend.Services;

/// <summary>
/// Chi cho ADMIN (dang nhap cookie o trang quan tri) xem dashboard /hangfire.
/// Nguoi la / nhan vien thuong truy cap -> 401.
/// </summary>
public class HangfireAdminFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
               && httpContext.User.IsInRole("Admin");
    }
}

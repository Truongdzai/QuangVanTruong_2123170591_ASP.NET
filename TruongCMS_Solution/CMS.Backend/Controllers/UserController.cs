// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 4
// Ngay thuc hien: 23/03/2026
// Version: 1.4

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // HELPER: do danh sach quyen han vao ViewBag cho dropdown chon Role
    private void LoadRoleList(string? selected = null)
    {
        var roles = new List<string> { "Admin", "Editor", "Moderator", "User" };
        ViewBag.RoleList = new SelectList(roles, selected);
    }

    // INDEX - danh sach thanh vien, sap xep theo Role -> FullName
    // GET /User
    public async Task<IActionResult> Index()
    {
        // OrderBy Role truoc de nhom theo quyen han, sau do theo ten
        var users = await _context.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .ToListAsync();

        return View(users);
    }

    // CREATE - them thanh vien moi

    // GET: hien form trong de nhap lieu
    [HttpGet]
    public IActionResult Create()
    {
        LoadRoleList("User"); // Mac dinh chon "User" khi mo form
        return View();
    }

    // POST: nhan du lieu tu form, kiem tra username chua bi trung, ghi vao SQL
    [HttpPost]
    public async Task<IActionResult> Create(User model)
    {
        // Kiem tra ten dang nhap da ton tai chua (khong phan biet hoa thuong)
        bool trungTen = await _context.Users
            .AnyAsync(u => u.Username == model.Username);

        if (trungTen)
        {
            // Them loi vao ModelState -> View hien thong bao do cho nguoi dung biet
            ModelState.AddModelError("Username", "Ten dang nhap nay da ton tai, vui long chon ten khac.");
            LoadRoleList(model.Role);
            return View(model);
        }

        _context.Users.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // EDIT - sua thong tin thanh vien da co

    // GET: tim thanh vien cu, do du lieu len form
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        LoadRoleList(user.Role); // Giu dung Role dang chon trong dropdown
        return View(user);
    }

    // POST: nhan du lieu da sua, cap nhat SQL
    [HttpPost]
    public async Task<IActionResult> Edit(User model)
    {
        // Kiem tra username trung voi NGUOI KHAC (khong tinh chinh ban than)
        bool trungTen = await _context.Users
            .AnyAsync(u => u.Username == model.Username && u.Id != model.Id);

        if (trungTen)
        {
            ModelState.AddModelError("Username", "Ten dang nhap nay da ton tai, vui long chon ten khac.");
            LoadRoleList(model.Role);
            return View(model);
        }

        _context.Users.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // DELETE - xoa thanh vien
    // GET /User/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}

// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 9 (CKEditor + upload anh vao noi dung + phan trang admin)
// Ngay thuc hien: 11/06/2026
// Version: 1.9

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize] // Buoi 5: Bat buoc phai dang nhap moi duoc vao
public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env; // de lay duong dan tuyet doi cua wwwroot

    public PostController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // HELPER: xu ly upload file anh, tra ve duong dan tuong doi
    private async Task<string?> UploadImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        // Thu muc luu anh: wwwroot/uploads
        string folder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Ten file ngau nhien de tranh trung voi file cua nguoi khac
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(folder, fileName);

        // Chep du lieu tu trinh duyet xuong server
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Tra ve duong dan tuong doi dung trong the img src
        return "/uploads/" + fileName;
    }

    // HELPER: do danh sach danh muc vao ViewBag de dung trong dropdown
    private void LoadCategoryList(int? selectedId = null)
    {
        ViewBag.CategoryList = new SelectList(
            _context.Categories.OrderBy(c => c.Name).ToList(),
            "Id", "Name", selectedId);
    }

    // INDEX - danh sach bai viet, co loc theo danh muc + PHAN TRANG (Tieu chi 14)
    // GET /Post                 -> trang 1
    // GET /Post?page=2          -> trang 2
    // GET /Post/Index/5?page=1  -> loc bai thuoc CategoryId = 5, trang 1
    public async Task<IActionResult> Index(int? id, int page = 1, string? search = null)
    {
        const int pageSize = 5; // 5 bai moi trang de bang gon gang

        // .Include(p -> p.Category): lay kem ten danh muc, tranh null khi hien len view
        var query = _context.Posts.Include(p => p.Category).AsQueryable();

        if (id != null)
        {
            // .Where: chi lay bai co CategoryId bang voi id truyen vao URL
            query = query.Where(p => p.CategoryId == id);
        }

        // TIM KIEM theo tieu de / noi dung
        if (!string.IsNullOrWhiteSpace(search))
        {
            string kw = search.Trim();
            query = query.Where(p => p.Title.Contains(kw) ||
                                     (p.Content != null && p.Content.Contains(kw)));
        }

        int tongBai = await query.CountAsync();
        int tongTrang = Math.Max(1, (int)Math.Ceiling(tongBai / (double)pageSize));
        page = Math.Clamp(page, 1, tongTrang);

        var posts = await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Du lieu phan trang cho View ve thanh chuyen trang
        ViewBag.Page = page;
        ViewBag.TotalPages = tongTrang;
        ViewBag.CategoryId = id;
        ViewBag.Search = search;
        ViewBag.TotalItems = tongBai;

        return View(posts);
    }

    // DETAILS - xem chi tiet 1 bai viet
    // GET /Post/Details/5
    public async Task<IActionResult> Details(int id)
    {
        // .Include lay kem danh muc de hien ten tren trang chi tiet
        var post = await _context.Posts
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();
        return View(post);
    }

    // CREATE - them bai viet moi

    // GET: hien form trong de nhap lieu, do dropdown danh muc
    [HttpGet]
    public IActionResult Create()
    {
        LoadCategoryList();
        // Truyen model voi CreatedDate = hom nay de input date hien dung gia tri mac dinh
        return View(new Post { CreatedDate = DateTime.Now });
    }

    // POST: nhan du lieu tu form, xu ly upload anh, ghi vao SQL
    [HttpPost]
    public async Task<IActionResult> Create(Post model, IFormFile? uploadImage)
    {
        // ImageUrl do Controller tu gan sau khi upload, Category do EF nap -> bo khoi kiem tra
        ModelState.Remove(nameof(Post.ImageUrl));
        ModelState.Remove(nameof(Post.Category));

        // BAT LOI DU LIEU THIEU: Title/Content rong se bi binder doi thanh NULL.
        // Khong chan o day -> NULL vao cot NOT NULL -> SaveChanges nem DbUpdateException (loi 500).
        // Tra ve form kem thong bao loi de nguoi dung sua, thay vi de trang crash.
        if (!ModelState.IsValid)
        {
            LoadCategoryList(model.CategoryId);
            return View(model);
        }

        // Neu co chon file anh thi upload, nguoc lai giu null -> hien anh mac dinh
        model.ImageUrl = await UploadImageAsync(uploadImage);
        model.CreatedDate = DateTime.Now;

        // Buoc 1: them bai viet vao bo nho tam EF
        _context.Posts.Add(model);
        // Buoc 2: ghi xuong SQL Server
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // EDIT - sua bai viet da co

    // GET: tim bai viet cu, do du lieu len form de nguoi dung sua
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return NotFound();

        // Giu danh muc dang chon hien len dung vi tri trong dropdown
        LoadCategoryList(post.CategoryId);
        return View(post);
    }

    // POST: nhan du lieu da sua, cap nhat SQL
    [HttpPost]
    public async Task<IActionResult> Edit(Post model, IFormFile? uploadImage)
    {
        // ImageUrl xu ly rieng ben duoi, Category do EF nap -> bo khoi kiem tra hop le
        ModelState.Remove(nameof(Post.ImageUrl));
        ModelState.Remove(nameof(Post.Category));

        // Chan Title/Content/CategoryId thieu -> tra ve form bao loi (khong de NULL crash SQL)
        if (!ModelState.IsValid)
        {
            LoadCategoryList(model.CategoryId);
            return View(model);
        }

        if (uploadImage != null && uploadImage.Length > 0)
        {
            // Co file anh moi -> upload len server, cap nhat ImageUrl
            model.ImageUrl = await UploadImageAsync(uploadImage);
        }
        else
        {
            // Khong upload anh moi -> giu nguyen anh cu trong DB
            // Dung AsNoTracking de doc ma khong "chiem" doi tuong, tranh xung dot voi Update
            var oldPost = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (oldPost != null && string.IsNullOrEmpty(model.ImageUrl))
                model.ImageUrl = oldPost.ImageUrl;
        }

        _context.Posts.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // TIEU CHI 35 - UPLOAD ANH TU CKEDITOR
    // CKEditor 5 (CKFinder adapter) POST file vao day voi field name = "upload".
    // Tra ve JSON { uploaded, url } de CKEditor tu chen the <img> vao giua noi dung.
    // POST /Post/UploadImage
    [HttpPost]
    [IgnoreAntiforgeryToken] // CKEditor goi AJAX truc tiep, khong co token cua form Razor
    public async Task<IActionResult> UploadImage(IFormFile upload)
    {
        if (upload == null || upload.Length == 0)
        {
            return Json(new { uploaded = 0, error = new { message = "Chưa chọn file ảnh." } });
        }

        // Chi nhan dung dinh dang anh, chan file la (exe, script...)
        var choPhep = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        string duoiFile = Path.GetExtension(upload.FileName).ToLowerInvariant();
        if (!choPhep.Contains(duoiFile))
        {
            return Json(new { uploaded = 0, error = new { message = "Chỉ chấp nhận file ảnh (jpg, png, gif, webp, svg)." } });
        }

        string? url = await UploadImageAsync(upload);

        return Json(new
        {
            uploaded = 1,
            fileName = Path.GetFileName(url ?? ""),
            url
        });
    }

    // DELETE - xoa bai viet
    // GET /Post/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            // Xoa file anh local (neu la anh upload cua chinh minh, khong xoa URL ngoai)
            if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl.StartsWith("/uploads/"))
            {
                var imgPath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}

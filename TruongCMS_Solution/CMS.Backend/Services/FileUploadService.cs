// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Dich vu upload anh dung chung (san pham, bien the) — luu wwwroot/uploads

namespace CMS.Backend.Services;

/// <summary>
/// Upload file anh vao wwwroot/uploads, tra ve duong dan tuong doi "/uploads/xxx.jpg".
/// Chi nhan dinh dang anh hop le + gioi han dung luong (chong upload file la/qua to).
/// </summary>
public interface IFileUploadService
{
    Task<string?> UploadAsync(IFormFile? file);
    Task<List<string>> UploadManyAsync(IEnumerable<IFormFile>? files);
}

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    private static readonly HashSet<string> AllowedExt =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB / anh

    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string?> UploadAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        string ext = Path.GetExtension(file.FileName);
        if (!AllowedExt.Contains(ext))
        {
            _logger.LogWarning("Tu choi upload file khong phai anh: {Name}", file.FileName);
            return null;
        }
        if (file.Length > MaxBytes)
        {
            _logger.LogWarning("Tu choi upload anh qua lon: {Name} ({Size} bytes)", file.FileName, file.Length);
            return null;
        }

        string folder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);

        string fileName = Guid.NewGuid().ToString("N") + ext;
        string filePath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return "/uploads/" + fileName;
    }

    public async Task<List<string>> UploadManyAsync(IEnumerable<IFormFile>? files)
    {
        var urls = new List<string>();
        if (files == null) return urls;
        foreach (var file in files)
        {
            var url = await UploadAsync(file);
            if (url != null) urls.Add(url);
        }
        return urls;
    }
}

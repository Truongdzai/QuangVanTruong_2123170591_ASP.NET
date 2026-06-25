using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// GIAO DỊCH THANH TOÁN (bảng PaymentTransactions) — mục "3. Payment Gateway".
/// Mỗi lần khách bấm thanh toán online sinh 1 TxnRef duy nhất; webhook IPN
/// từ cổng thanh toán đối chiếu TxnRef để cập nhật KẾT QUẢ MỘT LẦN DUY NHẤT
/// (idempotent — test case 11: ngân hàng bắn IPN trùng cũng không xử lý 2 lần).
/// </summary>
public class PaymentTransaction
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    /// <summary>Cổng thanh toán: VNPAY / MOMO / COD</summary>
    [Required, StringLength(20)]
    public string Provider { get; set; } = "VNPAY";

    /// <summary>Mã tham chiếu DUY NHẤT hệ thống tự sinh, gửi sang cổng (vnp_TxnRef)</summary>
    [Required, StringLength(64)]
    public string TxnRef { get; set; } = string.Empty;

    /// <summary>Mã giao dịch phía ngân hàng trả về (vnp_TransactionNo)</summary>
    [StringLength(64)]
    public string? GatewayTransactionNo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>0 = Đang chờ, 1 = Thành công, 2 = Thất bại/Hủy, 3 = Đã hoàn tiền</summary>
    public int Status { get; set; }

    /// <summary>Mã phản hồi từ cổng (vnp_ResponseCode: "00" = thành công)</summary>
    [StringLength(10)]
    public string? ResponseCode { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>Thời điểm IPN xử lý xong (null = chưa nhận được IPN)</summary>
    public DateTime? ProcessedDate { get; set; }

    /// <summary>Toàn bộ query string IPN gửi về — lưu để đối soát/debug</summary>
    public string? RawData { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }
}

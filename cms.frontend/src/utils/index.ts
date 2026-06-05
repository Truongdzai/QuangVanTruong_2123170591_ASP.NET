// =============================================================
// Shared utility helpers
// =============================================================

/**
 * Định dạng số thành tiền tệ Việt Nam Đồng (VND, không phần lẻ).
 * @example formatPrice(450000) -> "450.000 ₫"
 */
export function formatPrice(n: number): string {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(n);
}

/**
 * Định dạng chuỗi ngày ISO thành ngày/tháng/năm để dễ đọc.
 * @example formatDate('2026-05-17T17:26:17') -> "17/05/2026"
 */
export function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}

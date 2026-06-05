//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 7 – HẠ TẦNG CHUNG CHO CÁC SERVICE
// Gom những helper được các *Service tái sử dụng để tránh lặp code:
//   apiGet / apiPost – gọi HTTP qua axiosClient (Buổi 7 – Phần 2)
//   USE_MOCK / delay – bật/tắt mock data và giả lập độ trễ mạng
//   resolveImageUrl   – chuẩn hoá URL ảnh trả về từ Backend
// =============================================================

import axiosClient from '../api/axiosClient';
import { ENV } from '../configs/env';

// Switch between mock and real backend:
//   .env   ->VITE_USE_MOCK=false   hits ASP.NET at :7152
//   .env  ->  VITE_USE_MOCK=true    uses local mock data (default)
export const USE_MOCK = ENV.USE_MOCK;

// Interceptor trong axiosClient đã bóc sẵn response.data, nên giá trị
// resolve chính là kiểu T (không còn là AxiosResponse<T>).
export const apiGet  = <T>(url: string): Promise<T> => axiosClient.get<T, T>(url);
export const apiPost = <T>(url: string, body: unknown): Promise<T> => axiosClient.post<T, T>(url, body);

/** Giả lập độ trễ mạng khi chạy bằng mock data */
export const delay = (ms = 300): Promise<void> => new Promise((r) => setTimeout(r, ms));

// Origin của Backend (vd https://localhost:7152) — suy ra từ API_BASE_URL
const API_ORIGIN = new URL(ENV.API_BASE_URL).origin;

/**
 * Backend lưu ảnh dạng đường dẫn tương đối (vd "/uploads/abc.jpg").
 * Hàm này ghép thành URL tuyệt đối trỏ về Backend để <img> tải đúng,
 * còn URL tuyệt đối (http/https/data:)
 */
export function resolveImageUrl(path?: string | null): string {
  if (!path) return '';
  if (/^(https?:)?\/\//i.test(path) || path.startsWith('data:')) return path;
  return `${API_ORIGIN}/${path.replace(/^\/+/, '')}`;
}

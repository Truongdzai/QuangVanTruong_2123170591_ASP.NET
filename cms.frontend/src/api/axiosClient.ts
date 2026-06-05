//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 7 – PHẦN 2: HTTP CLIENT (AXIOS) TẬP TRUNG
// Cấu hình một "trục cơ sở" axios duy nhất để mọi service tái sử dụng,
// tránh việc lặp lại đường dẫn https://localhost:7152/api/... ở khắp nơi.
// =============================================================

import axios from 'axios';
import { ENV } from '../configs/env';

// Khởi tạo một thực thể axios với cấu hình base chung
const axiosClient = axios.create({
  baseURL: ENV.API_BASE_URL,          // Cổng Backend ASP.NET Core (đổi trong .env)
  headers: { 'Content-Type': 'application/json' },
  timeout: 10000,                     // Tối đa chờ phản hồi server 10 giây
});

// Interceptor: can thiệp vào phản hồi TRƯỚC khi trả về cho component.
// Bóc tách thẳng `response.data` để service nhận luôn JSON, và xử lý lỗi tập trung.
axiosClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    console.error('Lỗi kết nối API:', error.message);
    return Promise.reject(error);
  },
);

export default axiosClient;

//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 7 – SERVICE LIÊN HỆ & ĐĂNG KÝ NHẬN TIN
// Gom các form gửi dữ liệu lên Backend: đăng ký nhận tin (newsletter)
// và form liên hệ. Route Backend: /api/newsletter , /api/contact
// =============================================================

import { USE_MOCK, apiPost, delay } from './http';

export async function submitEmail(email: string): Promise<{ success: boolean }> {
  if (USE_MOCK) {
    await delay(500);
    console.log('[Mock] Email submitted:', email);
    return { success: true };
  }
  return apiPost<{ success: boolean }>('/newsletter', { email });
}

export async function submitContact(
  name: string, email: string, message: string,
): Promise<{ success: boolean }> {
  if (USE_MOCK) {
    await delay(600);
    console.log('[Mock] Contact submitted:', { name, email, message });
    return { success: true };
  }
  return apiPost<{ success: boolean }>('/contact', { name, email, message });
}

const contactService = {
  submitEmail,
  submitContact,
};

export default contactService;

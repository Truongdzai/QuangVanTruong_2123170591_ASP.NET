//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 – Cấu hình API_BASE_URL trỏ tới ASP.NET Core Web API
// =============================================================
// Environment & site-wide configuration
// =============================================================
// Vite exposes VITE_* vars via import.meta.env at build time.
// Defaults fall back to reasonable dev values.
// See .env.example for the full list of supported variables.
// =============================================================

export const ENV = {
  /** Toggle false when the ASP.NET backend is ready */
  USE_MOCK: (import.meta.env.VITE_USE_MOCK as string | undefined) !== 'false',

  /** Base URL of the ASP.NET Web API */
  API_BASE_URL: (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'https://localhost:7152/api',

  /** Public-facing website origin (no trailing slash) */
  SITE_URL: (import.meta.env.VITE_SITE_URL as string | undefined) ?? 'https://shop.co',

  /** Brand name used in titles, schema.org, and social cards */
  SITE_NAME: (import.meta.env.VITE_SITE_NAME as string | undefined) ?? 'SHOP.CO',
} as const;

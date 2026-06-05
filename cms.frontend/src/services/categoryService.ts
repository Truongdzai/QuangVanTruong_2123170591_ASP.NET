//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 7 – SERVICE DANH MỤC SẢN PHẨM (Categories Products)
// Quản lý hàm gọi API danh mục sản phẩm.
// Route Backend: /api/categoriesproducts
// =============================================================

import type { Category } from '../types';
import { USE_MOCK, apiGet, delay } from './http';
import { mockCategories } from './mock';

/**  projection GET /api/categoriesproducts */
interface BackendCategory {
  id: number;
  name: string;
  description?: string;
}

function mapCategory(bc: BackendCategory): Category {
  return {
    id:          bc.id,
    name:        bc.name,
    slug:        bc.name.toLowerCase().replace(/\s+/g, '-'),
    description: bc.description ?? '',
    icon:        '',
  };
}

export async function getCategories(): Promise<Category[]> {
  if (USE_MOCK) { await delay(); return mockCategories; }
  const raw = await apiGet<BackendCategory[]>('/categoriesproducts');
  return raw.map(mapCategory);
}

const categoryService = {
  getCategories,
};

export default categoryService;

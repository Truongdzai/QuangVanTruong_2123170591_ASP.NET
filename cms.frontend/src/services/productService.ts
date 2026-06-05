//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 7 – SERVICE SẢN PHẨM (Products)
// Quản lý các hàm gọi API liên quan tới sản phẩm.
// Route Backend: /api/products , /api/products/{id} ,
//                /api/products/categoryproduct/{categoryId}
// =============================================================

import type { Product } from '../types';
import { USE_MOCK, apiGet, delay, resolveImageUrl } from './http';
import { mockProducts } from './mock';
import { PLACEHOLDER_IMAGE } from '../configs/constants';

/** Khớp với entity Product trong CMS.Data (ASP.NET JSON — camelCase) */
interface BackendProduct {
  id: number;
  name: string;
  description?: string | null;
  price: number;               // decimal -> number
  stockQuantity: number;
  imageUrl?: string | null;
  categoryProductId: number;
}

function mapProduct(bp: BackendProduct): Product {
  return {
    id:               bp.id,
    slug:             String(bp.id),          // backend has no slug field
    name:             bp.name,
    description:      bp.description ?? '',
    price:            bp.price,
    originalPrice:    null,
    discount:         undefined,
    rating:           4.5,
    reviewCount:      undefined,
    stockQuantity:    bp.stockQuantity,
    imageUrl:         resolveImageUrl(bp.imageUrl) || PLACEHOLDER_IMAGE,
    categoryProductId: bp.categoryProductId,
    tags:             [],
    badge:            null,
    colors:           [],
    sizes:            ['Small', 'Medium', 'Large', 'X-Large'],
  };
}

export async function getProducts(): Promise<Product[]> {
  if (USE_MOCK) { await delay(); return mockProducts; }
  const raw = await apiGet<BackendProduct[]>('/products');
  return raw.map(mapProduct);
}

export async function getProductById(id: number): Promise<Product | null> {
  if (USE_MOCK) { await delay(); return mockProducts.find((p) => p.id === id) ?? null; }
  try {
    const raw = await apiGet<BackendProduct>(`/products/${id}`);
    return mapProduct(raw);
  } catch {
    return null;
  }
}

/** Backend không có trường slug — slug ở frontend chính là id dạng string */
export async function getProductBySlug(slug: string): Promise<Product | null> {
  if (USE_MOCK) { await delay(); return mockProducts.find((p) => p.slug === slug) ?? null; }
  const id = parseInt(slug, 10);
  if (isNaN(id)) return null;
  return getProductById(id);
}

export async function getProductsByCategory(categoryId: number): Promise<Product[]> {
  if (USE_MOCK) {
    await delay();
    return mockProducts.filter((p) => p.categoryProductId === categoryId);
  }
  const raw = await apiGet<BackendProduct[]>(`/products/categoryproduct/${categoryId}`);
  return raw.map(mapProduct);
}

const productService = {
  getProducts,
  getProductById,
  getProductBySlug,
  getProductsByCategory,
};

export default productService;

//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 8 – GỌI API TỪ REACTJS & HOOK USEEFFECT
// =============================================================
// Service tin tức/blog (tạo ở Buổi 7, mở rộng ở Buổi 8):
//  - getPosts / getPostById -> /api/posts , /api/posts/{id}
//  - getBlogCategories      -> /api/categories (bài tập tự làm Buổi 8)

import type { Post, BlogCategory } from '../types';
import { USE_MOCK, apiGet, delay, resolveImageUrl } from './http';
import { mockPosts, mockBlogCategories } from './mock';

/** Khớp với projection GET /api/posts (danh sach, khong co content) */
interface BackendPostList {
  id: number;
  title: string;
  imageUrl?: string | null;
  createdDate: string;
  categoryName?: string;
}

/** entity Post day du GET /api/posts/{id} */
interface BackendPostDetail {
  id: number;
  title: string;
  content: string;
  imageUrl?: string | null;
  createdDate: string;
  categoryId: number;
  category?: { id: number; name: string } | null;
}

function mapPostDetail(bp: BackendPostDetail): Post {
  return {
    id:          bp.id,
    slug:        String(bp.id),
    title:       bp.title,
    content:     bp.content,
    excerpt:     bp.content.slice(0, 150),
    imageUrl:    resolveImageUrl(bp.imageUrl),
    createdDate: bp.createdDate,
    categoryId:  bp.categoryId,
    category:    bp.category ?? { id: bp.categoryId, name: '' },
  };
}

function mapPostList(bp: BackendPostList): Post {
  return {
    id:          bp.id,
    slug:        String(bp.id),
    title:       bp.title,
    content:     '',
    excerpt:     undefined,
    imageUrl:    resolveImageUrl(bp.imageUrl),
    createdDate: bp.createdDate,
    categoryId:  0,
    category:    { id: 0, name: bp.categoryName ?? '' },
  };
}

/** Lấy danh sách bài viết (mặc định 3 bài mới nhất cho trang chủ) */
export async function getPosts(limit = 3): Promise<Post[]> {
  if (USE_MOCK) { await delay(); return mockPosts.slice(0, limit); }
  const raw = await apiGet<BackendPostList[]>('/posts');
  return raw.slice(0, limit).map(mapPostList);
}

export async function getPostById(id: number): Promise<Post | null> {
  if (USE_MOCK) { await delay(); return mockPosts.find((p) => p.id === id) ?? null; }
  try {
    const raw = await apiGet<BackendPostDetail>(`/posts/${id}`);
    return mapPostDetail(raw);
  } catch {
    return null;
  }
}

/** Backend khong co slug — slug chinh la id dang string */
export async function getPostBySlug(slug: string): Promise<Post | null> {
  if (USE_MOCK) { await delay(); return mockPosts.find((p) => p.slug === slug) ?? null; }
  const id = parseInt(slug, 10);
  if (isNaN(id)) return null;
  return getPostById(id);
}

// =============================================================
// BUỔI 8 – BÀI TẬP MỞ RỘNG: SERVICE CHUYÊN MỤC TIN TỨC (Categories)
// Route Backend: GET /api/categories  (CategoriesController – trả JSON)
// Lưu ý: KHÁC hẳn danh mục sản phẩm (categoryService -> /categoriesproducts).
//        Bảng Categories phân loại BÀI VIẾT, không phải SẢN PHẨM bán hàng.
// =============================================================

/** Khớp projection GET /api/categories (Id, Name, Description) */
interface BackendBlogCategory {
  id: number;
  name: string;
  description?: string | null;
}

function mapBlogCategory(bc: BackendBlogCategory): BlogCategory {
  return {
    id:          bc.id,
    name:        bc.name,
    description: bc.description ?? '',
  };
}

/** Lấy danh sách chuyên mục tin tức để dựng thanh lọc bài viết (List Group) */
export async function getBlogCategories(): Promise<BlogCategory[]> {
  if (USE_MOCK) { await delay(); return mockBlogCategories; }
  const raw = await apiGet<BackendBlogCategory[]>('/categories');
  return raw.map(mapBlogCategory);
}

const blogService = {
  getPosts,
  getPostById,
  getPostBySlug,
  getBlogCategories,
};

export default blogService;

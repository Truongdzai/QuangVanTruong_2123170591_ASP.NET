//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 8 – GỌI API TỪ REACTJS & HOOK USEEFFECT
// =============================================================
// useEffect + useState gọi getPosts() hiển thị danh sách bài viết (Buổi 7)
// Buổi 8 (bài tập tự làm): nhúng sidebar BlogCategoryList lọc bài theo chuyên mục
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { getPosts } from '../../services/blogService';
import type { Post, BlogCategory } from '../../types';
import { useSEO } from '../../seo';
import { formatDate } from '../../utils';
import Breadcrumb from '../../components/layout/Breadcrumb';
import BlogCategoryList from '../../components/sections/BlogCategoryList';
import { ArrowRightIcon } from '../../assets/icons/svg';
import './BlogPage.css';

export default function BlogPage(): JSX.Element {
  useSEO({ title: 'Blog', description: 'Mẹo thời trang, cẩm nang phối đồ và xu hướng mới nhất từ SHOP.CO.' });

  const [posts,   setPosts]   = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  // Buổi 8: chuyên mục đang lọc (null = xem tất cả bài viết)
  const [activeCategory, setActiveCategory] = useState<BlogCategory | null>(null);

  useEffect(() => {
    getPosts(20).then(setPosts).catch(console.error).finally(() => setLoading(false));
  }, []);

  // Lọc bài viết theo chuyên mục đang chọn (so khớp theo tên chuyên mục bài viết)
  const visiblePosts = useMemo(
    () => (activeCategory ? posts.filter((p) => p.category.name === activeCategory.name) : posts),
    [posts, activeCategory],
  );

  return (
    <div className="page-content blog-page">
      <div className="container">
        <div className="blog-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Blog' }]} />
        </div>

        <div className="section-header">
          <h1 className="section-title">Tin tức thời trang</h1>
        </div>

        <div className="blog-layout">
          {/* Buổi 8 – cột trái: List Group chuyên mục (component tự gọi API qua useEffect) */}
          <aside className="blog-sidebar">
            <BlogCategoryList activeId={activeCategory?.id ?? null} onSelect={setActiveCategory} />
          </aside>

          {/* Cột phải: danh sách bài viết lấy real-time từ Web API */}
          <div className="blog-main">
            {activeCategory && (
              <p className="blog-filter-note">
                Đang lọc theo chủ đề: <strong>{activeCategory.name}</strong>
                <button type="button" className="blog-filter-clear" onClick={() => setActiveCategory(null)}>
                  Bỏ lọc ✕
                </button>
              </p>
            )}

            {loading ? (
              <div className="blog-loading">Đang tải bài viết…</div>
            ) : visiblePosts.length === 0 ? (
              <div className="blog-loading">Chưa có bài viết nào thuộc chủ đề này.</div>
            ) : (
              <div className="blog-grid">
                {visiblePosts.map((post) => (
                  <article key={post.id} className="blog-card">
                    <Link to={`/blog/${post.id}`} className="blog-card-img-wrap">
                      <img src={post.imageUrl} alt={post.title} className="blog-card-img" />
                      <span className="blog-card-cat">{post.category.name}</span>
                    </Link>
                    <div className="blog-card-body">
                      <p className="blog-card-date">{formatDate(post.createdDate)}</p>
                      <Link to={`/blog/${post.id}`} className="blog-card-title">{post.title}</Link>
                      <p className="blog-card-excerpt">{post.excerpt ?? post.content.slice(0, 140)}…</p>
                      <Link to={`/blog/${post.id}`} className="blog-card-read">Đọc thêm <ArrowRightIcon /></Link>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 (mở rộng – Tin tức) – useEffect gọi getPosts hiển thị danh sách bài viết
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getPosts } from '../../services/blogService';
import type { Post } from '../../types';
import { useSEO } from '../../seo';
import { formatDate } from '../../utils';
import Breadcrumb from '../../components/layout/Breadcrumb';
import { ArrowRightIcon } from '../../assets/icons/svg';
import './BlogPage.css';

export default function BlogPage(): JSX.Element {
  useSEO({ title: 'Blog', description: 'Mẹo thời trang, cẩm nang phối đồ và xu hướng mới nhất từ SHOP.CO.' });

  const [posts,   setPosts]   = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getPosts(20).then(setPosts).catch(console.error).finally(() => setLoading(false));
  }, []);

  return (
    <div className="page-content blog-page">
      <div className="container">
        <div className="blog-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Blog' }]} />
        </div>

        <div className="section-header">
          <h1 className="section-title">Tin tức thời trang</h1>
        </div>

        {loading ? (
          <div className="blog-loading">Đang tải bài viết…</div>
        ) : (
          <div className="blog-grid">
            {posts.map((post) => (
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
  );
}

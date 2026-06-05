//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 (mở rộng – Tin tức) – useEffect gọi getPostById lấy chi tiết bài viết
import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getPostById } from '../../services/blogService';
import type { Post } from '../../types';
import { useSEO } from '../../seo';
import { formatDate } from '../../utils';
import Breadcrumb from '../../components/layout/Breadcrumb';
import { ArrowLeftIcon } from '../../assets/icons/svg';
import './BlogDetailPage.css';

export default function BlogDetailPage(): JSX.Element {
  const { id } = useParams<{ id: string }>();
  const [post,    setPost]    = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);

  useSEO(post ? { title: post.title, description: post.excerpt ?? post.content.slice(0, 160), image: post.imageUrl } : {});

  useEffect(() => {
    if (!id) return;
    getPostById(Number(id)).then(setPost).catch(console.error).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="page-content container bld-loading">Đang tải…</div>;
  if (!post)   return <div className="page-content container bld-loading">Không tìm thấy bài viết. <Link to="/blog">Quay lại Blog</Link></div>;

  return (
    <div className="page-content bld-page">
      <div className="container">
        <div className="bld-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Blog', href: '/blog' }, { label: post.title }]} />
        </div>

        <article className="bld-article">
          <header className="bld-header">
            <span className="bld-cat">{post.category.name}</span>
            <h1 className="bld-title">{post.title}</h1>
            <p className="bld-date">{formatDate(post.createdDate)}</p>
          </header>

          <div className="bld-hero-img">
            <img src={post.imageUrl} alt={post.title} />
          </div>

          <div className="bld-content">
            {post.content.split('\n').map((para, i) => <p key={i}>{para}</p>)}
          </div>
        </article>

        <div className="bld-back">
          <Link to="/blog" className="bld-back-link"><ArrowLeftIcon /> Quay lại Blog</Link>
        </div>
      </div>
    </div>
  );
}

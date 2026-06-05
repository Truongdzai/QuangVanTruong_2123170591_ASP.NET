//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 (mở rộng – Tin tức) – useEffect gọi getPosts() lấy bài viết mới nhất hiển thị trên trang chủ
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getPosts } from '../../services/blogService';
import type { Post } from '../../types';
import { formatDate } from '../../utils';
import { ArrowRightIcon } from '../../assets/icons/svg';
import styles from './NewsSection.module.css';

export default function NewsSection(): JSX.Element | null {
  // 1. State lưu danh sách bài viết lấy từ Backend
  const [posts, setPosts] = useState<Post[]>([]);

  // 2. useEffect tự gọi API một lần khi component mount
  useEffect(() => {
    getPosts(3).then(setPosts).catch(console.error);
  }, []);

  // 3. Chưa có bài viết (đang tải hoặc backend trống) thì ẩn cả khu vực
  if (posts.length === 0) return null;

  // 4. Render lưới tin tức ra giao diện
  return (
    <section className={styles.section}>
      <div className={styles.container}>
        <h2 className={styles.title}>TIN TỨC MỚI NHẤT</h2>
        <div className={styles.grid}>
          {posts.map((post) => (
            <Link key={post.id} to={`/blog/${post.id}`} className={styles.card}>
              <div className={styles.imgWrap}>
                <img src={post.imageUrl} alt={post.title} className={styles.img} loading="lazy" />
                <span className={styles.cat}>{post.category.name}</span>
              </div>
              <div className={styles.body}>
                <span className={styles.date}>{formatDate(post.createdDate)}</span>
                <h3 className={styles.cardTitle}>{post.title}</h3>
                <p className={styles.excerpt}>{post.excerpt ?? `${post.content.slice(0, 120)}…`}</p>
                <span className={styles.read}>Đọc thêm <ArrowRightIcon /></span>
              </div>
            </Link>
          ))}
        </div>
        <div className={styles.footer}>
          <Link to="/blog" className={styles.viewAll}>Xem tất cả</Link>
        </div>
      </div>
    </section>
  );
}

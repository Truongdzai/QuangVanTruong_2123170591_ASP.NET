//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// =============================================================
// BUỔI 8 – GỌI API TỪ REACTJS & HOOK USEEFFECT
// =============================================================
// Bài tập tự làm: Chuyên mục tin tức (Blog Category) — useState khởi tạo
// mảng rỗng + useEffect kiểm soát vòng đời để gọi API /api/categories
// ĐÚNG MỘT LẦN duy nhất khi nạp trang
import { useEffect, useState } from 'react';
import { getBlogCategories } from '../../services/blogService';
import type { BlogCategory } from '../../types';
import { SparkleIcon } from '../../assets/icons/svg';
import styles from './BlogCategoryList.module.css';

interface BlogCategoryListProps {
  /** Id chuyên mục đang chọn để tô sáng (null = đang xem tất cả).*/
  activeId?: number | null;
  /** Bấm 1 chuyên mục để lọc bài viết; gọi với null nghĩa là bỏ lọc */
  onSelect?: (category: BlogCategory | null) => void;
}

export default function BlogCategoryList({ activeId = null, onSelect }: BlogCategoryListProps): JSX.Element {
  // 1. State chứa danh sách chuyên mục lấy từ SQL Server (khởi tạo mảng rỗng)
  const [categories, setCategories] = useState<BlogCategory[]>([]);
  // State quản lý trạng thái chờ (Loading) để tối ưu trải nghiệm người dùng
  const [loading, setLoading] = useState(true);

  // 2. useEffect kiểm soát vòng đời: ép gọi API đúng 1 lần khi component mount
  useEffect(() => {
    const fetchBlogCategories = async () => {
      try {
        setLoading(true);
        // Gọi sang lớp Service chứa trục Axios tập trung
        const data = await getBlogCategories();
        setCategories(data); // Đẩy dữ liệu JSON nhận được vào State
      } catch (error) {
        console.error('Lỗi hệ thống khi gọi API chuyên mục tin tức:', error);
      } finally {
        setLoading(false); // Đóng trạng thái Loading dù thành công hay thất bại
      }
    };

    fetchBlogCategories();
  }, []); // Mảng rỗng đảm bảo không xảy ra vòng lặp render vô hạn làm treo trình duyệt

  // Chỉ bật chế độ lọc khi component cha có truyền onSelect
  const interactive = typeof onSelect === 'function';

  // 3. Giao diện tạm thời trong lúc đợi API phản hồi
  if (loading) {
    return <div className={styles.loading}>Đang nạp các chuyên mục bài viết…</div>;
  }

  // 4. Render List Group khi đã có dữ liệu thành công
  return (
    <aside className={styles.card}>
      <h3 className={styles.title}>
        <SparkleIcon /> Chủ đề bài viết
      </h3>

      {categories.length === 0 ? (
        <p className={styles.empty}>Chưa có chủ đề tin tức nào.</p>
      ) : (
        <ul className={styles.list}>
          {interactive && (
            <li>
              <button
                type="button"
                className={`${styles.item} ${activeId === null ? styles.active : ''}`}
                onClick={() => onSelect?.(null)}
              >
                <span className={styles.name}>Tất cả chủ đề</span>
              </button>
            </li>
          )}

          {categories.map((cate) => {
            const isActive = activeId === cate.id;
            const cls = `${styles.item} ${isActive ? styles.active : ''}`;
            const label = (
              <span className={styles.name}>
                <span className={styles.hash}>#</span>
                {cate.name}
              </span>
            );

            return (
              <li key={cate.id}>
                {interactive ? (
                  <button type="button" className={cls} onClick={() => onSelect?.(cate)} title={cate.description}>
                    {label}
                  </button>
                ) : (
                  <span className={styles.item} title={cate.description}>{label}</span>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </aside>
  );
}

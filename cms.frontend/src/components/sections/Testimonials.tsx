import React from 'react';
import { ArrowLeftIcon, ArrowRightIcon } from '../../assets/icons/svg';
import styles from './Testimonials.module.css';

const reviews = [
  {
    id: 1,
    author: 'Minh Anh',
    rating: 5,
    text: '"Mình thực sự ấn tượng với chất lượng và kiểu dáng quần áo nhận được từ SHOP.CO. Từ đồ thường ngày đến váy dạ hội, món nào cũng vượt mong đợi."',
  },
  {
    id: 2,
    author: 'Quốc Huy',
    rating: 5,
    text: '"Trước đây tìm trang phục hợp gu là cả một thử thách, cho đến khi mình biết SHOP.CO. Mẫu mã đa dạng, đáp ứng đủ mọi sở thích."',
  },
  {
    id: 3,
    author: 'Thu Hà',
    rating: 5,
    text: '"Là người luôn săn lùng những món thời trang độc đáo, mình rất vui khi tìm thấy SHOP.CO. Bộ sưu tập vừa đa dạng vừa bắt kịp xu hướng mới nhất."',
  },
];

const Stars: React.FC<{ count: number }> = ({ count }) => (
  <div className={styles.stars}>
    {[1, 2, 3, 4, 5].map((i) => (
      <span key={i} className={i <= count ? styles.starFull : styles.starEmpty}>★</span>
    ))}
  </div>
);

const Testimonials: React.FC = () => (
  <section className={styles.section}>
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>KHÁCH HÀNG HÀI LÒNG</h2>
        <div className={styles.nav}>
          <button className={styles.navBtn} aria-label="Trước"><ArrowLeftIcon /></button>
          <button className={styles.navBtn} aria-label="Sau"><ArrowRightIcon /></button>
        </div>
      </div>
      <div className={styles.grid}>
        {reviews.map((r) => (
          <div key={r.id} className={styles.card}>
            <Stars count={r.rating} />
            <p className={styles.author}>
              {r.author}
              <span className={styles.verified} title="Đã xác minh">✓</span>
            </p>
            <p className={styles.text}>{r.text}</p>
          </div>
        ))}
      </div>
    </div>
  </section>
);

export default Testimonials;

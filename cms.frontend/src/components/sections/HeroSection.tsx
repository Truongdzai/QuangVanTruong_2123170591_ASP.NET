import React from 'react';
import { Link } from 'react-router-dom';
import { SparkleIcon } from '../../assets/icons/svg';
import styles from './HeroSection.module.css';

// Ảnh banner đặt trong public/images/banner
const HERO_IMG = '/images/banner/Bannertam.png';

const HeroSection: React.FC = () => {
  return (
    <section className={styles.hero}>
      <img src={HERO_IMG} alt="Banner SHOP.CO" className={styles.banner} />
      <span className={styles.starLg}><SparkleIcon /></span>
      <span className={styles.starSm}><SparkleIcon /></span>

      <div className={styles.container}>
        <div className={styles.content}>
          <h1 className={styles.heading}>
            TÌM TRANG PHỤC<br />HỢP PHONG CÁCH<br />CỦA BẠN
          </h1>
          <p className={styles.subtitle}>
            Khám phá bộ sưu tập trang phục đa dạng, được chế tác tỉ mỉ,
            giúp bạn tôn lên cá tính và thể hiện phong cách riêng của mình.
          </p>
          <Link to="/products" className={styles.shopBtn}>Mua ngay</Link>

          <div className={styles.stats}>
            <div className={styles.statItem}>
              <span className={styles.statNumber}>200+</span>
              <span className={styles.statLabel}>Thương hiệu quốc tế</span>
            </div>
            <div className={styles.divider} />
            <div className={styles.statItem}>
              <span className={styles.statNumber}>2,000+</span>
              <span className={styles.statLabel}>Sản phẩm chất lượng cao</span>
            </div>
            <div className={styles.divider} />
            <div className={styles.statItem}>
              <span className={styles.statNumber}>30,000+</span>
              <span className={styles.statLabel}>Khách hàng hài lòng</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default HeroSection;

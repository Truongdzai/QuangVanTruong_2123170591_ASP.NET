import React from 'react';
import { Link } from 'react-router-dom';
import styles from './StyleSection.module.css';

// Lifestyle images — portrait ratio for tall card, landscape for wide/small cards
const categories = [
  {
    name: 'Thường ngày',
    slug: 'thuong-ngay',
    image: 'https://images.unsplash.com/photo-1434389677669-e08b4cac3105?w=400&h=450&fit=crop&q=80',
  },
  {
    name: 'Công sở',
    slug: 'cong-so',
    image: 'https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=640&h=230&fit=crop&q=80',
  },
  {
    name: 'Dự tiệc',
    slug: 'du-tiec',
    image: 'https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=230&fit=crop&q=80',
  },
  {
    name: 'Thể thao',
    slug: 'the-thao',
    image: 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400&h=230&fit=crop&q=80',
  },
];

const StyleSection: React.FC = () => (
  <section className={styles.wrapper}>
    <div className={styles.inner}>
      <h2 className={styles.title}>MUA THEO PHONG CÁCH</h2>
      <div className={styles.grid}>

        {/* Left — tall Casual card */}
        <Link to={`/products?category=${categories[0].slug}`} className={`${styles.card} ${styles.tall}`}>
          <img src={categories[0].image} alt={categories[0].name} className={styles.img} />
          <span className={styles.label}>{categories[0].name}</span>
        </Link>

        {/* Right column */}
        <div className={styles.rightCol}>
          {/* Formal — wide top */}
          <Link to={`/products?category=${categories[1].slug}`} className={`${styles.card} ${styles.wide}`}>
            <img src={categories[1].image} alt={categories[1].name} className={styles.img} />
            <span className={styles.label}>{categories[1].name}</span>
          </Link>

          {/* Party + Gym — bottom row */}
          <div className={styles.bottomRow}>
            <Link to={`/products?category=${categories[2].slug}`} className={styles.card}>
              <img src={categories[2].image} alt={categories[2].name} className={styles.img} />
              <span className={styles.label}>{categories[2].name}</span>
            </Link>
            <Link to={`/products?category=${categories[3].slug}`} className={styles.card}>
              <img src={categories[3].image} alt={categories[3].name} className={styles.img} />
              <span className={styles.label}>{categories[3].name}</span>
            </Link>
          </div>
        </div>

      </div>
    </div>
  </section>
);

export default StyleSection;

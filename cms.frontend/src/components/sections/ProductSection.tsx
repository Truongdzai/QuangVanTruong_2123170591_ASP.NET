import React from 'react';
import { Link } from 'react-router-dom';
import styles from './ProductSection.module.css';
import ProductCard from './ProductCard';
import type { Product } from '../../types';

interface Props {
  title: string;
  products: Product[];
  sectionId?: string;
  viewAllLink?: string;
}

const ProductSection: React.FC<Props> = ({
  title,
  products,
  sectionId,
  viewAllLink = '/products',
}) => (
  <section className={styles.section} id={sectionId}>
    <div className={styles.container}>
      <h2 className={styles.title}>{title}</h2>
      <div className={styles.grid}>
        {products.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
      <div className={styles.footer}>
        <div className={styles.divider} />
        <Link to={viewAllLink} className={styles.viewAll}>Xem tất cả</Link>
        <div className={styles.divider} />
      </div>
    </div>
  </section>
);

export default ProductSection;

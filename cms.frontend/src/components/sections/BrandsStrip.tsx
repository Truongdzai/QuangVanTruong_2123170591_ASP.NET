import React from 'react';
import styles from './BrandsStrip.module.css';

const brands = ['VERSACE', 'ZARA', 'GUCCI', 'PRADA', 'Calvin Klein'];

const BrandsStrip: React.FC = () => (
  <section className={styles.strip}>
    <div className={styles.container}>
      {brands.map((brand) => (
        <span key={brand} className={styles.brand}>{brand}</span>
      ))}
    </div>
  </section>
);

export default BrandsStrip;

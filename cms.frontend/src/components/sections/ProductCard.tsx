import React from 'react';
import { Link } from 'react-router-dom';
import type { Product } from '../../types';
import { formatPrice } from '../../utils';
import { PLACEHOLDER_IMAGE } from '../../configs/constants';
import Stars from '../ui/Stars';
import styles from './ProductCard.module.css';

const ProductCard: React.FC<{ product: Product }> = ({ product }) => {
  const { id, slug, name, price, originalPrice, discount, rating = 4, imageUrl } = product;
  const to = `/products/${slug ?? id}`;

  return (
    <Link to={to} className={styles.cardLink}>
      <div className={styles.card}>
        <div className={styles.imageWrap}>
          <img
            src={imageUrl ?? PLACEHOLDER_IMAGE}
            alt={name}
            className={styles.image}
            loading="lazy"
          />
          {discount && <span className={styles.badge}>-{discount}%</span>}
        </div>
        <div className={styles.info}>
          <h3 className={styles.name}>{name}</h3>
          <Stars rating={rating} size={16} showValue />
          <div className={styles.priceRow}>
            <span className={styles.price}>{formatPrice(price)}</span>
            {originalPrice && (
              <>
                <span className={styles.originalPrice}>{formatPrice(originalPrice)}</span>
                {discount && <span className={styles.discountTag}>-{discount}%</span>}
              </>
            )}
          </div>
        </div>
      </div>
    </Link>
  );
};

export default ProductCard;

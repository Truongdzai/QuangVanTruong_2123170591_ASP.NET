import styles from './Stars.module.css';

interface StarsProps {
  rating: number;
  size?: number;
  showValue?: boolean;
  className?: string;
}

const STAR_PATH =
  'M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z';
const FILLED_COLOR = '#FFC633';
const EMPTY_COLOR = '#e0e0e0';
const MAX_STARS = 5;

/** Hiển thị xếp hạng sao dùng chung cho thẻ sản phẩm, trang chi tiết, đánh giá. */
export default function Stars({ rating, size = 18, showValue = false, className }: StarsProps): JSX.Element {
  const filledCount = Math.round(rating);
  return (
    <span className={className ? `${styles.stars} ${className}` : styles.stars}>
      {Array.from({ length: MAX_STARS }, (_, i) => (
        <svg key={i} width={size} height={size} viewBox="0 0 24 24" fill={i < filledCount ? FILLED_COLOR : EMPTY_COLOR}>
          <path d={STAR_PATH} />
        </svg>
      ))}
      {showValue && <span className={styles.value}>{rating.toFixed(1)}/5</span>}
    </span>
  );
}

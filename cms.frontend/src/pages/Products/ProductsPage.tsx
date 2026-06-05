//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 – useEffect gọi getProducts/getCategories hiển thị lưới sản phẩm & bộ lọc
import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { getProducts } from '../../services/productService';
import { getCategories } from '../../services/categoryService';
import type { Product, Category } from '../../types';
import { useSEO } from '../../seo';
import Breadcrumb from '../../components/layout/Breadcrumb';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils';
import './ProductsPage.css';

const SORT_OPTIONS = [
  { value: 'default',    label: 'Phổ biến nhất' },
  { value: 'price_asc',  label: 'Giá: Thấp đến cao' },
  { value: 'price_desc', label: 'Giá: Cao đến thấp' },
  { value: 'name_asc',   label: 'Tên: A–Z' },
  { value: 'rating_desc',label: 'Đánh giá cao nhất' },
] as const;

function StarRating({ rating }: { rating: number }) {
  return (
    <span className="pp-stars" aria-label={`${rating} trên 5 sao`}>
      {Array.from({ length: 5 }, (_, i) => (
        <svg key={i} width="14" height="14" viewBox="0 0 24 24" fill={i < Math.round(rating) ? '#FFC633' : '#e0e0e0'}>
          <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
        </svg>
      ))}
      <span className="pp-rating-num">{rating.toFixed(1)}</span>
    </span>
  );
}

export default function ProductsPage(): JSX.Element {
  useSEO({ title: 'Tất cả sản phẩm', description: 'Khám phá toàn bộ bộ sưu tập thời trang của SHOP.CO.' });

  const [searchParams, setSearchParams] = useSearchParams();
  const [products,   setProducts]   = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading,    setLoading]    = useState(true);
  const [sortBy,     setSortBy]     = useState('default');
  const [activeCategory, setActiveCategory] = useState(0);
  const { addToCart, openCart } = useCart();

  useEffect(() => {
    Promise.all([getProducts(), getCategories()])
      .then(([prods, cats]) => { setProducts(prods); setCategories(cats); })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  // filter
  const filterSlug = searchParams.get('filter');
  let filtered = [...products];
  if (filterSlug === 'sale')    filtered = filtered.filter((p) => p.originalPrice !== null);
  if (filterSlug === 'new')     filtered = filtered.slice(0, 4);
  if (activeCategory > 0)       filtered = filtered.filter((p) => p.categoryProductId === activeCategory);

  // sort
  if (sortBy === 'price_asc')   filtered.sort((a, b) => a.price - b.price);
  if (sortBy === 'price_desc')  filtered.sort((a, b) => b.price - a.price);
  if (sortBy === 'name_asc')    filtered.sort((a, b) => a.name.localeCompare(b.name));
  if (sortBy === 'rating_desc') filtered.sort((a, b) => b.rating - a.rating);

  const handleAddToCart = (product: Product) => {
    addToCart(product, 1);
    openCart();
  };

  return (
    <div className="page-content pp-page">
      <div className="container">
        <div className="pp-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Cửa hàng' }]} />
        </div>

        <div className="pp-layout">
          {/* Sidebar */}
          <aside className="pp-sidebar">
            <h3 className="pp-filter-title">Bộ lọc</h3>

            <div className="pp-filter-section">
              <h4>Danh mục</h4>
              <ul className="pp-cat-list">
                <li>
                  <button
                    className={`pp-cat-btn${activeCategory === 0 ? ' active' : ''}`}
                    onClick={() => setActiveCategory(0)}
                  >Tất cả</button>
                </li>
                {categories.map((cat) => (
                  <li key={cat.id}>
                    <button
                      className={`pp-cat-btn${activeCategory === cat.id ? ' active' : ''}`}
                      onClick={() => setActiveCategory(cat.id)}
                    >{cat.name}</button>
                  </li>
                ))}
              </ul>
            </div>
          </aside>

          {/* Main content */}
          <div className="pp-main">
            <div className="pp-toolbar">
              <p className="pp-count">
                Hiển thị {filtered.length} sản phẩm
              </p>
              <select
                className="pp-sort"
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
              >
                {SORT_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>{o.label}</option>
                ))}
              </select>
            </div>

            {loading ? (
              <div className="pp-loading">Đang tải sản phẩm…</div>
            ) : filtered.length === 0 ? (
              <div className="pp-empty">Không tìm thấy sản phẩm nào.</div>
            ) : (
              <div className="pp-grid">
                {filtered.map((product) => (
                  <div key={product.id} className="pp-card">
                    <Link to={`/products/${product.slug}`} className="pp-card-img-wrap">
                      <img src={product.imageUrl} alt={product.name} className="pp-card-img" />
                      {product.badge && <span className="pp-badge">{product.badge}</span>}
                    </Link>
                    <div className="pp-card-body">
                      <Link to={`/products/${product.slug}`} className="pp-card-name">{product.name}</Link>
                      <StarRating rating={product.rating} />
                      <div className="pp-card-prices">
                        <span className="pp-price">{formatPrice(product.price)}</span>
                        {product.originalPrice && (
                          <>
                            <span className="pp-orig-price">{formatPrice(product.originalPrice)}</span>
                            {product.discount && <span className="pp-discount">-{product.discount}%</span>}
                          </>
                        )}
                      </div>
                      <button
                        className="pp-add-btn"
                        onClick={() => handleAddToCart(product)}
                      >Thêm vào giỏ</button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

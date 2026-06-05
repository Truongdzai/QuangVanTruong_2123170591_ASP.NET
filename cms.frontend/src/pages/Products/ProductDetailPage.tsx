//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 – useEffect gọi getProductBySlug lấy chi tiết sản phẩm từ Backend
import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getProductBySlug, getProductsByCategory } from '../../services/productService';
import type { Product } from '../../types';
import { useSEO, buildProductSchema } from '../../seo';
import Breadcrumb from '../../components/layout/Breadcrumb';
import Stars from '../../components/ui/Stars';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils';
import './ProductDetailPage.css';

// Đánh giá mẫu 
const REVIEWS = [
  { name: 'Minh Anh',  rating: 5, date: '14/08/2026', text: 'Chất vải đẹp đúng như mô tả, đường may chắc chắn. Mình rất hài lòng với chất lượng của SHOP.CO.' },
  { name: 'Quốc Huy',  rating: 5, date: '10/08/2026', text: 'Trước đây tìm đồ hợp gu khá khó, từ khi biết SHOP.CO thì mọi thứ dễ hơn hẳn. Mẫu mã đa dạng, rất đáng tiền.' },
  { name: 'Thu Hà',    rating: 4, date: '02/08/2026', text: 'Sản phẩm form chuẩn, mặc lên rất tôn dáng. Giao hàng nhanh, đóng gói cẩn thận. Sẽ ủng hộ tiếp.' },
  { name: 'Đức Anh',   rating: 5, date: '28/07/2026', text: 'Mình mê phong cách tối giản và SHOP.CO làm điều đó cực tốt. Màu sắc và chất liệu đều trên cả mong đợi.' },
  { name: 'Lan Phương', rating: 4, date: '21/07/2026', text: 'Giá hợp lý so với chất lượng. Áo lên màu đẹp, vải mát. Trừ nửa sao vì size hơi nhỏ so với bảng size.' },
  { name: 'Hoàng Nam', rating: 5, date: '15/07/2026', text: 'Đặt hàng lần thứ ba rồi và chưa bao giờ thất vọng. Dịch vụ và sản phẩm đều xuất sắc.' },
];

type Tab = 'details' | 'reviews' | 'faqs';

// Tên danh mục theo categoryProductId của Backend (index 0 để trống)
const CATEGORY_NAMES = ['', 'Thường ngày', 'Công sở', 'Dự tiệc', 'Thể thao'];

export default function ProductDetailPage(): JSX.Element {
  const { slug } = useParams<{ slug: string }>();
  const [product,  setProduct]  = useState<Product | null>(null);
  const [related,  setRelated]  = useState<Product[]>([]);
  const [loading,  setLoading]  = useState(true);
  const [qty,      setQty]      = useState(1);
  const [size,     setSize]     = useState('');
  const [color,    setColor]    = useState('');
  const [imgIdx,   setImgIdx]   = useState(0);
  const [tab,      setTab]      = useState<Tab>('reviews');
  const { addToCart, openCart } = useCart();

  useSEO(product ? {
    title: product.name,
    description: product.description,
    image: product.imageUrl,
    structuredData: buildProductSchema(product),
  } : {});

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    setImgIdx(0);
    getProductBySlug(slug)
      .then((p) => {
        setProduct(p);
        if (p) {
          // Buổi 7: lấy sản phẩm cùng danh mục làm "có thể bạn cũng thích"
          getProductsByCategory(p.categoryProductId)
            .then((list) => setRelated(list.filter((x) => x.id !== p.id).slice(0, 4)))
            .catch(console.error);
        }
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [slug]);

  if (loading) return <div className="pdp-loading page-content container">Đang tải…</div>;
  if (!product) return (
    <div className="pdp-not-found page-content container">
      <h2>Không tìm thấy sản phẩm</h2>
      <Link to="/products">Quay lại cửa hàng</Link>
    </div>
  );

  const images = product.gallery?.length ? product.gallery : [product.imageUrl];
  const SIZES  = product.sizes ?? ['Nhỏ', 'Vừa', 'Lớn', 'X-Lớn'];
  const colors = product.colors ?? [];

  const handleAddToCart = () => {
    addToCart(product, qty);
    openCart();
  };

  return (
    <div className="page-content pdp-page">
      <div className="container">
        <div className="pdp-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Cửa hàng', href: '/products' }, { label: product.name }]} />
        </div>

        <div className="pdp-grid">
          {/* Ảnh sản phẩm */}
          <div className="pdp-images">
            <div className="pdp-thumbnails">
              {images.map((img, i) => (
                <button key={i} className={`pdp-thumb${imgIdx === i ? ' active' : ''}`} onClick={() => setImgIdx(i)}>
                  <img src={img} alt={`${product.name} góc ${i + 1}`} />
                </button>
              ))}
            </div>
            <div className="pdp-main-img">
              <img src={images[imgIdx]} alt={product.name} />
            </div>
          </div>

          {/* Thông tin sản phẩm */}
          <div className="pdp-details">
            {product.badge && <span className="pdp-badge">{product.badge}</span>}
            <h1 className="pdp-name">{product.name}</h1>

            <div className="pdp-rating-row">
              <Stars rating={product.rating} />
              <span className="pdp-rating-val">{product.rating.toFixed(1)}/5</span>
            </div>

            <div className="pdp-prices">
              <span className="pdp-price">{formatPrice(product.price)}</span>
              {product.originalPrice && (
                <>
                  <span className="pdp-orig">{formatPrice(product.originalPrice)}</span>
                  {product.discount && <span className="pdp-disc">-{product.discount}%</span>}
                </>
              )}
            </div>

            <p className="pdp-desc">{product.description}</p>

            <div className="pdp-divider" />

            {/* Chọn màu */}
            {colors.length > 0 && (
              <>
                <div className="pdp-section">
                  <h4>Chọn màu</h4>
                  <div className="pdp-colors">
                    {colors.map((c) => (
                      <button
                        key={c}
                        className={`pdp-color${color === c ? ' active' : ''}`}
                        style={{ background: c }}
                        onClick={() => setColor(c)}
                        aria-label={`Màu ${c}`}
                      >
                        {color === c && (
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
                            <path d="M5 12l5 5 9-9" stroke="#fff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"/>
                          </svg>
                        )}
                      </button>
                    ))}
                  </div>
                </div>
                <div className="pdp-divider" />
              </>
            )}

            {/* Chọn kích cỡ */}
            <div className="pdp-section">
              <h4>Chọn kích cỡ</h4>
              <div className="pdp-sizes">
                {SIZES.map((s) => (
                  <button key={s} className={`pdp-size-btn${size === s ? ' active' : ''}`} onClick={() => setSize(s)}>
                    {s}
                  </button>
                ))}
              </div>
            </div>

            <div className="pdp-divider" />

            {/* Số lượng + Thêm vào giỏ */}
            <div className="pdp-add-row">
              <div className="pdp-qty-ctrl">
                <button onClick={() => setQty((q) => Math.max(1, q - 1))} aria-label="Giảm">−</button>
                <span>{qty}</span>
                <button onClick={() => setQty((q) => q + 1)} aria-label="Tăng">+</button>
              </div>
              <button className="pdp-add-btn" onClick={handleAddToCart}>
                Thêm vào giỏ
              </button>
            </div>
          </div>
        </div>

        {/* Tabs: Chi tiết / Đánh giá / Hỏi & Đáp */}
        <div className="pdp-tabs">
          <div className="pdp-tablist" role="tablist">
            <button className={`pdp-tab${tab === 'details' ? ' active' : ''}`} onClick={() => setTab('details')}>Chi tiết sản phẩm</button>
            <button className={`pdp-tab${tab === 'reviews' ? ' active' : ''}`} onClick={() => setTab('reviews')}>Đánh giá &amp; Nhận xét</button>
            <button className={`pdp-tab${tab === 'faqs' ? ' active' : ''}`} onClick={() => setTab('faqs')}>Hỏi &amp; Đáp</button>
          </div>

          {tab === 'details' && (
            <div className="pdp-tabpanel">
              <p className="pdp-desc">{product.description}</p>
              <ul className="pdp-specs">
                <li><span>Danh mục</span><strong>{CATEGORY_NAMES[product.categoryProductId] ?? '—'}</strong></li>
                <li><span>Tình trạng</span><strong>{product.stockQuantity > 0 ? `Còn hàng (${product.stockQuantity})` : 'Hết hàng'}</strong></li>
                <li><span>Chất liệu</span><strong>Cotton cao cấp</strong></li>
                <li><span>Xuất xứ</span><strong>Việt Nam</strong></li>
              </ul>
            </div>
          )}

          {tab === 'reviews' && (
            <div className="pdp-tabpanel">
              <div className="pdp-reviews-head">
                <h3>Tất cả đánh giá <span>({product.reviewCount ?? REVIEWS.length})</span></h3>
                <button className="pdp-write-review">Viết đánh giá</button>
              </div>
              <div className="pdp-reviews-grid">
                {REVIEWS.map((r) => (
                  <div className="pdp-review" key={r.name}>
                    <Stars rating={r.rating} size={16} className="pdp-review-stars" />
                    <div className="pdp-review-name">
                      {r.name}
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="#01AB31"><path d="M12 2a10 10 0 100 20 10 10 0 000-20zm-1.2 14.2l-4-4 1.4-1.4 2.6 2.6 5.6-5.6 1.4 1.4-7 7z"/></svg>
                    </div>
                    <p className="pdp-review-text">“{r.text}”</p>
                    <span className="pdp-review-date">Đăng ngày {r.date}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {tab === 'faqs' && (
            <div className="pdp-tabpanel">
              <ul className="pdp-faqs">
                <li><strong>Thời gian giao hàng bao lâu?</strong><p>Đơn nội thành 1–2 ngày, các tỉnh khác 2–5 ngày làm việc.</p></li>
                <li><strong>Có được đổi trả không?</strong><p>Hỗ trợ đổi trả trong 7 ngày nếu sản phẩm còn nguyên tem mác.</p></li>
                <li><strong>Làm sao chọn đúng size?</strong><p>Bạn tham khảo bảng size ở mỗi sản phẩm hoặc nhắn tin để được tư vấn.</p></li>
              </ul>
            </div>
          )}
        </div>

        {/* Sản phẩm liên quan */}
        {related.length > 0 && (
          <section className="pdp-related">
            <h2 className="pdp-related-title">CÓ THỂ BẠN CŨNG THÍCH</h2>
            <div className="pdp-related-grid">
              {related.map((p) => (
                <Link key={p.id} to={`/products/${p.slug}`} className="pdp-rel-card">
                  <div className="pdp-rel-img">
                    <img src={p.imageUrl} alt={p.name} />
                  </div>
                  <p className="pdp-rel-name">{p.name}</p>
                  <p className="pdp-rel-price">{formatPrice(p.price)}</p>
                </Link>
              ))}
            </div>
          </section>
        )}
      </div>
    </div>
  );
}

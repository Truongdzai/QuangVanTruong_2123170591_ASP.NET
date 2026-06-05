import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useSEO } from '../../seo';
import Breadcrumb from '../../components/layout/Breadcrumb';
import { formatPrice } from '../../utils';
import { SuccessIcon, VisaIcon, MastercardIcon, PaypalIcon, ApplePayIcon, GooglePayIcon, ArrowLeftIcon } from '../../assets/icons/svg';
import './CheckoutPage.css';

type Step = 'shipping' | 'payment' | 'confirmation';

export default function CheckoutPage(): JSX.Element {
  useSEO({ title: 'Thanh toán', description: 'Hoàn tất đơn hàng SHOP.CO của bạn.' });

  const { items, totalPrice, clearCart } = useCart();
  const [step, setStep] = useState<Step>('shipping');
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '',
    address: '', city: '', zip: '', country: 'US',
  });

  const handleField = (field: string, value: string) =>
    setForm((f) => ({ ...f, [field]: value }));

  const handleShipping = (e: FormEvent) => {
    e.preventDefault();
    setStep('payment');
  };

  const handlePayment = (e: FormEvent) => {
    e.preventDefault();
    clearCart();
    setStep('confirmation');
  };

  if (items.length === 0 && step !== 'confirmation') {
    return (
      <div className="page-content co-empty">
        <div className="container">
          <h2>Giỏ hàng của bạn đang trống</h2>
          <Link to="/products" className="co-back-btn">Tiếp tục mua sắm</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="page-content co-page">
      <div className="container">
        <div className="co-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Giỏ hàng', href: '/' }, { label: 'Thanh toán' }]} />
        </div>

        <div className="co-layout">
          {/* Left: form */}
          <div className="co-form-col">
            {step === 'confirmation' ? (
              <div className="co-confirm">
                <div className="co-confirm-icon"><SuccessIcon /></div>
                <h2>Đặt hàng thành công!</h2>
                <p>Cảm ơn bạn đã mua sắm. Chúng tôi sẽ gửi email xác nhận trong giây lát.</p>
                <Link to="/" className="co-back-btn">Tiếp tục mua sắm</Link>
              </div>
            ) : step === 'shipping' ? (
              <>
                <h2 className="co-section-title">Thông tin giao hàng</h2>
                <form onSubmit={handleShipping} className="co-form">
                  <div className="co-row">
                    <div className="co-field">
                      <label>Họ</label>
                      <input required value={form.firstName} onChange={(e) => handleField('firstName', e.target.value)} placeholder="Nguyễn Văn" />
                    </div>
                    <div className="co-field">
                      <label>Tên</label>
                      <input required value={form.lastName} onChange={(e) => handleField('lastName', e.target.value)} placeholder="A" />
                    </div>
                  </div>
                  <div className="co-field">
                    <label>Email</label>
                    <input required type="email" value={form.email} onChange={(e) => handleField('email', e.target.value)} placeholder="ban@example.com" />
                  </div>
                  <div className="co-field">
                    <label>Địa chỉ</label>
                    <input required value={form.address} onChange={(e) => handleField('address', e.target.value)} placeholder="123 Đường ABC" />
                  </div>
                  <div className="co-row">
                    <div className="co-field">
                      <label>Tỉnh / Thành phố</label>
                      <input required value={form.city} onChange={(e) => handleField('city', e.target.value)} placeholder="TP. Hồ Chí Minh" />
                    </div>
                    <div className="co-field">
                      <label>Mã bưu chính</label>
                      <input required value={form.zip} onChange={(e) => handleField('zip', e.target.value)} placeholder="700000" />
                    </div>
                  </div>
                  <button type="submit" className="co-submit-btn">Tiếp tục thanh toán</button>
                </form>
              </>
            ) : (
              <>
                <h2 className="co-section-title">Thanh toán</h2>
                <p className="co-payment-note">Đây là bản demo. Không có giao dịch thanh toán thật nào được xử lý.</p>
                <div className="co-cards" aria-label="Phương thức thanh toán được chấp nhận">
                  <VisaIcon /><MastercardIcon /><PaypalIcon /><ApplePayIcon /><GooglePayIcon />
                </div>
                <form onSubmit={handlePayment} className="co-form">
                  <div className="co-field">
                    <label>Số thẻ</label>
                    <input required placeholder="4242 4242 4242 4242" maxLength={19} />
                  </div>
                  <div className="co-row">
                    <div className="co-field">
                      <label>Hết hạn</label>
                      <input required placeholder="MM/YY" maxLength={5} />
                    </div>
                    <div className="co-field">
                      <label>CVV</label>
                      <input required placeholder="123" maxLength={4} />
                    </div>
                  </div>
                  <div className="co-btn-row">
                    <button type="button" className="co-back-link" onClick={() => setStep('shipping')}><ArrowLeftIcon /> Quay lại</button>
                    <button type="submit" className="co-submit-btn">Đặt hàng</button>
                  </div>
                </form>
              </>
            )}
          </div>

          {/* Right: order summary */}
          {step !== 'confirmation' && (
            <div className="co-summary">
              <h3 className="co-summary-title">Tóm tắt đơn hàng</h3>
              <div className="co-summary-items">
                {items.map((item) => (
                  <div key={item.id} className="co-summary-item">
                    <img src={item.imageUrl} alt={item.name} className="co-summary-img" />
                    <div className="co-summary-info">
                      <p className="co-summary-name">{item.name}</p>
                      <p className="co-summary-qty">SL: {item.quantity}</p>
                    </div>
                    <span className="co-summary-price">{formatPrice(item.price * item.quantity)}</span>
                  </div>
                ))}
              </div>
              <div className="co-summary-totals">
                <div className="co-summary-row">
                  <span>Tạm tính</span><span>{formatPrice(totalPrice)}</span>
                </div>
                <div className="co-summary-row">
                  <span>Phí giao hàng</span><span>Miễn phí</span>
                </div>
                <div className="co-summary-row co-summary-total">
                  <span>Tổng cộng</span><span>{formatPrice(totalPrice)}</span>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

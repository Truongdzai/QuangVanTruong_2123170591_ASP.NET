import { Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { formatPrice } from '../../utils';
const css = `
.shopco-overlay{position:fixed;inset:0;background:rgba(0,0,0,0.4);z-index:999}
.shopco-drawer{position:fixed;top:0;right:0;height:100%;width:420px;max-width:100vw;background:#fff;z-index:1000;display:flex;flex-direction:column;box-shadow:-4px 0 20px rgba(0,0,0,.1)}
.shopco-drawer-header{display:flex;align-items:center;justify-content:space-between;padding:1.25rem 1.5rem;border-bottom:1px solid #f0f0f0}
.shopco-drawer-header h2{font-size:1.1rem;font-weight:700}
.shopco-close-btn{background:none;border:none;font-size:1.5rem;cursor:pointer;color:#000;line-height:1}
.shopco-drawer-body{flex:1;overflow-y:auto;padding:1rem 1.5rem}
.shopco-cart-empty{text-align:center;padding:3rem 0;color:#666}
.shopco-cart-item{display:flex;gap:1rem;padding:1rem 0;border-bottom:1px solid #f5f5f5}
.shopco-cart-img{width:80px;height:80px;object-fit:cover;border-radius:8px;background:#f0f0f0;flex-shrink:0}
.shopco-cart-info{flex:1}
.shopco-cart-name{font-size:.9rem;font-weight:600;margin-bottom:.25rem}
.shopco-cart-price{font-size:.9rem;font-weight:700}
.shopco-qty-row{display:flex;align-items:center;gap:.5rem;margin-top:.5rem}
.shopco-qty-btn{width:28px;height:28px;border:1px solid #e0e0e0;background:#fff;border-radius:6px;cursor:pointer;font-size:1rem;display:flex;align-items:center;justify-content:center}
.shopco-qty-num{min-width:24px;text-align:center;font-size:.9rem;font-weight:600}
.shopco-remove-btn{background:none;border:none;color:#999;cursor:pointer;font-size:.85rem;padding:0;margin-left:auto;align-self:flex-start}
.shopco-drawer-footer{padding:1.25rem 1.5rem;border-top:1px solid #f0f0f0}
.shopco-total{display:flex;justify-content:space-between;font-weight:700;font-size:1rem;margin-bottom:1rem}
.shopco-checkout-btn{display:block;width:100%;padding:1rem;background:#000;color:#fff;border:none;border-radius:62px;font-size:1rem;font-weight:700;cursor:pointer;text-align:center;text-decoration:none;transition:background .2s}
.shopco-checkout-btn:hover{background:#333}
.shopco-clear-btn{display:block;width:100%;margin-top:.75rem;background:none;border:none;color:#999;font-size:.85rem;cursor:pointer}
`;

export default function CartDrawer(): JSX.Element {
  const { items, isOpen, totalItems, totalPrice, removeItem, updateQty, clearCart, closeCart } = useCart();

  if (!isOpen) return <></>;

  return (
    <>
      <style>{css}</style>
      <div className="shopco-overlay" onClick={closeCart} />
      <div className="shopco-drawer">
        <div className="shopco-drawer-header">
          <h2>Giỏ hàng ({totalItems})</h2>
          <button className="shopco-close-btn" onClick={closeCart} aria-label="Đóng giỏ hàng">×</button>
        </div>

        <div className="shopco-drawer-body">
          {items.length === 0 ? (
            <p className="shopco-cart-empty">Giỏ hàng của bạn đang trống.</p>
          ) : (
            items.map((item) => (
              <div key={item.id} className="shopco-cart-item">
                <img src={item.imageUrl} alt={item.name} className="shopco-cart-img" />
                <div className="shopco-cart-info">
                  <p className="shopco-cart-name">{item.name}</p>
                  <p className="shopco-cart-price">{formatPrice(item.price)}</p>
                  <div className="shopco-qty-row">
                    <button className="shopco-qty-btn" onClick={() => updateQty(item.id, item.quantity - 1)}>−</button>
                    <span className="shopco-qty-num">{item.quantity}</span>
                    <button className="shopco-qty-btn" onClick={() => updateQty(item.id, item.quantity + 1)}>+</button>
                    <button className="shopco-remove-btn" onClick={() => removeItem(item.id)}>Xóa</button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>

        {items.length > 0 && (
          <div className="shopco-drawer-footer">
            <div className="shopco-total">
              <span>Tổng cộng</span>
              <span>{formatPrice(totalPrice)}</span>
            </div>
            <Link to="/checkout" className="shopco-checkout-btn" onClick={closeCart}>
              Tiến hành thanh toán
            </Link>
            <button className="shopco-clear-btn" onClick={clearCart}>Xóa giỏ hàng</button>
          </div>
        )}
      </div>
    </>
  );
}

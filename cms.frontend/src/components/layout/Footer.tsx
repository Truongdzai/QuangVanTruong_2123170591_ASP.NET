import { Link } from 'react-router-dom';
import { ApplePayIcon, MastercardIcon, GooglePayIcon, VisaIcon, PaypalIcon, FacebookIcon, MessageIcon, ZaloIcon } from '../../assets/icons/svg';
const css = String.raw`
.ft{background:#fff;border-top:1px solid rgba(0,0,0,.1);padding:48px 0 32px}
.ft-inner{max-width:1240px;margin:0 auto;padding:0 40px}
.ft-top{display:grid;grid-template-columns:1fr auto;gap:48px;padding-bottom:32px;border-bottom:1px solid rgba(0,0,0,.1)}
.ft-logo{font-family:var(--font-sans);font-weight:700;font-size:24px;color:#000;text-decoration:none;letter-spacing:.5px;display:block;margin-bottom:16px}
.ft-desc{font-size:14px;color:rgba(0,0,0,.6);line-height:1.6;max-width:220px;margin-bottom:24px}
.ft-socials{display:flex;gap:12px}
.ft-social{width:40px;height:40px;background:#f4f4f4;border-radius:50%;display:flex;align-items:center;justify-content:center;text-decoration:none;transition:transform .2s,background .2s}
.ft-social:hover{transform:translateY(-2px);background:#ececec}
.ft-social svg{height:20px;width:auto;display:block}
.ft-links{display:grid;grid-template-columns:repeat(4,1fr);gap:32px}
.ft-col h4{font-size:14px;font-weight:700;letter-spacing:.05em;margin-bottom:20px}
.ft-col ul{list-style:none;padding:0;margin:0;display:flex;flex-direction:column;gap:12px}
.ft-col ul a{font-size:14px;color:rgba(0,0,0,.6);text-decoration:none;transition:color .2s}
.ft-col ul a:hover{color:#000}
.ft-bottom{display:flex;align-items:center;justify-content:space-between;padding-top:24px;gap:16px;flex-wrap:wrap}
.ft-copy{font-size:14px;color:rgba(0,0,0,.6)}
.ft-payments{display:flex;gap:8px;flex-wrap:wrap}
.ft-pay{background:#f4f4f4;border:1px solid #e8e8e8;border-radius:6px;padding:5px 8px;display:flex;align-items:center;justify-content:center}
.ft-pay svg{height:24px;width:auto;display:block}
@media(max-width:900px){.ft-top{grid-template-columns:1fr}.ft-links{grid-template-columns:repeat(2,1fr)}.ft-bottom{flex-direction:column;align-items:flex-start}}
`;

const NAV_COLUMNS = [
  { heading: 'CÔNG TY',    items: [['Giới thiệu', '/about'], ['Tính năng', '/about'], ['Sản phẩm', '/products'], ['Tuyển dụng', '/contact']] },
  { heading: 'HỖ TRỢ',     items: [['Hỗ trợ khách hàng', '/contact'], ['Thông tin giao hàng', '/checkout'], ['Điều khoản & Điều kiện', '/about'], ['Chính sách bảo mật', '/about']] },
  { heading: 'CÂU HỎI THƯỜNG GẶP', items: [['Tài khoản', '/login'], ['Quản lý giao hàng', '/checkout'], ['Đơn hàng', '/checkout'], ['Thanh toán', '/checkout']] },
  { heading: 'TÀI NGUYÊN', items: [['eBook miễn phí', '/blog'], ['Hướng dẫn', '/blog'], ['Blog hướng dẫn', '/blog'], ['Playlist YouTube', '/blog']] },
] as const;

export default function Footer(): JSX.Element {
  return (
    <>
      <style>{css}</style>
      <footer className="ft">
        <div className="ft-inner">
          <div className="ft-top">
            <div>
              <Link to="/" className="ft-logo">SHOP.CO</Link>
              <p className="ft-desc">
                Chúng tôi có những trang phục hợp phong cách của bạn và khiến bạn
                tự hào khi khoác lên mình. Từ nữ đến nam.
              </p>
              <div className="ft-socials">
                <a href="https://facebook.com" className="ft-social" aria-label="Facebook" target="_blank" rel="noopener noreferrer">
                  <FacebookIcon />
                </a>
                <a href="https://m.me" className="ft-social" aria-label="Messenger" target="_blank" rel="noopener noreferrer">
                  <MessageIcon />
                </a>
                <a href="https://zalo.me" className="ft-social" aria-label="Zalo" target="_blank" rel="noopener noreferrer">
                  <ZaloIcon />
                </a>
              </div>
            </div>

            <div className="ft-links">
              {NAV_COLUMNS.map(({ heading, items }) => (
                <div key={heading} className="ft-col">
                  <h4>{heading}</h4>
                  <ul>
                    {items.map(([label, href]) => (
                      <li key={label}><Link to={href}>{label}</Link></li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          </div>

          <div className="ft-bottom">
            <p className="ft-copy">SHOP.CO Create by Quang Van Truong.</p>
            <div className="ft-payments">
              <div className="ft-pay"><VisaIcon /></div>
              <div className="ft-pay"><MastercardIcon /></div>
              <div className="ft-pay"><PaypalIcon /></div>
              <div className="ft-pay"><ApplePayIcon /></div>
              <div className="ft-pay"><GooglePayIcon /></div>
            </div>
          </div>
        </div>
      </footer>
    </>
  );
}

import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';
import { CartIcon, ProfileIcon } from '../../assets/icons/svg';
import styles from './Navbar.module.css';

// ── Icon SVG dùng lại trong Navbar (gom về một chỗ, tránh lặp inline) ────────
function SearchIcon({ size = 18, stroke = '#666', strokeWidth = 2 }: {
  size?: number; stroke?: string; strokeWidth?: number;
}): JSX.Element {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <circle cx="11" cy="11" r="8" stroke={stroke} strokeWidth={strokeWidth} />
      <path d="m21 21-4.35-4.35" stroke={stroke} strokeWidth={strokeWidth} strokeLinecap="round" />
    </svg>
  );
}

function CloseIcon({ size = 16 }: { size?: number }): JSX.Element {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}

function ChevronDown(): JSX.Element {
  return (
    <svg width="12" height="7" viewBox="0 0 12 7" fill="none">
      <path d="M1 1l5 5 5-5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function ChevronRight(): JSX.Element {
  return (
    <svg width="8" height="14" viewBox="0 0 8 14" fill="none">
      <path d="M1 1l6 6-6 6" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
    </svg>
  );
}

// Nguồn dữ liệu duy nhất cho các link điều hướng (dùng cho cả desktop & mobile)
const NAV_LINKS: { to: string; label: string; hasDropdown?: boolean }[] = [
  { to: '/products', label: 'Cửa hàng', hasDropdown: true },
  { to: '/products?filter=sale', label: 'Khuyến mãi' },
  { to: '/products?filter=new', label: 'Hàng mới về' },
  { to: '/blog', label: 'Tin tức' },
  { to: '/about', label: 'Thương hiệu' },
];

export default function Navbar(): JSX.Element {
  const [menuOpen, setMenuOpen] = useState(false);
  const [annHidden, setAnnHidden] = useState(false);
  const { totalItems, toggleCart } = useCart();
  const { isLoggedIn } = useAuth();

  const closeMenu = () => setMenuOpen(false);

  return (
    <>
      {/* ── Announcement bar ── */}
      {!annHidden && (
        <div className={styles.annBar}>
          Đăng ký nhận giảm 20% cho đơn hàng đầu tiên.{' '}
          <Link to="/register">Đăng ký ngay </Link>
          <button className={styles.annClose} onClick={() => setAnnHidden(true)} aria-label="Đóng">
            <CloseIcon size={14} />
          </button>
        </div>
      )}

      {/* Desktop navbar  */}
      <nav className={styles.navRoot}>
        <div className={styles.navInner}>
          {/* Hamburger (mobile only) */}
          <button className={styles.navHamburger} onClick={() => setMenuOpen(true)} aria-label="Mở menu">
            <span /><span /><span />
          </button>

          {/* Logo */}
          <Link to="/" className={styles.navLogo}>SHOP.CO</Link>

          {/* Desktop nav links */}
          <ul className={styles.navLinks}>
            {NAV_LINKS.map((link) => (
              <li key={link.to}>
                <Link to={link.to}>
                  {link.label}
                  {link.hasDropdown && <ChevronDown />}
                </Link>
              </li>
            ))}
          </ul>

          {/* Desktop search bar */}
          <div className={styles.navSearch}>
            <SearchIcon />
            <input type="text" placeholder="Tìm kiếm sản phẩm..." />
          </div>

          {/* Icons (desktop + mobile) */}
          <div className={styles.navIcons}>
            {/* Mobile search icon — only visible on mobile */}
            <button
              className={`${styles.navIconBtn} ${styles.navSearchMobile}`}
              aria-label="Tìm kiếm"
              onClick={() => setMenuOpen(true)}
            >
              <SearchIcon size={22} stroke="currentColor" strokeWidth={1.8} />
            </button>

            {/* Cart */}
            <button className={styles.navIconBtn} aria-label="Giỏ hàng" onClick={toggleCart}>
              <CartIcon />
              {totalItems > 0 && <span className={styles.cartBadge}>{totalItems}</span>}
            </button>

            {/* Account */}
            <Link to={isLoggedIn ? '/account' : '/login'} className={styles.navIconBtn} aria-label="Tài khoản">
              <ProfileIcon />
            </Link>
          </div>
        </div>
      </nav>

      {/*  Mobile full-screen menu */}
      <div className={`${styles.navMobileMenu}${menuOpen ? ` ${styles.open}` : ''}`} role="dialog" aria-label="Menu điều hướng">
        <div className={styles.navMobileTop}>
          <Link to="/" className={styles.navLogo} onClick={closeMenu}>SHOP.CO</Link>
          <button className={styles.navMobileClose} onClick={closeMenu} aria-label="Đóng menu">
            <CloseIcon size={18} />
          </button>
        </div>

        <nav className={styles.navMobileLinks}>
          {NAV_LINKS.map((link) => (
            <Link key={link.to} to={link.to} onClick={closeMenu}>
              {link.label} <ChevronRight />
            </Link>
          ))}
        </nav>

        {/* Search in mobile menu */}
        <div className={styles.navMobileSearch}>
          <SearchIcon />
          <input type="text" placeholder="Tìm kiếm sản phẩm..." />
        </div>
      </div>
    </>
  );
}

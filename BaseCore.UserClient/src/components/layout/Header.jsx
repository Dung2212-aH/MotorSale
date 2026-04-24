import { Link, NavLink } from 'react-router-dom';
import Icon from '../common/Icon.jsx';

const navItems = [
  { label: 'Trang chủ', href: '/' },
  { label: 'Giới thiệu', href: '/gioi-thieu' },
  { label: 'Sản phẩm', href: '/san-pham', hasDropdown: true },
  { label: 'Tin tức', href: '/tin-tuc', hasDropdown: true },
  { label: 'Liên hệ', href: '/lien-he' },
  { label: 'Hệ thống cửa hàng', href: '/he-thong-cua-hang' },
  { label: 'Câu hỏi thường gặp', href: '/cau-hoi-thuong-gap' },
];

function Header({ cartCount = 0, favoriteCount = 0 }) {
  return (
    <header className="site-header">
      <div className="top-bar">
        <div className="container header-row">
          <Link to="/he-thong-cua-hang">Hệ thống cửa hàng</Link>
          <div className="top-links">
            <Link to="/login">Đăng nhập</Link>
            <span aria-hidden="true">/</span>
            <Link to="/register">Đăng ký</Link>
          </div>
        </div>
      </div>

      <div className="main-header">
        <div className="container header-main-grid">
          <button className="mobile-menu-button" type="button" aria-label="Mở menu">
            <Icon name="menu" />
          </button>

          <Link className="brand" to="/">
            <span className="brand-mark">EURO</span>
            <span className="brand-sub">Moto</span>
          </Link>

          <nav className="main-nav" aria-label="Menu chính">
            {navItems.map((item) => (
              <NavLink key={item.label} to={item.href} className="nav-link">
                {item.label}
                {item.hasDropdown && <Icon name="chevron" size={14} />}
              </NavLink>
            ))}
          </nav>

          <div className="header-actions">
            <Link className="action-icon search-trigger" to="/search" aria-label="Tìm kiếm">
              <Icon name="search" />
            </Link>
            <Link className="action-icon" to="/favorites" aria-label="Danh sách yêu thích">
              <Icon name="heart" />
              <span>{favoriteCount}</span>
            </Link>
            <Link className="action-icon active" to="/cart" aria-label="Giỏ hàng">
              <Icon name="cart" />
              <span>{cartCount}</span>
            </Link>
          </div>
        </div>
      </div>
    </header>
  );
}

export default Header;

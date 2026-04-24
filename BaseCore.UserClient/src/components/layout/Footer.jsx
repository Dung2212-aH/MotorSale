import { Link } from 'react-router-dom';
import Icon from '../common/Icon.jsx';

function Footer({ cartCount = 0, favoriteCount = 0 }) {
  return (
    <>
      <footer className="site-footer">
        <div className="container footer-grid">
          <div className="footer-about">
            <Link className="footer-logo" to="/">
              EURO Moto
            </Link>
            <p>
              Chào mừng quý khách đến với Dola Moto - điểm đến đáng tin cậy cho những người yêu thích xe máy.
              Chúng tôi cung cấp các dòng xe máy chất lượng từ Honda, Yamaha và SYM.
            </p>
            <ul className="footer-contact">
              <li>Địa chỉ: 70 Lữ Gia, Phường 15, Quận 11, TP.HCM</li>
              <li>Điện thoại: 1900 6750</li>
              <li>Email: support@euro-moto.vn</li>
            </ul>
          </div>

          <div>
            <h3>Hướng dẫn</h3>
            <Link to="/huong-dan-mua-hang">Hướng dẫn mua hàng</Link>
            <Link to="/huong-dan-thanh-toan">Hướng dẫn thanh toán</Link>
            <Link to="/huong-dan-giao-nhan">Hướng dẫn giao nhận</Link>
            <Link to="/dieu-khoan-dich-vu">Điều khoản dịch vụ</Link>
            <Link to="/cau-hoi-thuong-gap">Câu hỏi thường gặp</Link>
          </div>

          <div>
            <h3>Chính sách</h3>
            <Link to="/chinh-sach-thanh-vien">Chính sách thành viên</Link>
            <Link to="/chinh-sach-thanh-toan">Chính sách thanh toán</Link>
            <Link to="/bao-mat-thong-tin">Bảo mật thông tin cá nhân</Link>
            <Link to="/chinh-sach-van-chuyen">Chính sách vận chuyển và giao nhận</Link>
          </div>

          <div className="footer-newsletter">
            <h3>Nhận tin khuyến mãi</h3>
            <div className="newsletter-box">
              <input type="email" placeholder="Email của bạn" aria-label="Email nhận tin khuyến mãi" />
              <button type="button">Đăng ký</button>
            </div>
            <h3>Liên kết sàn</h3>
            <div className="market-links">
              <span>S</span>
              <span>L</span>
              <span>T</span>
              <span>F</span>
              <span>Z</span>
            </div>
          </div>
        </div>

        <div className="footer-bottom">
          <div className="container">Bản quyền thuộc về Dola theme. Cung cấp bởi Sapo</div>
        </div>
      </footer>

      <nav className="mobile-bottom-nav" aria-label="Điều hướng mobile">
        <Link to="/">
          <Icon name="home" />
          <span>Trang chủ</span>
        </Link>
        <Link to="/san-pham">
          <Icon name="grid" />
          <span>Danh mục</span>
        </Link>
        <Link className="active" to="/cart">
          <Icon name="cart" />
          <span>Giỏ hàng {cartCount}</span>
        </Link>
        <Link to="/favorites">
          <Icon name="heart" />
          <span>Yêu thích {favoriteCount}</span>
        </Link>
        <Link to="/he-thong-cua-hang">
          <Icon name="pin" />
          <span>Hệ thống</span>
        </Link>
      </nav>
    </>
  );
}

export default Footer;

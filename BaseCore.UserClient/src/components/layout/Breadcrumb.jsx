import { Link } from 'react-router-dom';

function Breadcrumb() {
  return (
    <div className="breadcrumb-wrap">
      <div className="container">
        <ol className="breadcrumb-list">
          <li>
            <Link to="/">Trang chủ</Link>
          </li>
          <li>Giỏ hàng</li>
        </ol>
      </div>
    </div>
  );
}

export default Breadcrumb;

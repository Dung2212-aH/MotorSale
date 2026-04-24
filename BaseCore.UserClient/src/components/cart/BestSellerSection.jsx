import { Link } from 'react-router-dom';
import { formatCurrency } from '../../utils/formatters.js';

function ProductThumb({ product }) {
  if (product.imageUrl) {
    return <img src={product.imageUrl} alt={product.name} loading="lazy" />;
  }

  return (
    <div className="best-seller-placeholder">
      <span>EURO</span>
    </div>
  );
}

function BestSellerSection({ products, loading, error }) {
  return (
    <section className="best-seller-section">
      <div className="container">
        <div className="section-heading">
          <h2>Sản phẩm bán chạy</h2>
          <Link to="/san-pham">Xem tất cả</Link>
        </div>

        {loading && (
          <div className="product-grid">
            {Array.from({ length: 5 }).map((_, index) => (
              <div className="product-card skeleton-card" key={index}>
                <span />
                <strong />
                <em />
              </div>
            ))}
          </div>
        )}

        {!loading && error && <div className="inline-error">{error}</div>}

        {!loading && !error && products.length > 0 && (
          <div className="product-grid">
            {products.map((product) => {
              const discount =
                product.oldPrice > product.price ? Math.round(100 - (product.price / product.oldPrice) * 100) : 0;

              return (
                <article className="product-card" key={product.id}>
                  {discount > 0 && (
                    <div className="sale-badge">
                      <span>Giảm</span>
                      <strong>{discount}%</strong>
                    </div>
                  )}
                  <Link className="product-thumb" to={`/san-pham/${product.slug || product.id}`}>
                    <ProductThumb product={product} />
                    <span>Xem chi tiết</span>
                  </Link>
                  <h3>
                    <Link to={`/san-pham/${product.slug || product.id}`}>{product.name}</Link>
                  </h3>
                  <div className="product-price">
                    <span>Giá: {formatCurrency(product.price)}</span>
                    {product.oldPrice > product.price && <del>{formatCurrency(product.oldPrice)}</del>}
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </div>
    </section>
  );
}

export default BestSellerSection;

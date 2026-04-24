import { Link } from 'react-router-dom';
import Icon from '../common/Icon.jsx';
import { formatCurrency } from '../../utils/formatters.js';

function ProductImage({ src, name }) {
  if (src) {
    return <img src={src} alt={name} loading="lazy" />;
  }

  return (
    <div className="product-image-placeholder" aria-label={name}>
      <span>EURO</span>
    </div>
  );
}

function CartItemRow({ item, onQuantityChange, onRemove, busy }) {
  const productUrl = item.product.slug ? `/san-pham/${item.product.slug}` : `/san-pham/${item.productId}`;

  return (
    <div className="cart-item-row">
      <div className="cart-product-cell">
        <Link className="cart-product-image" to={productUrl}>
          <ProductImage src={item.product.imageUrl} name={item.product.name} />
        </Link>
        <div className="cart-product-info">
          <Link to={productUrl} className="cart-product-name">
            {item.product.name}
          </Link>
          {item.variantName && <div className="cart-product-variant">{item.variantName}</div>}
          {item.product.code && <div className="cart-product-code">Mã: {item.product.code}</div>}
          <button className="remove-link mobile-remove" type="button" onClick={() => onRemove(item.id)} disabled={busy}>
            Xóa
          </button>
        </div>
      </div>

      <div className="cart-price-cell">{formatCurrency(item.unitPrice)}</div>

      <div className="cart-quantity-cell">
        <div className="quantity-stepper">
          <button
            type="button"
            aria-label="Giảm số lượng"
            onClick={() => onQuantityChange(item.id, Math.max(1, item.quantity - 1))}
            disabled={busy || item.quantity <= 1}
          >
            <Icon name="minus" size={15} />
          </button>
          <input
            aria-label="Số lượng"
            value={item.quantity}
            inputMode="numeric"
            onChange={(event) => onQuantityChange(item.id, Number(event.target.value) || 1)}
            disabled={busy}
          />
          <button
            type="button"
            aria-label="Tăng số lượng"
            onClick={() => onQuantityChange(item.id, item.quantity + 1)}
            disabled={busy}
          >
            <Icon name="plus" size={15} />
          </button>
        </div>
      </div>

      <div className="cart-total-cell">{formatCurrency(item.lineTotal)}</div>

      <div className="cart-remove-cell">
        <button type="button" aria-label="Xóa sản phẩm" onClick={() => onRemove(item.id)} disabled={busy}>
          <Icon name="trash" size={18} />
        </button>
      </div>
    </div>
  );
}

export default CartItemRow;

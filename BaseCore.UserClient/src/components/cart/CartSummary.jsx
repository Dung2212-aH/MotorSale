import { formatCurrency } from '../../utils/formatters.js';

function CartSummary({
  subtotal,
  discount,
  voucherCode,
  onVoucherCodeChange,
  onApplyVoucher,
  voucherMessage,
  note,
  onNoteChange,
  onCheckout,
  busy,
  disabled,
}) {
  const total = Math.max(0, subtotal - discount);

  return (
    <aside className="cart-summary">
      <h2>Thông tin đơn hàng</h2>

      <div className="summary-line">
        <span>Tạm tính</span>
        <strong>{formatCurrency(subtotal)}</strong>
      </div>
      <div className="summary-line">
        <span>Giảm giá</span>
        <strong>{discount > 0 ? `-${formatCurrency(discount)}` : formatCurrency(0)}</strong>
      </div>
      <div className="summary-total">
        <span>Tổng tiền</span>
        <strong>{formatCurrency(total)}</strong>
      </div>

      <div className="voucher-form">
        <label htmlFor="voucher-code">Mã khuyến mãi</label>
        <div>
          <input
            id="voucher-code"
            value={voucherCode}
            onChange={(event) => onVoucherCodeChange(event.target.value)}
            placeholder="Nhập mã giảm giá"
          />
          <button type="button" onClick={onApplyVoucher} disabled={busy || !voucherCode.trim()}>
            Áp dụng
          </button>
        </div>
        {voucherMessage && <p>{voucherMessage}</p>}
      </div>

      <label className="cart-note" htmlFor="cart-note">
        Ghi chú đơn hàng
        <textarea
          id="cart-note"
          rows="4"
          value={note}
          onChange={(event) => onNoteChange(event.target.value)}
          placeholder="Ghi chú thêm cho đơn hàng"
        />
      </label>

      <button className="checkout-button" type="button" onClick={onCheckout} disabled={busy || disabled}>
        Thanh toán
      </button>
      <a className="continue-button" href="/san-pham">
        Tiếp tục mua hàng
      </a>
    </aside>
  );
}

export default CartSummary;

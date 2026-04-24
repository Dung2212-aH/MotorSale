function EmptyCart() {
  return (
    <div className="empty-cart">
      <div className="empty-cart-icon">0</div>
      <h2>Giỏ hàng của bạn đang trống</h2>
      <p>Hãy chọn mẫu xe yêu thích và quay lại đây để hoàn tất đơn hàng.</p>
      <a href="/san-pham">Tiếp tục mua hàng</a>
    </div>
  );
}

export default EmptyCart;

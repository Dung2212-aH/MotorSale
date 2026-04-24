import { useEffect, useMemo, useState } from 'react';
import Header from '../components/layout/Header.jsx';
import Breadcrumb from '../components/layout/Breadcrumb.jsx';
import Footer from '../components/layout/Footer.jsx';
import FloatingActions from '../components/layout/FloatingActions.jsx';
import CartItemRow from '../components/cart/CartItemRow.jsx';
import CartSummary from '../components/cart/CartSummary.jsx';
import DeliveryTimeSelector from '../components/cart/DeliveryTimeSelector.jsx';
import CompanyInvoiceForm from '../components/cart/CompanyInvoiceForm.jsx';
import EmptyCart from '../components/cart/EmptyCart.jsx';
import BestSellerSection from '../components/cart/BestSellerSection.jsx';
import { cartService } from '../services/cartService.js';
import { productService } from '../services/productService.js';
import { getErrorMessage } from '../utils/formatters.js';

const invoiceDefaults = {
  companyName: '',
  taxCode: '',
  companyAddress: '',
  invoiceEmail: '',
};

function buildCheckoutPayload({ cart, note, deliveryTime, invoiceEnabled, invoiceValues, discount }) {
  const invoiceNote = invoiceEnabled
    ? [
        'Xuất hóa đơn công ty:',
        `Tên công ty: ${invoiceValues.companyName}`,
        `Mã số thuế: ${invoiceValues.taxCode}`,
        `Địa chỉ công ty: ${invoiceValues.companyAddress}`,
        `Email nhận hóa đơn: ${invoiceValues.invoiceEmail}`,
      ].join('\n')
    : '';

  return {
    shippingFullName: '',
    shippingPhoneNumber: '',
    shippingEmail: invoiceEnabled ? invoiceValues.invoiceEmail : '',
    shippingAddressLine: '',
    shippingProvince: '',
    discountAmount: discount,
    shippingFee: 0,
    note: [note, `Thời gian giao hàng: ${deliveryTime}`, invoiceNote].filter(Boolean).join('\n\n'),
    items: cart.items.map((item) => ({
      productId: item.productId,
      productVariantId: item.productVariantId,
      quantity: item.quantity,
    })),
  };
}

function CartPage() {
  const [cart, setCart] = useState({ items: [], subtotal: 0, totalItems: 0 });
  const [cartLoading, setCartLoading] = useState(true);
  const [cartError, setCartError] = useState('');
  const [rowBusyId, setRowBusyId] = useState(null);
  const [deliveryTime, setDeliveryTime] = useState('08h00 - 12h00');
  const [invoiceEnabled, setInvoiceEnabled] = useState(false);
  const [invoiceValues, setInvoiceValues] = useState(invoiceDefaults);
  const [note, setNote] = useState('');
  const [voucherCode, setVoucherCode] = useState('');
  const [voucher, setVoucher] = useState(null);
  const [voucherMessage, setVoucherMessage] = useState('');
  const [bestSellers, setBestSellers] = useState([]);
  const [bestSellerLoading, setBestSellerLoading] = useState(true);
  const [bestSellerError, setBestSellerError] = useState('');
  const [checkoutMessage, setCheckoutMessage] = useState('');

  const discount = useMemo(() => {
    if (!voucher) {
      return 0;
    }

    const minOrderValue = Number(voucher.minOrderValue || 0);
    if (cart.subtotal < minOrderValue) {
      return 0;
    }

    if (voucher.discountType === 'Percent') {
      const rawDiscount = cart.subtotal * (Number(voucher.discountValue || 0) / 100);
      const maxDiscount = Number(voucher.maxDiscountAmount || rawDiscount);
      return Math.min(rawDiscount, maxDiscount);
    }

    return Number(voucher.discountValue || voucher.value || 0);
  }, [cart.subtotal, voucher]);

  const refreshCart = async () => {
    setCartLoading(true);
    setCartError('');

    try {
      const data = await cartService.getCart();
      setCart(data);
    } catch (error) {
      setCartError(getErrorMessage(error, 'Không thể tải giỏ hàng. Vui lòng đăng nhập hoặc thử lại sau.'));
      setCart({ items: [], subtotal: 0, totalItems: 0 });
    } finally {
      setCartLoading(false);
    }
  };

  useEffect(() => {
    refreshCart();
  }, []);

  useEffect(() => {
    let mounted = true;

    async function loadBestSellers() {
      setBestSellerLoading(true);
      setBestSellerError('');

      try {
        const products = await productService.getBestSellers();
        if (mounted) {
          setBestSellers(products);
        }
      } catch (error) {
        if (mounted) {
          setBestSellerError(getErrorMessage(error, 'Không thể tải sản phẩm bán chạy.'));
        }
      } finally {
        if (mounted) {
          setBestSellerLoading(false);
        }
      }
    }

    loadBestSellers();

    return () => {
      mounted = false;
    };
  }, []);

  const handleQuantityChange = async (itemId, quantity) => {
    const nextQuantity = Math.max(1, Math.min(99, Number(quantity) || 1));
    setRowBusyId(itemId);

    try {
      await cartService.updateQuantity(itemId, nextQuantity);
      await refreshCart();
    } catch (error) {
      setCartError(getErrorMessage(error, 'Không thể cập nhật số lượng.'));
    } finally {
      setRowBusyId(null);
    }
  };

  const handleRemoveItem = async (itemId) => {
    setRowBusyId(itemId);

    try {
      await cartService.removeItem(itemId);
      await refreshCart();
    } catch (error) {
      setCartError(getErrorMessage(error, 'Không thể xóa sản phẩm.'));
    } finally {
      setRowBusyId(null);
    }
  };

  const handleApplyVoucher = async () => {
    setVoucherMessage('');

    try {
      const data = await cartService.applyVoucher(voucherCode.trim());
      setVoucher(data);
      setVoucherMessage('Mã khuyến mãi đã được áp dụng.');
    } catch (error) {
      setVoucher(null);
      setVoucherMessage(getErrorMessage(error, 'Mã khuyến mãi không hợp lệ.'));
    }
  };

  const handleCheckout = async () => {
    setCheckoutMessage('');

    try {
      await cartService.checkout(
        buildCheckoutPayload({
          cart,
          note,
          deliveryTime,
          invoiceEnabled,
          invoiceValues,
          discount,
        }),
      );
      setCheckoutMessage('Đơn hàng đã được gửi. Vui lòng tiếp tục bổ sung thông tin giao hàng ở luồng checkout.');
      await refreshCart();
    } catch (error) {
      setCheckoutMessage(getErrorMessage(error, 'Chưa thể tạo đơn hàng. Vui lòng kiểm tra thông tin checkout.'));
    }
  };

  return (
    <div className="page-shell">
      <Header cartCount={cart.totalItems} />
      <Breadcrumb />

      <main className="cart-page">
        <div className="container">
          <h1>Giỏ hàng của bạn</h1>

          {cartError && <div className="page-error">{cartError}</div>}
          {checkoutMessage && <div className="page-message">{checkoutMessage}</div>}

          <div className="cart-layout">
            <section className="cart-main">
              {cartLoading && (
                <div className="cart-loading">
                  <span />
                  <span />
                  <span />
                </div>
              )}

              {!cartLoading && cart.items.length === 0 && <EmptyCart />}

              {!cartLoading && cart.items.length > 0 && (
                <div className="cart-table">
                  <div className="cart-table-head">
                    <span>Sản phẩm</span>
                    <span>Đơn giá</span>
                    <span>Số lượng</span>
                    <span>Thành tiền</span>
                    <span />
                  </div>
                  {cart.items.map((item) => (
                    <CartItemRow
                      key={item.id}
                      item={item}
                      busy={rowBusyId === item.id}
                      onQuantityChange={handleQuantityChange}
                      onRemove={handleRemoveItem}
                    />
                  ))}
                </div>
              )}

              <DeliveryTimeSelector value={deliveryTime} onChange={setDeliveryTime} />
              <CompanyInvoiceForm
                enabled={invoiceEnabled}
                onEnabledChange={setInvoiceEnabled}
                values={invoiceValues}
                onChange={setInvoiceValues}
              />
            </section>

            <CartSummary
              subtotal={cart.subtotal}
              discount={discount}
              voucherCode={voucherCode}
              onVoucherCodeChange={setVoucherCode}
              onApplyVoucher={handleApplyVoucher}
              voucherMessage={voucherMessage}
              note={note}
              onNoteChange={setNote}
              onCheckout={handleCheckout}
              busy={cartLoading || Boolean(rowBusyId)}
              disabled={cart.items.length === 0}
            />
          </div>
        </div>
      </main>

      <BestSellerSection products={bestSellers} loading={bestSellerLoading} error={bestSellerError} />
      <Footer cartCount={cart.totalItems} />
      <FloatingActions />
    </div>
  );
}

export default CartPage;

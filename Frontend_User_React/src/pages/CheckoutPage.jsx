import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '../api/authApi.js';
import { orderApi } from '../api/orderApi.js';
import Breadcrumb from '../components/Breadcrumb.jsx';
import LoadingState from '../components/LoadingState.jsx';
import { useCart } from '../contexts/CartContext.jsx';
import { formatCurrency } from '../utils/formatters.js';

const RECEIVING_METHODS = [
  { value: 'Delivery', label: 'Giao hàng tận nơi' },
  { value: 'Pickup', label: 'Nhận tại showroom' },
];

const ORDER_TYPES = [
  { value: 'FullPayment', label: 'Thanh toán toàn bộ' },
  { value: 'Deposit', label: 'Đặt cọc trước' },
];

const PAYMENT_METHODS = [
  { value: 'COD', label: 'Thanh toán khi nhận hàng', icon: '💵', desc: 'Thanh toán bằng tiền mặt khi nhận hàng' },
  { value: 'BankTransfer', label: 'Chuyển khoản ngân hàng', icon: '🏦', desc: 'Chuyển khoản qua tài khoản ngân hàng' },
  { value: 'Momo', label: 'Ví MoMo', icon: '📱', desc: 'Thanh toán qua ví điện tử MoMo' },
  { value: 'VNPay', label: 'VNPay', icon: '💳', desc: 'Thanh toán qua cổng VNPay' },
];

function CheckoutPage() {
  const navigate = useNavigate();
  const { cart, refreshCart, clearCart } = useCart();
  const isAuthenticated = Boolean(authApi.getToken());
  const items = cart?.items || [];

  const [form, setForm] = useState({
    shippingFullName: '',
    shippingPhoneNumber: '',
    shippingEmail: '',
    shippingAddressLine: '',
    shippingWard: '',
    shippingDistrict: '',
    shippingProvince: '',
    receivingMethod: 'Delivery',
    orderType: 'FullPayment',
    paymentMethod: 'COD',
    depositAmount: '',
    note: '',
    fulfillmentNote: '',
    pickupAppointmentAt: '',
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login?redirect=/checkout', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    if (isAuthenticated && !items.length) {
      refreshCart().catch(() => {});
    }
  }, [isAuthenticated]);

  function handleChange(e) {
    const { name, value } = e.target;
    
    setForm((prev) => {
      const nextForm = { ...prev, [name]: value };
      
      // If switching to Deposit, COD is not allowed. Fall back to BankTransfer.
      if (name === 'orderType' && value === 'Deposit' && prev.paymentMethod === 'COD') {
        nextForm.paymentMethod = 'BankTransfer';
      }
      
      return nextForm;
    });
    
    setFieldErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function validate() {
    const errors = {};
    if (!form.shippingFullName.trim()) errors.shippingFullName = 'Vui lòng nhập họ tên';
    if (!form.shippingPhoneNumber.trim()) errors.shippingPhoneNumber = 'Vui lòng nhập số điện thoại';
    else if (!/^0\d{9,10}$/.test(form.shippingPhoneNumber.trim()))
      errors.shippingPhoneNumber = 'Số điện thoại không hợp lệ';

    if (form.receivingMethod === 'Delivery') {
      if (!form.shippingAddressLine.trim()) errors.shippingAddressLine = 'Vui lòng nhập địa chỉ';
      if (!form.shippingProvince.trim()) errors.shippingProvince = 'Vui lòng nhập tỉnh/thành phố';
    }

    if (form.orderType === 'Deposit') {
      const deposit = Number(form.depositAmount);
      if (!deposit || deposit <= 0) errors.depositAmount = 'Số tiền đặt cọc phải lớn hơn 0';
      else if (deposit >= subtotal) errors.depositAmount = 'Số tiền đặt cọc phải nhỏ hơn tổng tiền';
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');

    if (!validate()) return;
    if (!items.length) {
      setError('Giỏ hàng trống. Vui lòng thêm sản phẩm.');
      return;
    }

    setSubmitting(true);
    try {
      const payload = {
        shippingFullName: form.shippingFullName.trim(),
        shippingPhoneNumber: form.shippingPhoneNumber.trim(),
        shippingEmail: form.shippingEmail.trim() || null,
        shippingAddressLine: form.shippingAddressLine.trim(),
        shippingWard: form.shippingWard.trim() || null,
        shippingDistrict: form.shippingDistrict.trim() || null,
        shippingProvince: form.shippingProvince.trim(),
        receivingMethod: form.receivingMethod,
        orderType: form.orderType,
        depositAmount: form.orderType === 'Deposit' ? Number(form.depositAmount) : 0,
        note: form.note.trim() || null,
        fulfillmentNote: form.fulfillmentNote.trim() || null,
        pickupAppointmentAt: form.pickupAppointmentAt || null,
        paymentMethod: form.paymentMethod,
        cartId: null,
        discountAmount: 0,
        shippingFee: 0,
        holdMinutes: 15,
        items: items.map((item) => ({
          productId: item.productId || item.product?.id,
          productVariantId: item.productVariantId || item.variant?.id || null,
          quantity: item.quantity || 1,
        })),
      };

      const res = await orderApi.createOrder(payload);
      clearCart();
      navigate(`/checkout/success?orderId=${res.order.id}`, { replace: true });
    } catch (err) {
      const msg = err?.response?.data?.message || err?.message || 'Đặt hàng thất bại. Vui lòng thử lại.';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  }

  const subtotal = items.reduce((sum, item) => {
    const quantity = item.quantity || 1;
    const price = item.unitPrice || item.product?.salePrice || item.product?.basePrice || 0;
    return sum + price * quantity;
  }, 0);

  if (!isAuthenticated) return null;

  return (
    <>
      <Breadcrumb
        items={[{ label: 'Giỏ hàng', to: '/cart' }, { label: 'Thanh toán' }]}
      />

      <section className="bg-[linear-gradient(180deg,#f5f6f8_0%,#ffffff_26%)] px-4 py-10">
        <div className="mx-auto grid w-full max-w-[1200px] gap-8 lg:grid-cols-[minmax(0,1fr)_380px]">
          {/* ── Left: Shipping Form ── */}
          <form onSubmit={handleSubmit} className="space-y-5" id="checkout-form">
            <div className="rounded-[30px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <div className="text-[12px] font-extrabold uppercase tracking-[0.18em] text-zinc-400">
                Thanh toán
              </div>
              <h1 className="mt-2 text-[28px] font-black text-zinc-950 sm:text-[34px]">
                Thông tin giao hàng
              </h1>
            </div>

            {error && (
              <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
                {error}
              </div>
            )}

            {/* Contact Info */}
            <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <h2 className="text-[18px] font-black text-zinc-950">Thông tin liên hệ</h2>

              <div className="mt-5 grid gap-4 sm:grid-cols-2">
                <Field
                  label="Họ và tên *"
                  id="shippingFullName"
                  name="shippingFullName"
                  value={form.shippingFullName}
                  onChange={handleChange}
                  error={fieldErrors.shippingFullName}
                  placeholder="Nguyễn Văn A"
                />
                <Field
                  label="Số điện thoại *"
                  id="shippingPhoneNumber"
                  name="shippingPhoneNumber"
                  value={form.shippingPhoneNumber}
                  onChange={handleChange}
                  error={fieldErrors.shippingPhoneNumber}
                  placeholder="0912345678"
                  type="tel"
                />
              </div>

              <div className="mt-4">
                <Field
                  label="Email"
                  id="shippingEmail"
                  name="shippingEmail"
                  value={form.shippingEmail}
                  onChange={handleChange}
                  placeholder="email@example.com"
                  type="email"
                />
              </div>
            </div>

            {/* Receiving Method */}
            <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <h2 className="text-[18px] font-black text-zinc-950">Phương thức nhận hàng</h2>

              <div className="mt-4 flex flex-wrap gap-3">
                {RECEIVING_METHODS.map((method) => (
                  <label
                    key={method.value}
                    className={`flex cursor-pointer items-center gap-2 rounded-full border px-5 py-2.5 text-sm font-bold transition ${
                      form.receivingMethod === method.value
                        ? 'border-[#d71920] bg-red-50 text-[#d71920]'
                        : 'border-zinc-200 bg-zinc-50 text-zinc-600 hover:border-zinc-300'
                    }`}
                  >
                    <input
                      type="radio"
                      name="receivingMethod"
                      value={method.value}
                      checked={form.receivingMethod === method.value}
                      onChange={handleChange}
                      className="sr-only"
                    />
                    <span
                      className={`flex h-4 w-4 items-center justify-center rounded-full border-2 ${
                        form.receivingMethod === method.value
                          ? 'border-[#d71920]'
                          : 'border-zinc-300'
                      }`}
                    >
                      {form.receivingMethod === method.value && (
                        <span className="h-2 w-2 rounded-full bg-[#d71920]" />
                      )}
                    </span>
                    {method.label}
                  </label>
                ))}
              </div>
            </div>

            {/* Address (shown for Delivery) */}
            {form.receivingMethod === 'Delivery' && (
              <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
                <h2 className="text-[18px] font-black text-zinc-950">Địa chỉ giao hàng</h2>

                <div className="mt-5">
                  <Field
                    label="Địa chỉ *"
                    id="shippingAddressLine"
                    name="shippingAddressLine"
                    value={form.shippingAddressLine}
                    onChange={handleChange}
                    error={fieldErrors.shippingAddressLine}
                    placeholder="Số nhà, tên đường..."
                  />
                </div>

                <div className="mt-4 grid gap-4 sm:grid-cols-2">
                  <Field
                    label="Tỉnh / Thành phố *"
                    id="shippingProvince"
                    name="shippingProvince"
                    value={form.shippingProvince}
                    onChange={handleChange}
                    error={fieldErrors.shippingProvince}
                    placeholder="TP. Hồ Chí Minh"
                  />
                  <Field
                    label="Phường / Xã"
                    id="shippingWard"
                    name="shippingWard"
                    value={form.shippingWard}
                    onChange={handleChange}
                    placeholder="Phường Tân Phú"
                  />
                </div>
              </div>
            )}

            {/* Pickup (shown for Pickup) */}
            {form.receivingMethod === 'Pickup' && (
              <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
                <h2 className="text-[18px] font-black text-zinc-950">Hẹn ngày nhận xe</h2>
                <div className="mt-5">
                  <Field
                    label="Ngày hẹn nhận"
                    id="pickupAppointmentAt"
                    name="pickupAppointmentAt"
                    value={form.pickupAppointmentAt}
                    onChange={handleChange}
                    type="datetime-local"
                  />
                </div>
                <div className="mt-4">
                  <Field
                    label="Ghi chú giao nhận"
                    id="fulfillmentNote"
                    name="fulfillmentNote"
                    value={form.fulfillmentNote}
                    onChange={handleChange}
                    placeholder="Ghi chú cho showroom..."
                    multiline
                  />
                </div>
              </div>
            )}

            {/* Order Type */}
            <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <h2 className="text-[18px] font-black text-zinc-950">Hình thức thanh toán</h2>

              <div className="mt-4 flex flex-wrap gap-3">
                {ORDER_TYPES.map((type) => (
                  <label
                    key={type.value}
                    className={`flex cursor-pointer items-center gap-2 rounded-full border px-5 py-2.5 text-sm font-bold transition ${
                      form.orderType === type.value
                        ? 'border-[#d71920] bg-red-50 text-[#d71920]'
                        : 'border-zinc-200 bg-zinc-50 text-zinc-600 hover:border-zinc-300'
                    }`}
                  >
                    <input
                      type="radio"
                      name="orderType"
                      value={type.value}
                      checked={form.orderType === type.value}
                      onChange={handleChange}
                      className="sr-only"
                    />
                    <span
                      className={`flex h-4 w-4 items-center justify-center rounded-full border-2 ${
                        form.orderType === type.value
                          ? 'border-[#d71920]'
                          : 'border-zinc-300'
                      }`}
                    >
                      {form.orderType === type.value && (
                        <span className="h-2 w-2 rounded-full bg-[#d71920]" />
                      )}
                    </span>
                    {type.label}
                  </label>
                ))}
              </div>

              {form.orderType === 'Deposit' && (
                <div className="mt-4">
                  <Field
                    label="Số tiền đặt cọc *"
                    id="depositAmount"
                    name="depositAmount"
                    value={form.depositAmount}
                    onChange={handleChange}
                    error={fieldErrors.depositAmount}
                    placeholder="5000000"
                    type="number"
                  />
                </div>
              )}
            </div>

            {/* Payment Method */}
            <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <h2 className="text-[18px] font-black text-zinc-950">Phương thức thanh toán</h2>

              <div className="mt-4 space-y-3">
                {PAYMENT_METHODS.filter(method => !(form.orderType === 'Deposit' && method.value === 'COD')).map((method) => (
                  <label
                    key={method.value}
                    className={`flex cursor-pointer items-center gap-4 rounded-2xl border p-4 transition ${
                      form.paymentMethod === method.value
                        ? 'border-[#d71920] bg-red-50/50 shadow-sm'
                        : 'border-zinc-200 bg-zinc-50 hover:border-zinc-300'
                    }`}
                  >
                    <input
                      type="radio"
                      name="paymentMethod"
                      value={method.value}
                      checked={form.paymentMethod === method.value}
                      onChange={handleChange}
                      className="sr-only"
                    />
                    <span
                      className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2 ${
                        form.paymentMethod === method.value
                          ? 'border-[#d71920]'
                          : 'border-zinc-300'
                      }`}
                    >
                      {form.paymentMethod === method.value && (
                        <span className="h-2.5 w-2.5 rounded-full bg-[#d71920]" />
                      )}
                    </span>
                    <span className="text-2xl leading-none">{method.icon}</span>
                    <div className="flex-1 min-w-0">
                      <div className={`text-sm font-bold ${
                        form.paymentMethod === method.value ? 'text-[#d71920]' : 'text-zinc-900'
                      }`}>
                        {method.label}
                      </div>
                      <div className="mt-0.5 text-xs text-zinc-500">{method.desc}</div>
                    </div>
                  </label>
                ))}
              </div>
            </div>

            {/* Note */}
            <div className="rounded-[28px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <h2 className="text-[18px] font-black text-zinc-950">Ghi chú đơn hàng</h2>
              <div className="mt-4">
                <Field
                  id="note"
                  name="note"
                  value={form.note}
                  onChange={handleChange}
                  placeholder="Ghi chú thêm cho đơn hàng..."
                  multiline
                />
              </div>
            </div>
          </form>

          {/* ── Right: Order Summary ── */}
          <aside className="space-y-5 lg:sticky lg:top-28 lg:self-start">
            <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-[0_18px_50px_rgba(15,23,42,0.08)]">
              <h2 className="text-[22px] font-black text-zinc-950">Đơn hàng của bạn</h2>

              <div className="mt-5 max-h-[340px] space-y-3 overflow-y-auto pr-1">
                {items.map((item) => {
                  const name = item.product?.name || item.productName || 'Sản phẩm';
                  const variantName = item.variant?.variantName || item.variantName || '';
                  const price = item.unitPrice || item.product?.salePrice || item.product?.basePrice || 0;
                  const qty = item.quantity || 1;
                  return (
                    <div
                      key={item.id}
                      className="flex items-start gap-3 rounded-2xl bg-zinc-50 p-3"
                    >
                      <div className="flex-1 min-w-0">
                        <div className="truncate text-sm font-bold text-zinc-900">{name}</div>
                        {variantName && (
                          <div className="mt-0.5 truncate text-xs text-zinc-500">{variantName}</div>
                        )}
                        <div className="mt-1 text-xs text-zinc-500">SL: {qty}</div>
                      </div>
                      <div className="whitespace-nowrap text-sm font-bold text-zinc-900">
                        {formatCurrency(price * qty)}
                      </div>
                    </div>
                  );
                })}

                {!items.length && (
                  <div className="py-6 text-center text-sm text-zinc-400">
                    Giỏ hàng trống
                  </div>
                )}
              </div>

              <div className="mt-5 space-y-3 border-t border-zinc-200 pt-4">
                <div className="flex items-center justify-between text-sm text-zinc-600">
                  <span>Tạm tính ({items.length} sản phẩm)</span>
                  <strong className="font-bold text-zinc-950">{formatCurrency(subtotal)}</strong>
                </div>
                <div className="flex items-center justify-between text-sm text-zinc-600">
                  <span>Phí giao hàng</span>
                  <strong className="font-bold text-zinc-950">{formatCurrency(0)}</strong>
                </div>

                {form.orderType === 'Deposit' && Number(form.depositAmount) > 0 && (
                  <div className="flex items-center justify-between text-sm text-amber-600">
                    <span>Đặt cọc</span>
                    <strong className="font-bold">{formatCurrency(Number(form.depositAmount))}</strong>
                  </div>
                )}

                <div className="flex items-center justify-between pt-2 text-[#d71920]">
                  <span className="text-sm font-extrabold uppercase tracking-[0.08em]">Tổng cộng</span>
                  <strong className="text-[24px] font-black">{formatCurrency(subtotal)}</strong>
                </div>
              </div>

              <button
                type="submit"
                form="checkout-form"
                disabled={submitting || !items.length}
                className="mt-6 inline-flex min-h-12 w-full items-center justify-center gap-2 rounded-full bg-[#d71920] px-5 text-sm font-extrabold uppercase tracking-[0.08em] text-white transition hover:bg-[#b61016] disabled:cursor-not-allowed disabled:bg-zinc-300"
              >
                {submitting ? (
                  <>
                    <svg
                      className="h-4 w-4 animate-spin"
                      viewBox="0 0 24 24"
                      fill="none"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      />
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                      />
                    </svg>
                    Đang xử lý...
                  </>
                ) : (
                  'Đặt hàng'
                )}
              </button>

              <Link
                to="/cart"
                className="mt-3 flex items-center justify-center gap-1 text-sm font-bold text-zinc-500 transition hover:text-zinc-900"
              >
                ← Quay lại giỏ hàng
              </Link>
            </div>
          </aside>
        </div>
      </section>
    </>
  );
}

/* ── Reusable Field Component ── */
function Field({ label, id, name, value, onChange, error, placeholder, type = 'text', multiline }) {
  const baseClass =
    'w-full rounded-xl border bg-zinc-50 px-4 py-3 text-sm text-zinc-900 outline-none transition placeholder:text-zinc-400 focus:border-[#d71920] focus:ring-2 focus:ring-[#d71920]/20';
  const errorClass = error ? 'border-red-300' : 'border-zinc-200';

  return (
    <div>
      {label && (
        <label htmlFor={id} className="mb-1.5 block text-sm font-bold text-zinc-700">
          {label}
        </label>
      )}
      {multiline ? (
        <textarea
          id={id}
          name={name}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          rows={3}
          className={`${baseClass} ${errorClass} resize-none`}
        />
      ) : (
        <input
          id={id}
          name={name}
          type={type}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          className={`${baseClass} ${errorClass}`}
        />
      )}
      {error && <p className="mt-1 text-xs font-medium text-red-500">{error}</p>}
    </div>
  );
}

export default CheckoutPage;

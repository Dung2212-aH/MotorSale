import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { orderApi } from '../api/orderApi.js';
import { paymentApi } from '../api/paymentApi.js';
import Breadcrumb from '../components/Breadcrumb.jsx';
import { formatCurrency } from '../utils/formatters.js';

const PAYMENT_METHODS = [
  { value: 'COD', label: 'Thanh toán khi nhận hàng', icon: '💵', desc: 'Thanh toán bằng tiền mặt khi nhận hàng' },
  { value: 'BankTransfer', label: 'Chuyển khoản ngân hàng', icon: '🏦', desc: 'Chuyển khoản qua tài khoản ngân hàng' },
  { value: 'Momo', label: 'Ví MoMo', icon: '📱', desc: 'Thanh toán qua ví điện tử MoMo' },
  { value: 'VNPay', label: 'VNPay', icon: '💳', desc: 'Thanh toán qua cổng VNPay' },
];

function OrderDetailPage() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [details, setDetails] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Payment state
  const [paymentMethod, setPaymentMethod] = useState('COD');
  const [submittingPayment, setSubmittingPayment] = useState(false);
  const [paymentSuccess, setPaymentSuccess] = useState(false);
  const [paymentError, setPaymentError] = useState(null);

  useEffect(() => {
    async function fetchOrder() {
      try {
        const res = await orderApi.getOrderById(id);
        setOrder(res.order);
        setDetails(res.details || []);
      } catch (err) {
        setError(err.message || 'Không thể tải thông tin đơn hàng');
      } finally {
        setLoading(false);
      }
    }
    fetchOrder();
  }, [id, paymentSuccess]);

  const handlePayment = async () => {
    if (!order || order.remainingAmount <= 0) return;
    
    setSubmittingPayment(true);
    setPaymentError(null);
    
    try {
      await paymentApi.createPayment({
        orderId: order.id,
        paymentType: 'Remaining',
        amount: order.remainingAmount,
        paymentMethod: paymentMethod,
        transactionRef: null, // Depending on gateway, this might be set later
        transferContent: `THANHTOAN ${order.orderCode}`,
      });
      
      setPaymentSuccess(true);
      // Wait a moment before refreshing the order
      setTimeout(() => {
        setPaymentSuccess(false);
      }, 3000);
    } catch (err) {
      setPaymentError(err.message || 'Thanh toán thất bại. Vui lòng thử lại.');
    } finally {
      setSubmittingPayment(false);
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-[#d71920] border-t-transparent"></div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="flex min-h-[50vh] flex-col items-center justify-center px-4 text-center">
        <div className="text-xl font-bold text-zinc-900">{error || 'Không tìm thấy đơn hàng'}</div>
        <Link to="/" className="mt-4 text-[#d71920] hover:underline">
          Quay lại trang chủ
        </Link>
      </div>
    );
  }

  return (
    <>
      <Breadcrumb items={[{ label: 'Chi tiết đơn hàng' }]} />

      <section className="bg-zinc-50 px-4 py-8 md:py-12">
        <div className="mx-auto max-w-5xl">
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            {/* Main Order Content */}
            <div className="lg:col-span-2 space-y-6">
              <div className="rounded-[28px] border border-zinc-200 bg-white p-6 md:p-8 shadow-sm">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-zinc-100 pb-6">
                  <div>
                    <h1 className="text-2xl font-black text-zinc-950">Mã đơn: {order.orderCode}</h1>
                    <p className="mt-1 text-sm text-zinc-500">
                      Ngày đặt: {new Date(order.createdAt).toLocaleString('vi-VN')}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="inline-flex items-center rounded-full bg-zinc-100 px-3 py-1 text-sm font-bold text-zinc-800">
                      {order.orderStatus}
                    </span>
                    <span className={`inline-flex items-center rounded-full px-3 py-1 text-sm font-bold ${
                      order.paymentStatus === 'Paid' ? 'bg-green-100 text-green-700' : 
                      order.paymentStatus === 'Partial' ? 'bg-orange-100 text-orange-700' : 'bg-red-100 text-red-700'
                    }`}>
                      {order.paymentStatus === 'Paid' ? 'Đã thanh toán' : 
                       order.paymentStatus === 'Partial' ? 'Đã cọc' : 'Chưa thanh toán'}
                    </span>
                  </div>
                </div>

                <div className="mt-6">
                  <h3 className="mb-4 text-lg font-bold text-zinc-900">Sản phẩm ({details.length})</h3>
                  <div className="divide-y divide-zinc-100 border-t border-zinc-100">
                    {details.map((item, index) => (
                      <div key={index} className="flex items-start justify-between py-4">
                        <div className="flex items-start gap-4">
                          <div className="flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-xl bg-zinc-100">
                            <span className="text-2xl">🏍️</span>
                          </div>
                          <div>
                            <h4 className="font-bold text-zinc-900">{item.productNameSnapshot}</h4>
                            <p className="mt-1 text-sm text-zinc-500">
                              Mã SP: {item.skuSnapshot} <br/>
                              Số lượng: {item.quantity}
                            </p>
                          </div>
                        </div>
                        <div className="text-right font-bold text-zinc-900">
                          {formatCurrency(item.lineTotal)}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>

            {/* Sidebar Summary & Payment */}
            <div className="space-y-6">
              <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-sm">
                <h3 className="mb-4 text-lg font-bold text-zinc-900">Tổng quan</h3>
                <div className="space-y-3 text-sm">
                  <div className="flex justify-between text-zinc-600">
                    <span>Tạm tính</span>
                    <span>{formatCurrency(order.subtotal)}</span>
                  </div>
                  {order.discountAmount > 0 && (
                    <div className="flex justify-between text-green-600">
                      <span>Giảm giá</span>
                      <span>-{formatCurrency(order.discountAmount)}</span>
                    </div>
                  )}
                  {order.shippingFee > 0 && (
                    <div className="flex justify-between text-zinc-600">
                      <span>Phí vận chuyển</span>
                      <span>{formatCurrency(order.shippingFee)}</span>
                    </div>
                  )}
                  <div className="my-4 border-t border-zinc-100"></div>
                  <div className="flex justify-between text-base font-bold text-zinc-900">
                    <span>Tổng cộng</span>
                    <span>{formatCurrency(order.totalAmount)}</span>
                  </div>
                  <div className="flex justify-between text-zinc-600">
                    <span>Đã thanh toán (Cọc)</span>
                    <span>{formatCurrency(order.depositAmount || 0)}</span>
                  </div>
                  <div className="my-4 border-t border-zinc-100"></div>
                  <div className="flex justify-between text-lg font-black text-[#d71920]">
                    <span>Còn lại cần thanh toán</span>
                    <span>{formatCurrency(order.remainingAmount || 0)}</span>
                  </div>
                </div>
              </div>

              {/* Remaining Payment Section */}
              {order.remainingAmount > 0 && order.orderStatus !== 'Cancelled' && (
                <div className="rounded-[28px] border border-zinc-200 bg-white p-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
                  <h3 className="mb-4 text-lg font-bold text-zinc-900">Thanh toán phần còn lại</h3>
                  
                  {paymentSuccess && (
                    <div className="mb-4 rounded-xl bg-green-50 p-4 text-sm text-green-700 font-medium">
                      Giao dịch đang được xử lý. Xin cảm ơn!
                    </div>
                  )}

                  {paymentError && (
                    <div className="mb-4 rounded-xl bg-red-50 p-4 text-sm text-red-600 font-medium">
                      {paymentError}
                    </div>
                  )}

                  <div className="space-y-3">
                    {PAYMENT_METHODS.map((method) => (
                      <label
                        key={method.value}
                        className={`flex cursor-pointer items-center gap-3 rounded-2xl border p-3 transition ${
                          paymentMethod === method.value
                            ? 'border-[#d71920] bg-red-50/50'
                            : 'border-zinc-200 bg-zinc-50 hover:border-zinc-300'
                        }`}
                      >
                        <input
                          type="radio"
                          name="paymentMethod"
                          value={method.value}
                          checked={paymentMethod === method.value}
                          onChange={() => setPaymentMethod(method.value)}
                          className="sr-only"
                        />
                        <span
                          className={`flex h-4 w-4 shrink-0 items-center justify-center rounded-full border-2 ${
                            paymentMethod === method.value
                              ? 'border-[#d71920]'
                              : 'border-zinc-300'
                          }`}
                        >
                          {paymentMethod === method.value && (
                            <span className="h-2 w-2 rounded-full bg-[#d71920]" />
                          )}
                        </span>
                        <span className="text-xl leading-none">{method.icon}</span>
                        <div className="flex-1 min-w-0">
                          <div className={`text-sm font-bold ${
                            paymentMethod === method.value ? 'text-[#d71920]' : 'text-zinc-900'
                          }`}>
                            {method.label}
                          </div>
                        </div>
                      </label>
                    ))}
                  </div>

                  <button
                    onClick={handlePayment}
                    disabled={submittingPayment || paymentSuccess}
                    className="mt-6 flex w-full items-center justify-center rounded-full bg-[#d71920] px-6 py-4 text-sm font-extrabold uppercase tracking-[0.08em] text-white transition hover:bg-[#b61016] disabled:opacity-50"
                  >
                    {submittingPayment ? 'Đang xử lý...' : `Thanh toán ${formatCurrency(order.remainingAmount)}`}
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </section>
    </>
  );
}

export default OrderDetailPage;

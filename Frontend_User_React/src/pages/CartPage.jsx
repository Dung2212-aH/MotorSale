import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { authApi } from '../api/authApi.js';
import { cartApi } from '../api/cartApi.js';
import Breadcrumb from '../components/Breadcrumb.jsx';
import CartItemRow from '../components/CartItemRow.jsx';
import CartSummary from '../components/CartSummary.jsx';
import EmptyCart from '../components/EmptyCart.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';

function CartPage() {
  const [cart, setCart] = useState({ items: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [checkoutNotice, setCheckoutNotice] = useState('');
  const isAuthenticated = Boolean(authApi.getToken());

  async function loadCart() {
    if (!isAuthenticated) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      setCart(await cartApi.getCart());
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadCart();
  }, [isAuthenticated]);

  async function updateQuantity(itemId, quantity) {
    await cartApi.updateItem(itemId, quantity);
    await loadCart();
  }

  async function removeItem(itemId) {
    await cartApi.removeItem(itemId);
    await loadCart();
  }

  function checkout() {
    setCheckoutNotice('Checkout flow đang chờ màn hình nhập thông tin giao hàng.');
  }

  const items = cart?.items || [];

  return (
    <>
      <Breadcrumb items={[{ label: 'Giỏ hàng' }]} />

      <section className="bg-[linear-gradient(180deg,#f5f6f8_0%,#ffffff_26%)] px-4 py-10">
        <div className="mx-auto grid w-full max-w-[1200px] gap-8 xl:grid-cols-[minmax(0,1fr)_360px]">
          <div className="space-y-5">
            <div className="rounded-[30px] border border-zinc-200 bg-white px-6 py-6 shadow-[0_18px_50px_rgba(15,23,42,0.07)]">
              <div className="text-[12px] font-extrabold uppercase tracking-[0.18em] text-zinc-400">Giỏ hàng</div>
              <h1 className="mt-2 text-[28px] font-black text-zinc-950 sm:text-[34px]">Sản phẩm đã chọn</h1>
            </div>

            {!isAuthenticated && (
              <div className="rounded-[28px] border border-dashed border-zinc-300 bg-zinc-50 px-6 py-10 text-center">
                <h2 className="text-[24px] font-black text-zinc-950">Bạn cần đăng nhập để xem giỏ hàng</h2>
                <p className="mt-3 text-sm leading-7 text-zinc-500">
                  Giỏ hàng hiện được lưu và xử lý qua backend theo tài khoản người dùng.
                </p>
                <Link
                  className="mt-5 inline-flex min-h-12 items-center justify-center rounded-full bg-[#d71920] px-6 text-sm font-extrabold uppercase tracking-[0.08em] text-white transition hover:bg-[#b61016]"
                  to="/login?redirect=/cart"
                >
                  Đăng nhập
                </Link>
              </div>
            )}

            {checkoutNotice && (
              <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm font-medium text-amber-700">
                {checkoutNotice}
              </div>
            )}

            {isAuthenticated && loading && <LoadingState />}
            {isAuthenticated && error && <ErrorState message={error.message} onRetry={loadCart} />}
            {isAuthenticated && !loading && !error && !items.length && <EmptyCart />}
            {isAuthenticated &&
              !loading &&
              !error &&
              items.map((item) => (
                <CartItemRow key={item.id} item={item} onQuantityChange={updateQuantity} onRemove={removeItem} />
              ))}
          </div>

          {isAuthenticated && <CartSummary items={items} onCheckout={checkout} />}
        </div>
      </section>
    </>
  );
}

export default CartPage;

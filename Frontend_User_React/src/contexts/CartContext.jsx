import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { authApi } from '../api/authApi.js';
import { cartApi } from '../api/cartApi.js';
import { CART_CHANGED_EVENT } from '../utils/cartEvents.js';

const emptyCart = { items: [], totalItems: 0, subtotal: 0 };
const CartContext = createContext(null);

function getCartCount(cart) {
  return Number(cart?.totalItems ?? cart?.items?.reduce((sum, item) => sum + Number(item.quantity || 0), 0) ?? 0);
}

export function CartProvider({ children }) {
  const [cart, setCart] = useState(emptyCart);
  const [count, setCount] = useState(0);
  const [loading, setLoading] = useState(false);

  function applyCart(nextCart) {
    const resolvedCart = nextCart || emptyCart;
    setCart(resolvedCart);
    setCount(getCartCount(resolvedCart));
    return resolvedCart;
  }

  function resetCart() {
    applyCart(emptyCart);
  }

  async function refreshCart() {
    if (!authApi.getToken()) {
      resetCart();
      return emptyCart;
    }

    setLoading(true);
    try {
      const nextCart = await cartApi.getCart();
      return applyCart(nextCart);
    } catch (error) {
      if (error.status === 401) {
        authApi.logout();
        resetCart();
      }
      throw error;
    } finally {
      setLoading(false);
    }
  }

  async function addItem(payload) {
    const nextCart = await cartApi.addItem(payload);
    return applyCart(nextCart);
  }

  async function updateItem(itemId, quantity) {
    const nextCart = await cartApi.updateItem(itemId, quantity);
    return applyCart(nextCart);
  }

  async function removeItem(itemId) {
    const nextCart = await cartApi.removeItem(itemId);
    return applyCart(nextCart);
  }

  async function clearCart() {
    const nextCart = await cartApi.clearCart();
    return applyCart(nextCart);
  }

  useEffect(() => {
    refreshCart().catch(() => resetCart());
  }, []);

  useEffect(() => {
    function handleCartChanged(event) {
      if (event.detail?.cart) {
        applyCart(event.detail.cart);
      }
    }

    function handleStorage(event) {
      if (!event.key || event.key.includes('basecore_user') || event.key === 'token' || event.key === 'user') {
        refreshCart().catch(() => resetCart());
      }
    }

    window.addEventListener(CART_CHANGED_EVENT, handleCartChanged);
    window.addEventListener('storage', handleStorage);
    return () => {
      window.removeEventListener(CART_CHANGED_EVENT, handleCartChanged);
      window.removeEventListener('storage', handleStorage);
    };
  }, []);

  const value = useMemo(
    () => ({
      cart,
      count,
      loading,
      refreshCart,
      addItem,
      updateItem,
      removeItem,
      clearCart,
      resetCart,
    }),
    [cart, count, loading],
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used inside CartProvider');
  }

  return context;
}

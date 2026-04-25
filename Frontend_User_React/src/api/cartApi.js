import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';
import { normalizeCart } from '../utils/productMappers.js';
import { notifyCartChanged } from '../utils/cartEvents.js';

const cartFallbackUrls = [API_CONFIG.productServiceUrl, API_CONFIG.orderServiceUrl];

async function refreshCart() {
  const response = await apiRequest(
    {
      method: 'GET',
      url: API_ENDPOINTS.cart.detail,
    },
    cartFallbackUrls,
  );

  const cart = normalizeCart(response);
  notifyCartChanged(cart);
  return cart;
}

export const cartApi = {
  async getCart() {
    return refreshCart();
  },

  async clearCart() {
    await apiRequest(
      {
        method: 'DELETE',
        url: `${API_ENDPOINTS.cart.detail}/clear`,
      },
      cartFallbackUrls,
    );
    return refreshCart();
  },

  async getCount() {
    const response = await apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.cart.count,
      },
      cartFallbackUrls,
    );

    return Number(response?.count || response?.Count || 0);
  },

  async addItem({ productId, variantId, productVariantId, quantity }) {
    const resolvedVariantId = variantId ?? productVariantId ?? null;
    await apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.cart.items,
        data: { productId, variantId: resolvedVariantId, productVariantId: resolvedVariantId, quantity },
      },
      cartFallbackUrls,
    );

    return refreshCart();
  },

  async updateItem(itemId, quantity) {
    await apiRequest(
      {
        method: 'PUT',
        url: API_ENDPOINTS.cart.item(itemId),
        data: { quantity },
      },
      cartFallbackUrls,
    );

    return refreshCart();
  },

  async removeItem(itemId) {
    await apiRequest(
      {
        method: 'DELETE',
        url: API_ENDPOINTS.cart.item(itemId),
      },
      cartFallbackUrls,
    );

    return refreshCart();
  },
};

export const cartService = cartApi;

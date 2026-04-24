import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';
import { normalizeCart } from '../utils/productMappers.js';

const cartFallbackUrls = [API_CONFIG.productServiceUrl, API_CONFIG.orderServiceUrl];

export const cartApi = {
  async getCart() {
    const response = await apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.cart.detail,
      },
      cartFallbackUrls,
    );

    return normalizeCart(response);
  },

  addItem({ productId, productVariantId, quantity }) {
    return apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.cart.items,
        data: { productId, productVariantId, quantity },
      },
      cartFallbackUrls,
    );
  },

  updateItem(itemId, quantity) {
    return apiRequest(
      {
        method: 'PUT',
        url: API_ENDPOINTS.cart.item(itemId),
        data: { quantity },
      },
      cartFallbackUrls,
    );
  },

  removeItem(itemId) {
    return apiRequest(
      {
        method: 'DELETE',
        url: API_ENDPOINTS.cart.item(itemId),
      },
      cartFallbackUrls,
    );
  },
};

export const cartService = cartApi;

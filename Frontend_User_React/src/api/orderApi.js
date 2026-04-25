import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';

export const orderApi = {
  getMyOrders() {
    return apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.orders.list,
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },

  getOrderById(id) {
    return apiRequest(
      {
        method: 'GET',
        url: `${API_ENDPOINTS.orders.list}/${id}`,
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },

  createOrder(payload) {
    return apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.orders.list,
        data: payload,
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },
};

export const orderService = orderApi;

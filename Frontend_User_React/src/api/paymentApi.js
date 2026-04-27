import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';

export const paymentApi = {
  getPaymentsByOrder(orderId) {
    return apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.payments.byOrder(orderId),
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },

  createPayment(payload) {
    return apiRequest(
      {
        method: 'POST',
        url: '/api/payments',
        data: payload,
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },

  confirmSuccess(paymentId, payload = {}) {
    return apiRequest(
      {
        method: 'POST',
        url: `/api/payments/${paymentId}/confirm-success`,
        data: payload,
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },
};

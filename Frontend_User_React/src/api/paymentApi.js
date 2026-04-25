import { apiRequest } from './axiosClient.js';
import { API_CONFIG } from './endpoints.js';

export const paymentApi = {
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
};

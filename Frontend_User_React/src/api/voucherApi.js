import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';

export const voucherApi = {
  /**
   * Gets active vouchers that can be shown to customers.
   * @param {{ productId?: number, categoryId?: number, brandId?: number }} params
   * @returns {Promise<Array<object>>}
   */
  listVouchers(params = {}) {
    const query = Object.fromEntries(
      Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== ''),
    );

    return apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.vouchers.list,
        params: query,
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },

  /**
   * Validates a voucher code against a given subtotal and product/category/brand context.
   * @param {{ code: string, subtotal: number, productIds?: number[], categoryIds?: number[], brandIds?: number[], orderType?: string }} params
   * @returns {Promise<{ valid: boolean, message: string, discountAmount?: number, voucher?: object }>}
   */
  validateVoucher({ code, subtotal, productIds, categoryIds, brandIds, orderType }) {
    return apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.vouchers.validate,
        data: { code, subtotal, productIds, categoryIds, brandIds, orderType },
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },

  /**
   * Gets applicable vouchers for the current cart.
   * @param {{ subtotal: number, productIds?: number[], categoryIds?: number[], brandIds?: number[], orderType?: string }} params
   * @returns {Promise<Array<object>>}
   */
  getApplicableVouchers({ subtotal, productIds, categoryIds, brandIds, orderType }) {
    return apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.vouchers.applicable,
        data: { subtotal, productIds, categoryIds, brandIds, orderType },
      },
      [API_CONFIG.orderServiceUrl, API_CONFIG.productServiceUrl],
    );
  },
};

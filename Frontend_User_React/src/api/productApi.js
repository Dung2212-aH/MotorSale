import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';
import {
  normalizeCategory,
  normalizeFilters,
  normalizeProduct,
  normalizeProductList,
} from '../utils/productMappers.js';

function normalizeSort(sortBy) {
  return (
    {
      'price-asc': 'price_asc',
      'price-desc': 'price_desc',
      'name-asc': 'name_asc',
      'name-desc': 'name_desc',
      'year-asc': 'year_asc',
      'year-desc': 'year_desc',
    }[sortBy] || sortBy
  );
}

function cleanParams(params = {}) {
  return Object.fromEntries(
    Object.entries({ ...params, sortBy: normalizeSort(params.sortBy) }).filter(
      ([, value]) => value !== '' && value !== undefined && value !== null,
    ),
  );
}

export const productApi = {
  async getProducts(params = {}) {
    const response = await apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.products.list,
        params: cleanParams(params),
      },
      [API_CONFIG.productServiceUrl],
    );

    return normalizeProductList(response);
  },

  async getProductById(id) {
    const response = await apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.products.detail(id),
      },
      [API_CONFIG.productServiceUrl],
    );

    return normalizeProduct(response);
  },

  async getFilters() {
    const response = await apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.products.filters,
      },
      [API_CONFIG.productServiceUrl],
    );

    return normalizeFilters(response);
  },

  async getCategories() {
    const response = await apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.categories.list,
      },
      [API_CONFIG.productServiceUrl],
    );

    return (Array.isArray(response) ? response : response?.items || []).map(normalizeCategory);
  },
};

export const productService = productApi;

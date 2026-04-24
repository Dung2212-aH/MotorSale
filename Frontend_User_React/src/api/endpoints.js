export const API_CONFIG = {
  gatewayUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  authServiceUrl: import.meta.env.VITE_AUTH_SERVICE_URL || 'http://localhost:5002',
  userServiceUrl: import.meta.env.VITE_USER_SERVICE_URL || 'http://localhost:5002',
  productServiceUrl: import.meta.env.VITE_PRODUCT_SERVICE_URL || 'http://localhost:5001',
  orderServiceUrl: import.meta.env.VITE_ORDER_SERVICE_URL || 'http://localhost:5001',
  tokenKey: 'basecore_user_token',
  userKey: 'basecore_user_profile',
};

export const API_ENDPOINTS = {
  auth: {
    login: '/api/auth/login',
    register: '/api/auth/register',
  },
  products: {
    list: '/api/products',
    detail: (id) => `/api/products/${id}`,
    filters: '/api/products/filters',
  },
  categories: {
    list: '/api/categories',
  },
  cart: {
    detail: '/api/cart',
    items: '/api/cart/items',
    item: (id) => `/api/cart/items/${id}`,
  },
  orders: {
    list: '/api/orders',
  },
  users: {
    list: '/api/users',
    detail: (id) => `/api/users/${id}`,
  },
};

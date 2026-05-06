import axios from 'axios';
import {
  normalizeCart,
  normalizeCategory,
  normalizeFilters,
  normalizeProduct,
  normalizeProductList,
} from '../utils/productMappers.js';
import { notifyCartChanged } from '../utils/cartEvents.js';

const API_BASE_URL = '/api';
const TOKEN_KEY = 'token';
const USER_KEY = 'user';

export const AUTH_CHANGED_EVENT = 'basecore:auth-changed';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const getStorage = (type) => {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    return window[type];
  } catch {
    return null;
  }
};

const sessionAuthStorage = {
  getItem(key) {
    return getStorage('sessionStorage')?.getItem(key) ?? null;
  },

  setItem(key, value) {
    getStorage('sessionStorage')?.setItem(key, value);
  },

  removeItem(key) {
    getStorage('sessionStorage')?.removeItem(key);
  },
};

const legacyAuthStorage = {
  getItem(key) {
    return getStorage('localStorage')?.getItem(key) ?? null;
  },

  removeItem(key) {
    getStorage('localStorage')?.removeItem(key);
  },
};

const responseData = (response) => response.data;

const cleanParams = (params = {}) => {
  const sortMap = {
    'price-asc': 'price_asc',
    'price-desc': 'price_desc',
    'name-asc': 'name_asc',
    'name-desc': 'name_desc',
    'year-asc': 'year_asc',
    'year-desc': 'year_desc',
  };

  return Object.fromEntries(
    Object.entries({ ...params, sortBy: sortMap[params.sortBy] || params.sortBy }).filter(
      ([, value]) => value !== '' && value !== undefined && value !== null,
    ),
  );
};

const notifyAuthChanged = (user = null) => {
  window.dispatchEvent(new CustomEvent(AUTH_CHANGED_EVENT, { detail: { user } }));
};

const clearAuthStorage = (notify = true) => {
  sessionAuthStorage.removeItem(TOKEN_KEY);
  sessionAuthStorage.removeItem(USER_KEY);
  legacyAuthStorage.removeItem(TOKEN_KEY);
  legacyAuthStorage.removeItem(USER_KEY);

  if (notify) {
    notifyAuthChanged(null);
  }
};

const getStoredUser = () => {
  const rawUser = sessionAuthStorage.getItem(USER_KEY);

  if (!rawUser) {
    return null;
  }

  try {
    return JSON.parse(rawUser);
  } catch {
    return null;
  }
};

const isTokenExpired = (token) => {
  const claims = decodeJwtPayload(token);
  const expiresAt = Number(claims?.exp);

  if (!Number.isFinite(expiresAt)) {
    return true;
  }

  return Date.now() >= expiresAt * 1000;
};

const decodeJwtPayload = (token) => {
  if (!token) {
    return null;
  }

  try {
    const [, payload] = token.split('.');
    if (!payload) {
      return null;
    }

    const normalizedPayload = payload.replace(/-/g, '+').replace(/_/g, '/');
    const decodedPayload = decodeURIComponent(
      atob(normalizedPayload)
        .split('')
        .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join(''),
    );

    return JSON.parse(decodedPayload);
  } catch {
    return null;
  }
};

const getClaim = (claims, key) => claims?.[key] || claims?.[`http://schemas.xmlsoap.org/ws/2005/05/identity/claims/${key}`];

const normalizeLoginResponse = (data) => ({
  token: data?.token || data?.Token,
  userId: data?.userId || data?.UserId,
  username: data?.username || data?.Username,
  name: data?.name || data?.Name,
  email: data?.email || data?.Email,
  role: data?.role || data?.Role,
  userType: data?.userType ?? data?.UserType,
  expiresIn: data?.expiresIn || data?.ExpiresIn,
  raw: data,
});

const saveAuthUser = (user) => {
  if (!user?.token) {
    throw new Error('Không nhận được token đăng nhập từ máy chủ');
  }

  sessionAuthStorage.setItem(TOKEN_KEY, user.token);
  sessionAuthStorage.setItem(USER_KEY, JSON.stringify(user));
  legacyAuthStorage.removeItem(TOKEN_KEY);
  legacyAuthStorage.removeItem(USER_KEY);

  notifyAuthChanged(user);
};

const mergeStoredUser = (data = {}) => {
  const currentUser = getStoredUser();
  const token = currentUser?.token || getToken();

  if (!token) {
    return null;
  }

  const nextUser = {
    ...currentUser,
    ...data,
    token,
  };

  sessionAuthStorage.setItem(USER_KEY, JSON.stringify(nextUser));
  notifyAuthChanged(nextUser);
  return nextUser;
};

api.interceptors.request.use(
  (config) => {
    const token = getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,
  (error) => Promise.reject(error),
);

export const authApi = {
  async login({ username, password }) {
    const response = await api.post('/auth/login', { username, password });
    const user = normalizeLoginResponse(responseData(response));
    saveAuthUser(user);
    return user;
  },

  register: (data) => api.post('/auth/register', {
    username: data.username || data.email,
    email: data.email,
    phone: data.phone,
    name: data.name,
    password: data.password,
  }),

  logout() {
    clearAuthStorage();
  },

  getCurrentUser() {
    const token = getToken();

    if (!token) {
      clearAuthStorage(false);
      return null;
    }

    if (isTokenExpired(token)) {
      clearAuthStorage(false);
      return null;
    }

    const storedUser = getStoredUser();
    if (storedUser) {
      return storedUser;
    }

    const claims = decodeJwtPayload(token);
    return {
      token,
      userId: getClaim(claims, 'nameidentifier') || claims?.sub,
      username: getClaim(claims, 'name'),
      name: getClaim(claims, 'name'),
      email: getClaim(claims, 'email') || getClaim(claims, 'name'),
      role: getClaim(claims, 'role'),
      raw: claims,
    };
  },

  getToken: () => getToken(),

  updateStoredUser(data) {
    return mergeStoredUser(data);
  },
};

function getToken() {
  const token = sessionAuthStorage.getItem(TOKEN_KEY);

  if (token) {
    return token;
  }

  const legacyToken = legacyAuthStorage.getItem(TOKEN_KEY);
  if (legacyToken) {
    clearAuthStorage(false);
  }

  return null;
}

export const productApi = {
  async getAll(params) {
    const response = await api.get('/products', { params: cleanParams(params) });
    return normalizeProductList(responseData(response));
  },

  async search(params) {
    return this.getAll(params);
  },

  async getById(id) {
    const response = await api.get(`/products/${id}`);
    return normalizeProduct(responseData(response));
  },

  async getFilters() {
    const response = await api.get('/products/filters');
    return normalizeFilters(responseData(response));
  },

  getProducts(params) {
    return this.getAll(params);
  },

  getProductById(id) {
    return this.getById(id);
  },

  async getCategories() {
    const response = await categoryApi.getAll();
    return response.data;
  },
};

export const categoryApi = {
  async getAll() {
    const response = await api.get('/categories');
    const data = responseData(response);
    return {
      ...response,
      data: (Array.isArray(data) ? data : data?.items || data?.Items || []).map(normalizeCategory),
    };
  },
};

const normalizeCartResponse = (response) => {
  const cart = normalizeCart(responseData(response));
  notifyCartChanged(cart);
  return cart;
};

export const cartApi = {
  async getMine() {
    const response = await api.get('/cart');
    return normalizeCartResponse(response);
  },

  getCart() {
    return this.getMine();
  },

  async getCount() {
    const response = await api.get('/cart/count');
    const data = responseData(response);
    return Number(data?.count ?? data?.totalItems ?? data ?? 0);
  },

  async addItem(data) {
    await api.post('/cart/items', {
      productId: data.productId,
      productVariantId: data.variantId ?? data.productVariantId ?? null,
      quantity: data.quantity,
    });
    return this.getMine();
  },

  async updateItem(id, quantityOrData) {
    const data = typeof quantityOrData === 'object' ? quantityOrData : { quantity: quantityOrData };
    await api.put(`/cart/items/${id}`, data);
    return this.getMine();
  },

  async removeItem(id) {
    await api.delete(`/cart/items/${id}`);
    return this.getMine();
  },

  async clearCart() {
    const response = await api.delete('/cart/clear');
    return normalizeCartResponse(response);
  },
};

export const orderApi = {
  async getAll(params) {
    const response = await api.get('/orders', { params });
    return responseData(response);
  },

  getMyOrders() {
    return this.getAll();
  },

  async getById(id) {
    const response = await api.get(`/orders/${id}`);
    return responseData(response);
  },

  getOrderById(id) {
    return this.getById(id);
  },

  async createOrder(data) {
    const response = await api.post('/orders', data);
    return responseData(response);
  },

  async cancelOrder(id, reason) {
    const response = await api.put(`/orders/${id}/cancel`, { reason });
    return responseData(response);
  },
};

export const paymentApi = {
  async getPaymentsByOrder(orderId) {
    const response = await api.get(`/payments/order/${orderId}`);
    return responseData(response);
  },

  async createPayment(data) {
    const response = await api.post('/payments', data);
    return responseData(response);
  },

  async confirmSuccess(paymentId, data = {}) {
    const response = await api.post(`/payments/${paymentId}/confirm-success`, data);
    return responseData(response);
  },
};

export const voucherApi = {
  async getAll(params) {
    const response = await api.get('/vouchers', { params: cleanParams(params) });
    return responseData(response);
  },

  listVouchers(params) {
    return this.getAll(params);
  },

  async validateVoucher(data) {
    const response = await api.post('/vouchers/validate', data);
    return responseData(response);
  },

  async getApplicableVouchers(data) {
    const response = await api.post('/vouchers/applicable', data);
    return responseData(response);
  },
};

export const userApi = {
  async getProfile() {
    const response = await api.get('/users/me');
    return responseData(response);
  },

  async updateProfile(data) {
    const response = await api.put('/users/me', {
      name: data.name,
      email: data.email,
      phone: data.phone,
    });
    return responseData(response);
  },

  async changePassword(data) {
    const response = await api.put('/users/me/password', {
      currentPassword: data.currentPassword,
      newPassword: data.newPassword,
    });
    return responseData(response);
  },

  async getAddress() {
    const response = await api.get('/users/me/address');
    return responseData(response);
  },

  async updateAddress(data) {
    const response = await api.put('/users/me/address', {
      fullName: data.fullName,
      phoneNumber: data.phoneNumber,
      addressLine: data.addressLine,
      ward: data.ward,
      province: data.province,
      note: data.note,
      isDefault: true,
    });
    return responseData(response);
  },

  async getAll(params) {
    const response = await api.get('/users', { params });
    return responseData(response);
  },

  async getById(id) {
    const response = await api.get(`/users/${id}`);
    return responseData(response);
  },

  getUsers(params) {
    return this.getAll(params);
  },

  getUserById(id) {
    return this.getById(id);
  },
};

export const favoriteApi = {
  getMine: () => api.get('/favorites'),
  add: (productId) => api.post(`/favorites/${productId}`),
  remove: (productId) => api.delete(`/favorites/${productId}`),
};

export const contentApi = {
  getBlogPosts: (params) => api.get('/content/blog-posts', { params }),
  getFaqs: (params) => api.get('/content/faqs', { params }),
  createContactRequest: (data) => api.post('/content/contact-requests', data),
  getVoucher: (code) => api.get(`/content/vouchers/${code}`),
};

export default api;

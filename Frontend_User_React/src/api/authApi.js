import { apiRequest, getAuthToken } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';
import { storage } from '../utils/storage.js';

function normalizeLoginResponse(response) {
  const token = response?.token || response?.Token;

  return {
    token,
    userId: response?.userId || response?.UserId,
    username: response?.username || response?.Username,
    name: response?.name || response?.Name,
    email: response?.email || response?.Email,
    role: response?.role || response?.Role,
    expiresIn: response?.expiresIn || response?.ExpiresIn,
    raw: response,
  };
}

export const authApi = {
  async login({ username, password }) {
    const response = await apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.auth.login,
        data: { username, password },
      },
      [API_CONFIG.authServiceUrl, API_CONFIG.userServiceUrl],
    );

    const user = normalizeLoginResponse(response);

    if (user.token) {
      storage.set(API_CONFIG.tokenKey, user.token);
      storage.set(API_CONFIG.legacyTokenKey, user.token);
      storage.setJson(API_CONFIG.userKey, user);
      storage.setJson(API_CONFIG.legacyUserKey, user);
    }

    return user;
  },

  register(payload) {
    return apiRequest(
      {
        method: 'POST',
        url: API_ENDPOINTS.auth.register,
        data: {
          username: payload.username || payload.email,
          email: payload.email,
          phone: payload.phone,
          name: payload.name,
          password: payload.password,
        },
      },
      [API_CONFIG.authServiceUrl, API_CONFIG.userServiceUrl],
    );
  },

  logout() {
    storage.remove(API_CONFIG.tokenKey);
    storage.remove(API_CONFIG.legacyTokenKey);
    storage.remove(API_CONFIG.userKey);
    storage.remove(API_CONFIG.legacyUserKey);
  },

  getCurrentUser() {
    return storage.getJson(API_CONFIG.userKey) || storage.getJson(API_CONFIG.legacyUserKey);
  },

  getToken() {
    return getAuthToken();
  },
};

export const authService = authApi;

import { apiRequest } from './axiosClient.js';
import { API_CONFIG, API_ENDPOINTS } from './endpoints.js';

export const userApi = {
  getUsers(params = {}) {
    return apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.users.list,
        params,
      },
      [API_CONFIG.userServiceUrl, API_CONFIG.authServiceUrl],
    );
  },

  getUserById(id) {
    return apiRequest(
      {
        method: 'GET',
        url: API_ENDPOINTS.users.detail(id),
      },
      [API_CONFIG.userServiceUrl, API_CONFIG.authServiceUrl],
    );
  },
};

export const userService = userApi;

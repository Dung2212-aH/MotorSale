import axios from 'axios';
import { API_CONFIG } from './endpoints.js';
import { storage } from '../utils/storage.js';

function attachAuth(config) {
  const token = storage.get(API_CONFIG.tokenKey);

  if (token) {
    config.headers = {
      ...config.headers,
      Authorization: `Bearer ${token}`,
    };
  }

  return config;
}

function toApiError(error) {
  const responseData = error.response?.data;
  const backendMessage =
    typeof responseData === 'string'
      ? responseData
      : responseData?.message || responseData?.title || responseData?.detail;

  return {
    message: backendMessage || error.message || 'Khong the ket noi den may chu',
    status: error.response?.status,
    data: error.response?.data,
    originalError: error,
  };
}

function createClient(baseURL) {
  const client = axios.create({
    baseURL,
    timeout: 15000,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  client.interceptors.request.use(attachAuth);
  client.interceptors.response.use((response) => response.data, (error) => Promise.reject(toApiError(error)));

  return client;
}

export const gatewayClient = createClient(API_CONFIG.gatewayUrl);

export function directClient(baseURL) {
  return createClient(baseURL);
}

function shouldTryFallback(error) {
  return !error.status || [404, 405, 500, 502, 503, 504].includes(error.status);
}

export async function apiRequest(config, fallbackBaseUrls = []) {
  try {
    return await gatewayClient.request(config);
  } catch (error) {
    if (!shouldTryFallback(error)) {
      throw error;
    }

    let latestError = error;

    for (const baseURL of fallbackBaseUrls.filter(Boolean)) {
      try {
        return await directClient(baseURL).request(config);
      } catch (fallbackError) {
        latestError = fallbackError;

        if (!shouldTryFallback(fallbackError)) {
          throw fallbackError;
        }
      }
    }

    throw latestError;
  }
}

export default gatewayClient;

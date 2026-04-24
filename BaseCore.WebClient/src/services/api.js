import axios from 'axios';

const API_BASE_URL = '/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            if (window.location.pathname !== '/login') {
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export const authApi = {
    login: (username, password) => api.post('/auth/login', { username, password }),
};

export const productApi = {
    getAll: (params) => api.get('/products', { params }),
    search: (params) => api.get('/products', { params }),
    getFilters: () => api.get('/products/filters'),
    getById: (id) => api.get(`/products/${id}`),
    create: (data) => api.post('/products', data),
    update: (id, data) => api.put(`/products/${id}`, data),
    delete: (id) => api.delete(`/products/${id}`),
};

export const categoryApi = {
    getAll: () => api.get('/categories'),
    create: (data) => api.post('/categories', data),
    update: (id, data) => api.put(`/categories/${id}`, data),
    delete: (id) => api.delete(`/categories/${id}`),
};

export const userApi = {
    getAll: (params) => api.get('/users', { params }),
    create: (data) => api.post('/users', data),
    update: (id, data) => api.put(`/users/${id}`, data),
    delete: (id) => api.delete(`/users/${id}`),
};

export const cartApi = {
    getMine: () => api.get('/cart'),
    addItem: (data) => api.post('/cart/items', data),
    updateItem: (id, data) => api.put(`/cart/items/${id}`, data),
    removeItem: (id) => api.delete(`/cart/items/${id}`),
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

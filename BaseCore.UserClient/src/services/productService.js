import apiClient from './apiClient';

export function normalizeProduct(product = {}) {
  return {
    id: product.id,
    name: product.name || 'Sản phẩm',
    slug: product.slug || '',
    code: product.productCode || '',
    imageUrl: product.mainImageUrl || product.imageUrl || '',
    price: Number(product.salePrice || product.basePrice || 0),
    oldPrice: product.salePrice ? Number(product.basePrice || 0) : 0,
    status: product.status,
  };
}

export const productService = {
  async getBestSellers() {
    const response = await apiClient.get('/products', {
      params: {
        page: 1,
        pageSize: 10,
        status: 'Available',
        sortBy: 'popular',
      },
    });

    const rawItems = Array.isArray(response.data?.items) ? response.data.items : response.data;
    return Array.isArray(rawItems) ? rawItems.map(normalizeProduct) : [];
  },
};

import apiClient from './apiClient';

function normalizeProduct(product = {}) {
  return {
    id: product.id,
    name: product.name || product.productName || 'Sản phẩm',
    slug: product.slug || '',
    code: product.productCode || product.code || '',
    imageUrl: product.mainImageUrl || product.imageUrl || product.thumbnailUrl || '',
    basePrice: product.basePrice || product.price || 0,
    salePrice: product.salePrice,
    stockQuantity: product.stockQuantity,
  };
}

function normalizeCartItem(item = {}) {
  const product = normalizeProduct(item.product);
  const variant = item.productVariant || item.variant || null;
  const quantity = Number(item.quantity || 0);
  const unitPrice = Number(item.unitPrice || variant?.priceOverride || product.salePrice || product.basePrice || 0);

  return {
    id: item.id,
    productId: item.productId || product.id,
    productVariantId: item.productVariantId || variant?.id || null,
    product,
    variantName: variant?.name || variant?.variantName || '',
    quantity,
    unitPrice,
    lineTotal: Number(item.lineTotal || unitPrice * quantity),
  };
}

function normalizeCart(cart = {}) {
  const items = Array.isArray(cart.items) ? cart.items.map(normalizeCartItem) : [];
  const subtotal = items.reduce((sum, item) => sum + item.lineTotal, 0);

  return {
    id: cart.id,
    status: cart.status || 'Active',
    items,
    subtotal,
    totalItems: items.reduce((sum, item) => sum + item.quantity, 0),
  };
}

export const cartService = {
  async getCart() {
    const response = await apiClient.get('/cart');
    return normalizeCart(response.data);
  },

  async updateQuantity(itemId, quantity) {
    const response = await apiClient.put(`/cart/items/${itemId}`, { quantity });
    return normalizeCartItem(response.data);
  },

  async removeItem(itemId) {
    return apiClient.delete(`/cart/items/${itemId}`);
  },

  async applyVoucher(code) {
    const response = await apiClient.get(`/content/vouchers/${encodeURIComponent(code)}`);
    return response.data;
  },

  async checkout(payload) {
    const response = await apiClient.post('/orders', payload);
    return response.data;
  },
};

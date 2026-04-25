export function formatCurrency(value) {
  const amount = Number(value || 0);
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

export function normalizeImageUrl(url) {
  if (!url) {
    return '';
  }
  if (url.startsWith('//')) {
    return `https:${url}`;
  }
  return url.replace(/^http:\/\//, 'https://');
}

export function getProductPrice(product) {
  return product?.salePrice ?? product?.basePrice ?? product?.price ?? 0;
}

export function getProductImage(product) {
  const primaryImage = product?.images?.find((image) => image?.isPrimary)?.imageUrl;
  return normalizeImageUrl(primaryImage || product?.images?.[0]?.imageUrl || product?.mainImageUrl || product?.imageUrl || '');
}

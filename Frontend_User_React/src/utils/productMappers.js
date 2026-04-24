import { normalizeImageUrl } from './formatters.js';

function valueOf(source, camelKey, pascalKey = camelKey[0].toUpperCase() + camelKey.slice(1)) {
  return source?.[camelKey] ?? source?.[pascalKey];
}

export function normalizeProduct(raw) {
  if (!raw) {
    return null;
  }

  const images = valueOf(raw, 'images') || valueOf(raw, 'productImages') || [];
  const variants = valueOf(raw, 'variants') || valueOf(raw, 'productVariants') || [];
  const brand = valueOf(raw, 'brand');
  const category = valueOf(raw, 'category');

  return {
    id: valueOf(raw, 'id'),
    productCode: valueOf(raw, 'productCode'),
    name: valueOf(raw, 'name') || '',
    slug: valueOf(raw, 'slug'),
    categoryId: valueOf(raw, 'categoryId'),
    categoryName: category?.name || category?.Name || valueOf(raw, 'categoryName'),
    brandId: valueOf(raw, 'brandId'),
    brandName: brand?.name || brand?.Name || valueOf(raw, 'brandName'),
    productType: valueOf(raw, 'productType'),
    shortDescription: valueOf(raw, 'shortDescription'),
    description: valueOf(raw, 'description'),
    basePrice: Number(valueOf(raw, 'basePrice') || 0),
    salePrice: valueOf(raw, 'salePrice') == null ? null : Number(valueOf(raw, 'salePrice')),
    stockQuantity: valueOf(raw, 'stockQuantity'),
    mainImageUrl: normalizeImageUrl(valueOf(raw, 'mainImageUrl')),
    status: valueOf(raw, 'status'),
    condition: valueOf(raw, 'condition'),
    year: valueOf(raw, 'year'),
    mileage: valueOf(raw, 'mileage'),
    exteriorColor: valueOf(raw, 'exteriorColor'),
    transmission: valueOf(raw, 'transmission'),
    fuelType: valueOf(raw, 'fuelType'),
    engine: valueOf(raw, 'engine'),
    interiorColor: valueOf(raw, 'interiorColor'),
    seats: valueOf(raw, 'seats'),
    driveType: valueOf(raw, 'driveType'),
    vin: valueOf(raw, 'vin'),
    licensePlate: valueOf(raw, 'licensePlate'),
    images: images
      .map((image) => ({
        id: valueOf(image, 'id'),
        imageUrl: normalizeImageUrl(valueOf(image, 'imageUrl') || valueOf(image, 'url')),
        altText: valueOf(image, 'altText'),
        isPrimary: valueOf(image, 'isPrimary'),
        sortOrder: valueOf(image, 'sortOrder') || 0,
        raw: image,
      }))
      .filter((image) => image.imageUrl),
    variants: variants.map((variant) => ({
      id: valueOf(variant, 'id'),
      variantName: valueOf(variant, 'variantName'),
      sku: valueOf(variant, 'sku'),
      priceOverride: valueOf(variant, 'priceOverride') == null ? null : Number(valueOf(variant, 'priceOverride')),
      stockQuantity: valueOf(variant, 'stockQuantity'),
      status: valueOf(variant, 'status'),
      version: valueOf(variant, 'version'),
      exteriorColor: valueOf(variant, 'exteriorColor'),
      interiorColor: valueOf(variant, 'interiorColor'),
      raw: variant,
    })),
    raw,
  };
}

export function normalizeProductList(response) {
  const rawItems = Array.isArray(response) ? response : response?.items || response?.Items || [];
  const pageSize = response?.pageSize || response?.PageSize || rawItems.length;
  const totalCount = response?.totalCount || response?.TotalCount || rawItems.length;

  return {
    items: rawItems.map(normalizeProduct).filter(Boolean),
    totalCount,
    page: response?.page || response?.Page || 1,
    pageSize,
    totalPages: response?.totalPages || response?.TotalPages || Math.max(1, Math.ceil(totalCount / Math.max(pageSize, 1))),
  };
}

export function normalizeCategory(raw) {
  return {
    id: valueOf(raw, 'id'),
    name: valueOf(raw, 'name') || '',
    slug: valueOf(raw, 'slug'),
    parentCategoryId: valueOf(raw, 'parentCategoryId'),
    description: valueOf(raw, 'description'),
    sortOrder: valueOf(raw, 'sortOrder') || 0,
    isActive: valueOf(raw, 'isActive') !== false,
  };
}

export function normalizeFilters(response) {
  return {
    categories: (response?.categories || response?.Categories || []).map(normalizeCategory),
    brands: (response?.brands || response?.Brands || []).map((brand) => ({
      id: valueOf(brand, 'id'),
      name: valueOf(brand, 'name') || '',
    })),
    carModels: response?.carModels || response?.CarModels || [],
    showrooms: response?.showrooms || response?.Showrooms || [],
  };
}

export function normalizeCart(response) {
  const items = response?.items || response?.Items || [];
  return {
    ...response,
    items: items.map((item) => ({
      id: valueOf(item, 'id'),
      productId: valueOf(item, 'productId'),
      productVariantId: valueOf(item, 'productVariantId'),
      quantity: valueOf(item, 'quantity') || 1,
      unitPrice: Number(valueOf(item, 'unitPrice') || 0),
      lineTotal: Number(valueOf(item, 'lineTotal') || 0),
      product: normalizeProduct(valueOf(item, 'product')) || {},
    })),
  };
}

import { normalizeImageUrl } from './formatters.js';

function valueOf(source, camelKey, pascalKey = camelKey[0].toUpperCase() + camelKey.slice(1)) {
  return source?.[camelKey] ?? source?.[pascalKey];
}

function normalizeVariant(raw) {
  if (!raw) {
    return null;
  }

  const images = valueOf(raw, 'images') || [];

  return {
    id: valueOf(raw, 'id'),
    productId: valueOf(raw, 'productId'),
    variantName: valueOf(raw, 'variantName') || '',
    sku: valueOf(raw, 'sku') || '',
    priceOverride: valueOf(raw, 'priceOverride') == null ? null : Number(valueOf(raw, 'priceOverride')),
    stockQuantity: valueOf(raw, 'stockQuantity'),
    status: valueOf(raw, 'status'),
    version: valueOf(raw, 'version'),
    color: valueOf(raw, 'color') || valueOf(raw, 'exteriorColor'),
    exteriorColor: valueOf(raw, 'exteriorColor') || valueOf(raw, 'color'),
    interiorColor: valueOf(raw, 'interiorColor'),
    images: images
      .map((image) => ({
        id: valueOf(image, 'id'),
        productVariantId: valueOf(image, 'productVariantId') ?? valueOf(raw, 'id'),
        imageUrl: normalizeImageUrl(valueOf(image, 'imageUrl') || valueOf(image, 'url')),
        altText: valueOf(image, 'altText'),
        isPrimary: valueOf(image, 'isPrimary'),
        sortOrder: valueOf(image, 'sortOrder') || 0,
        raw: image,
      }))
      .filter((image) => image.imageUrl),
    raw,
  };
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
    carModelId: valueOf(raw, 'carModelId'),
    carModelName: valueOf(raw, 'carModelName') || valueOf(valueOf(raw, 'carModel'), 'name'),
    showroomId: valueOf(raw, 'showroomId'),
    showroomName: valueOf(raw, 'showroomName') || valueOf(valueOf(raw, 'showroom'), 'name'),
    productType: valueOf(raw, 'productType'),
    shortDescription: valueOf(raw, 'shortDescription'),
    description: valueOf(raw, 'description'),
    basePrice: Number(valueOf(raw, 'basePrice') || 0),
    salePrice: valueOf(raw, 'salePrice') == null ? null : Number(valueOf(raw, 'salePrice')),
    stockQuantity: valueOf(raw, 'stockQuantity'),
    mainImageUrl: normalizeImageUrl(valueOf(raw, 'mainImageUrl')),
    status: valueOf(raw, 'status'),
    isActive: valueOf(raw, 'isActive') !== false,
    mainColor: valueOf(raw, 'mainColor'),
    motorcycleType: valueOf(raw, 'motorcycleType'),
    engineCapacity: valueOf(raw, 'engineCapacity'),
    power: valueOf(raw, 'power'),
    torque: valueOf(raw, 'torque'),
    fuelTankCapacity: valueOf(raw, 'fuelTankCapacity'),
    frontBrake: valueOf(raw, 'frontBrake'),
    rearBrake: valueOf(raw, 'rearBrake'),
    hasAbs: valueOf(raw, 'hasAbs'),
    weight: valueOf(raw, 'weight'),
    seatHeight: valueOf(raw, 'seatHeight'),
    origin: valueOf(raw, 'origin'),
    warrantyMonths: valueOf(raw, 'warrantyMonths'),
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
        productVariantId: valueOf(image, 'productVariantId'),
        imageUrl: normalizeImageUrl(valueOf(image, 'imageUrl') || valueOf(image, 'url')),
        altText: valueOf(image, 'altText'),
        isPrimary: valueOf(image, 'isPrimary'),
        sortOrder: valueOf(image, 'sortOrder') || 0,
        raw: image,
      }))
      .filter((image) => image.imageUrl),
    variants: variants.map(normalizeVariant).filter(Boolean),
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
    partCompatibleTypes: (response?.partCompatibleTypes || response?.PartCompatibleTypes || []).map((item) => ({
      id: valueOf(item, 'id'),
      name: valueOf(item, 'name') || '',
      brandId: valueOf(item, 'brandId'),
      brandName: valueOf(item, 'brandName') || '',
    })),
  };
}

export function normalizeCart(response) {
  const items = response?.items || response?.Items || [];
  const normalizedItems = items.map((item) => {
    const quantity = Number(valueOf(item, 'quantity') || 1);
    const unitPrice = Number(valueOf(item, 'unitPrice') || 0);
    const lineTotal = Number(valueOf(item, 'lineTotal') || unitPrice * quantity);

    return {
      id: valueOf(item, 'id'),
      cartId: valueOf(item, 'cartId'),
      productId: valueOf(item, 'productId'),
      productVariantId: valueOf(item, 'productVariantId'),
      quantity,
      unitPrice,
      lineTotal,
      product: normalizeProduct(valueOf(item, 'product')) || {},
      productVariant: normalizeVariant(valueOf(item, 'productVariant')) || valueOf(item, 'productVariant'),
    };
  });

  return {
    ...response,
    id: valueOf(response, 'id'),
    userId: valueOf(response, 'userId'),
    status: valueOf(response, 'status'),
    items: normalizedItems,
    totalItems: Number(valueOf(response, 'totalItems') ?? normalizedItems.reduce((sum, item) => sum + item.quantity, 0)),
    subtotal: Number(valueOf(response, 'subtotal') ?? normalizedItems.reduce((sum, item) => sum + item.lineTotal, 0)),
  };
}

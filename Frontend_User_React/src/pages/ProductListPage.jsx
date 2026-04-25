import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { authApi } from '../api/authApi.js';
import { productApi } from '../api/productApi.js';
import Breadcrumb from '../components/Breadcrumb.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import ProductFilters from '../components/ProductFilters.jsx';
import ProductGrid from '../components/ProductGrid.jsx';
import { useCart } from '../contexts/CartContext.jsx';
import { useNotification } from '../contexts/NotificationContext.jsx';

function cleanParams(params) {
  return Object.fromEntries(Object.entries(params).filter(([, value]) => value !== '' && value !== undefined && value !== null));
}

function normalizeText(value = '') {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim();
}

function normalizeSlug(value = '') {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}

function resolveProductTypeAliases(value = '') {
  const normalizedValue = normalizeText(value);

  if (!normalizedValue) {
    return [];
  }

  const aliasGroups = [
    ['xe ga', 'xe tay ga', 'tay ga', 'scooter'],
    ['xe con tay', 'xe côn tay', 'con tay', 'xe the thao', 'xe thể thao', 'sport'],
    ['xe so', 'xe số', 'underbone'],
    ['phu tung', 'phụ tùng', 'phu kien', 'phụ kiện', 'accessory', 'accessories'],
  ];

  const matchedGroup = aliasGroups.find((group) => group.some((item) => item === normalizedValue || item.includes(normalizedValue) || normalizedValue.includes(item)));
  return matchedGroup || [normalizedValue];
}

function matchesProductType(product, expectedProductType) {
  const aliases = resolveProductTypeAliases(expectedProductType);

  if (!aliases.length) {
    return true;
  }

  const haystacks = [product?.productType, product?.categoryName, product?.name]
    .map((value) => normalizeText(value || ''))
    .filter(Boolean);

  return haystacks.some((field) => aliases.some((alias) => field === alias || field.includes(alias) || alias.includes(field)));
}

function ProductListPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const { addItem } = useCart();
  const { notify } = useNotification();
  const [filters, setFilters] = useState(null);
  const [productsData, setProductsData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const queryValues = useMemo(
    () => ({
      keyword: searchParams.get('keyword') || '',
      categoryId: searchParams.get('categoryId') || '',
      categorySlug: searchParams.get('categorySlug') || '',
      productType: searchParams.get('productType') || '',
      brandId: searchParams.get('brandId') || '',
      brandSlug: searchParams.get('brandSlug') || '',
      minPrice: searchParams.get('minPrice') || '',
      maxPrice: searchParams.get('maxPrice') || '',
      sortBy: searchParams.get('sortBy') || '',
      page: Number(searchParams.get('page') || 1),
      pageSize: 12,
    }),
    [searchParams],
  );

  const resolvedCategoryId = useMemo(() => {
    if (queryValues.categoryId) {
      return queryValues.categoryId;
    }

    if (!queryValues.categorySlug || !filters?.categories?.length) {
      return '';
    }

    const normalizedSlug = queryValues.categorySlug.toLowerCase();
    const matchedCategory = filters.categories.find(
      (category) => (category.slug || '').toLowerCase() === normalizedSlug || (category.name || '').toLowerCase() === normalizedSlug,
    );

    return matchedCategory?.id || '';
  }, [filters?.categories, queryValues.categoryId, queryValues.categorySlug]);

  const resolvedBrandId = useMemo(() => {
    if (queryValues.brandId) {
      return queryValues.brandId;
    }

    if (!queryValues.brandSlug || !filters?.brands?.length) {
      return '';
    }

    const normalizedBrandSlug = normalizeSlug(queryValues.brandSlug);
    const matchedBrand = filters.brands.find((brand) => normalizeSlug(brand.name || '') === normalizedBrandSlug);

    return matchedBrand?.id || '';
  }, [filters?.brands, queryValues.brandId, queryValues.brandSlug]);

  const apiQueryValues = useMemo(() => {
    const nextValues = {
      ...queryValues,
      categoryId: resolvedCategoryId,
      brandId: resolvedBrandId,
      categorySlug: undefined,
      brandSlug: undefined,
    };

    if (queryValues.productType) {
      nextValues.productType = undefined;
      nextValues.page = 1;
      nextValues.pageSize = 200;
    }

    return cleanParams(nextValues);
  }, [queryValues, resolvedCategoryId, resolvedBrandId]);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const [filtersResponse, productsResponse] = await Promise.all([
        productApi.getFilters(),
        productApi.getProducts(apiQueryValues),
      ]);

      setFilters(filtersResponse);

      if (queryValues.productType) {
        const filteredItems = productsResponse.items.filter((item) => matchesProductType(item, queryValues.productType));
        const currentPage = queryValues.page || 1;
        const currentPageSize = queryValues.pageSize || 12;
        const startIndex = (currentPage - 1) * currentPageSize;

        setProductsData({
          ...productsResponse,
          items: filteredItems.slice(startIndex, startIndex + currentPageSize),
          totalCount: filteredItems.length,
          page: currentPage,
          pageSize: currentPageSize,
          totalPages: Math.max(1, Math.ceil(filteredItems.length / currentPageSize)),
        });
      } else {
        setProductsData(productsResponse);
      }
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, [apiQueryValues]);

  async function addToCart(product) {
    if (!authApi.getToken()) {
      notify('Vui lòng đăng nhập để thêm vào giỏ hàng', 'error');
      navigate('/login?redirect=/cart');
      return;
    }

    const detail = await productApi.getProductById(product.id);
    if (detail.variants?.length) {
      notify('Vui lòng chọn phiên bản/màu sắc', 'error');
      navigate(`/products/${product.id}`);
      return;
    }

    try {
      await addItem({ productId: product.id, quantity: 1 });
      notify('Đã thêm vào giỏ hàng', 'success');
    } catch (err) {
      notify(err.message || 'Không thể thêm vào giỏ hàng', 'error');
    }
  }

  function updateFilters(values) {
    setSearchParams(cleanParams(values));
  }

  const products = productsData?.items || [];
  const activeCategory = filters?.categories?.find((category) => String(category.id) === String(resolvedCategoryId));
  const activeBrand = filters?.brands?.find((brand) => String(brand.id) === String(resolvedBrandId));
  const pageTitle = activeCategory?.name || queryValues.productType || activeBrand?.name || 'Tất cả sản phẩm';

  return (
    <>
      <Breadcrumb items={[{ label: 'Sản phẩm' }, { label: pageTitle }]} />

      <section className="bg-[linear-gradient(180deg,#f5f6f8_0%,#ffffff_26%)] py-10">
        <div className="mx-auto grid w-full max-w-[1200px] gap-8 px-4 xl:grid-cols-[320px_minmax(0,1fr)]">
          <ProductFilters
            filters={filters}
            values={{
              ...queryValues,
              categoryId: resolvedCategoryId || queryValues.categoryId,
              brandId: resolvedBrandId || queryValues.brandId,
            }}
            onChange={updateFilters}
          />

          <div className="min-w-0 space-y-6">
            <div className="flex flex-col gap-4 rounded-[30px] border border-zinc-200 bg-white px-5 py-5 shadow-[0_18px_50px_rgba(15,23,42,0.07)] md:flex-row md:items-center md:justify-between md:px-7">
              <div>
                <div className="text-[12px] font-extrabold uppercase tracking-[0.18em] text-zinc-400">Danh sách sản phẩm</div>
                <h2 className="mt-2 text-[28px] leading-tight font-black text-zinc-950">{pageTitle}</h2>
              </div>

              <div className="flex flex-wrap gap-3 text-sm">
                <span className="inline-flex items-center rounded-full bg-zinc-100 px-4 py-2 font-semibold text-zinc-700">
                  {productsData?.totalCount || 0} sản phẩm
                </span>
                {queryValues.sortBy && (
                  <span className="inline-flex items-center rounded-full bg-[#d71920]/8 px-4 py-2 font-semibold text-[#d71920]">
                    Sắp xếp: {queryValues.sortBy}
                  </span>
                )}
              </div>
            </div>

            {loading && <LoadingState />}
            {error && <ErrorState message={error.message} onRetry={load} />}

            {!loading && !error && (
              <>
                <ProductGrid products={products} onAddToCart={addToCart} />

                {productsData?.totalPages > 1 && (
                  <div className="flex flex-wrap items-center justify-center gap-2 pt-2">
                    {Array.from({ length: productsData.totalPages }).map((_, index) => {
                      const page = index + 1;
                      const isActive = page === queryValues.page;

                      return (
                        <button
                          key={page}
                          type="button"
                          className={`inline-flex h-11 min-w-11 items-center justify-center rounded-full px-4 text-sm font-bold transition ${
                            isActive
                              ? 'bg-[#111111] text-white shadow-[0_16px_30px_rgba(15,23,42,0.18)]'
                              : 'border border-zinc-200 bg-white text-zinc-700 hover:border-zinc-950 hover:text-zinc-950'
                          }`}
                          onClick={() => updateFilters({ ...queryValues, page })}
                        >
                          {page}
                        </button>
                      );
                    })}
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </section>
    </>
  );
}

export default ProductListPage;

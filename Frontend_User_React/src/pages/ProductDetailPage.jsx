import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { authApi } from '../api/authApi.js';
import { productApi } from '../api/productApi.js';
import Breadcrumb from '../components/Breadcrumb.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import ProductCommitmentSidebar from '../components/product/ProductCommitmentSidebar.jsx';
import ProductImageGallery from '../components/product/ProductImageGallery.jsx';
import ProductInfoBox, { defaultPromotions } from '../components/product/ProductInfoBox.jsx';
import ProductPromotionBox from '../components/product/ProductPromotionBox.jsx';
import ProductTabs from '../components/product/ProductTabs.jsx';
import ProductVoucherBox from '../components/product/ProductVoucherBox.jsx';
import RelatedProductSection from '../components/product/RelatedProductSection.jsx';
import { useCart } from '../contexts/CartContext.jsx';
import { useNotification } from '../contexts/NotificationContext.jsx';
import { useAsync } from '../hooks/useAsync.js';
import { normalizeProductOptions } from '../utils/productOptions.js';
import { storage } from '../utils/storage.js';

const VIEWED_PRODUCTS_KEY = 'frontend_user_recent_products';

function dedupeProducts(products) {
  const seen = new Set();

  return products.filter((item) => {
    if (!item?.id || seen.has(item.id)) {
      return false;
    }

    seen.add(item.id);
    return true;
  });
}

function matchesSelection(variant, version, color) {
  const versionMatches = version ? variant.version === version : true;
  const colorMatches = color ? variant.color === color : true;
  return versionMatches && colorMatches;
}

function ProductDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [quantity, setQuantity] = useState(1);
  const [brandProducts, setBrandProducts] = useState([]);
  const [typeProducts, setTypeProducts] = useState([]);
  const [relatedProducts, setRelatedProducts] = useState([]);
  const [viewedProducts, setViewedProducts] = useState([]);
  const [selectedVersion, setSelectedVersion] = useState('');
  const [selectedColor, setSelectedColor] = useState('');
  const [selectedImage, setSelectedImage] = useState(null);
  const { addItem } = useCart();
  const { notify } = useNotification();
  const { data: product, loading, error, run } = useAsync(() => productApi.getProductById(id), [id]);

  const options = useMemo(() => normalizeProductOptions(product), [product]);

  useEffect(() => {
    async function loadCollections() {
      if (!product?.id) {
        setBrandProducts([]);
        setTypeProducts([]);
        setRelatedProducts([]);
        return;
      }

      const requests = [
        product?.brandId ? productApi.getProducts({ brandId: product.brandId, page: 1, pageSize: 8 }) : Promise.resolve({ items: [] }),
        product?.categoryId ? productApi.getProducts({ categoryId: product.categoryId, page: 1, pageSize: 8 }) : Promise.resolve({ items: [] }),
      ];

      try {
        const [brandResponse, typeResponse] = await Promise.all(requests);
        const sameBrand = (brandResponse?.items || []).filter((item) => String(item.id) !== String(product.id)).slice(0, 4);
        const sameType = (typeResponse?.items || []).filter((item) => String(item.id) !== String(product.id)).slice(0, 4);

        setBrandProducts(sameBrand);
        setTypeProducts(sameType);
        setRelatedProducts(dedupeProducts([...sameBrand, ...sameType]).slice(0, 4));
      } catch {
        setBrandProducts([]);
        setTypeProducts([]);
        setRelatedProducts([]);
      }
    }

    loadCollections();
  }, [product]);

  useEffect(() => {
    if (!product?.id) {
      return;
    }

    const current = storage.getJson(VIEWED_PRODUCTS_KEY, []);
    const next = dedupeProducts([
      {
        id: product.id,
        name: product.name,
        brandName: product.brandName,
        categoryName: product.categoryName,
        basePrice: product.basePrice,
        salePrice: product.salePrice,
        mainImageUrl: product.mainImageUrl,
        images: product.images,
      },
      ...(Array.isArray(current) ? current : []),
    ]).slice(0, 8);

    storage.setJson(VIEWED_PRODUCTS_KEY, next);
    setViewedProducts(next.filter((item) => String(item.id) !== String(product.id)).slice(0, 4));
  }, [product]);

  useEffect(() => {
    if (!product) {
      setSelectedVersion('');
      setSelectedColor('');
      setSelectedImage(null);
      return;
    }

    const firstInStockVariant =
      options.variants.find((variant) => Number(variant.stockQuantity || 0) > 0) ||
      options.variants[0] ||
      null;
    const defaultVersion = firstInStockVariant?.version || options.versions[0] || '';
    const candidateColors = options.variants
      .filter((variant) => matchesSelection(variant, defaultVersion, ''))
      .map((variant) => variant.color)
      .filter(Boolean);
    const defaultColor = candidateColors[0] || options.colors[0] || '';
    const firstImage =
      options.images.find((image) => {
        const colorMatches = defaultColor ? image.color === defaultColor : true;
        const versionMatches = defaultVersion && image.version ? image.version === defaultVersion : true;
        return colorMatches && versionMatches;
      }) || options.images[0] || null;

    setSelectedVersion(defaultVersion);
    setSelectedColor(defaultColor);
    setSelectedImage(firstImage);
  }, [product, options]);

  const selectedVariant = useMemo(() => {
    if (!options.variants.length) {
      return null;
    }

    return (
      options.variants.find((variant) => matchesSelection(variant, selectedVersion, selectedColor)) ||
      options.variants.find((variant) => matchesSelection(variant, selectedVersion, '')) ||
      options.variants.find((variant) => matchesSelection(variant, '', selectedColor)) ||
      options.variants[0]
    );
  }, [options.variants, selectedColor, selectedVersion]);

  const availableColorOptions = useMemo(() => {
    const colors = options.variants
      .filter((variant) => !selectedVersion || variant.version === selectedVersion)
      .map((variant) => variant.color)
      .filter(Boolean);

    return colors.length ? [...new Set(colors)] : options.colors;
  }, [options.colors, options.variants, selectedVersion]);

  useEffect(() => {
    if (!availableColorOptions.length) {
      if (selectedColor) {
        setSelectedColor('');
      }
      return;
    }

    if (!selectedColor || !availableColorOptions.includes(selectedColor)) {
      setSelectedColor(availableColorOptions[0]);
    }
  }, [availableColorOptions, selectedColor]);

  const visibleImages = useMemo(() => {
    if (!options.images.length) {
      return [];
    }

    const byColor = selectedColor ? options.images.filter((image) => image.color === selectedColor) : [];
    const byColorAndVersion =
      selectedColor && selectedVersion
        ? byColor.filter((image) => !image.version || image.version === selectedVersion)
        : byColor;

    if (byColorAndVersion.length) {
      return byColorAndVersion;
    }

    if (byColor.length) {
      return byColor;
    }

    if (selectedVersion) {
      const byVersion = options.images.filter((image) => image.version === selectedVersion);
      if (byVersion.length) {
        return byVersion;
      }
    }

    return options.images;
  }, [options.images, selectedColor, selectedVersion]);

  useEffect(() => {
    if (!visibleImages.length) {
      setSelectedImage(null);
      return;
    }

    if (selectedImage && visibleImages.some((image) => image.imageUrl === selectedImage.imageUrl)) {
      return;
    }

    const preferredImage =
      (selectedVariant?.imageUrl && visibleImages.find((image) => image.imageUrl === selectedVariant.imageUrl)) ||
      visibleImages[0];

    setSelectedImage(preferredImage);
  }, [selectedImage, selectedVariant, visibleImages]);

  function handleSelectVersion(version) {
    setSelectedVersion(version);

    const matchedVariant = options.variants.find((variant) => matchesSelection(variant, version, selectedColor));
    const fallbackVariant = options.variants.find((variant) => matchesSelection(variant, version, ''));

    if (matchedVariant?.color) {
      setSelectedColor(matchedVariant.color);
    } else if (fallbackVariant?.color) {
      setSelectedColor(fallbackVariant.color);
    }

    const nextImage =
      options.images.find((image) => {
        const versionMatches = image.version ? image.version === version : true;
        const colorTarget = matchedVariant?.color || fallbackVariant?.color || selectedColor;
        const colorMatches = colorTarget ? image.color === colorTarget : true;
        return versionMatches && colorMatches;
      }) ||
      options.images.find((image) => (image.version ? image.version === version : false)) ||
      null;

    if (nextImage) {
      setSelectedImage(nextImage);
    }
  }

  function handleSelectColor(color) {
    setSelectedColor(color);

    const matchedVariant = options.variants.find((variant) => matchesSelection(variant, selectedVersion, color));
    if (matchedVariant?.version && matchedVariant.version !== selectedVersion) {
      setSelectedVersion(matchedVariant.version);
    }

    const nextImage =
      options.images.find((image) => {
        const colorMatches = image.color === color;
        const versionMatches = selectedVersion && image.version ? image.version === selectedVersion : true;
        return colorMatches && versionMatches;
      }) ||
      options.images.find((image) => image.color === color) ||
      (matchedVariant?.imageUrl ? options.images.find((image) => image.imageUrl === matchedVariant.imageUrl) : null);

    if (nextImage) {
      setSelectedImage(nextImage);
    }
  }

  function handleSelectImage(image) {
    setSelectedImage(image);

    if (image?.color) {
      setSelectedColor(image.color);
    }

    if (image?.version) {
      setSelectedVersion(image.version);
      return;
    }

    const variantByImage = options.variants.find((variant) => variant.imageUrl && variant.imageUrl === image?.imageUrl);
    if (variantByImage?.version) {
      setSelectedVersion(variantByImage.version);
    }
  }

  async function addToCart(redirectToCart = true) {
    if (!product) {
      return;
    }

    if (!authApi.getToken()) {
      notify('Vui lòng đăng nhập để thêm vào giỏ hàng', 'error');
      navigate('/login?redirect=/cart');
      return;
    }

    if (options.hasVariantData && !selectedVariant?.id) {
      notify('Vui lòng chọn phiên bản/màu sắc', 'error');
      return;
    }

    const stockValue = selectedVariant?.stockQuantity ?? product?.stockQuantity;
    if (Number(stockValue || 0) <= 0 && !String(selectedVariant?.status || product?.status || '').toLowerCase().includes('available')) {
      notify('Sản phẩm đã hết hàng', 'error');
      return;
    }

    try {
      await addItem({
        productId: product.id,
        variantId: Number.isFinite(Number(selectedVariant?.id)) ? Number(selectedVariant.id) : null,
        quantity,
      });
      notify('Đã thêm vào giỏ hàng', 'success');
    } catch (err) {
      notify(err.message || 'Không thể thêm vào giỏ hàng', 'error');
      return;
    }

    if (redirectToCart) {
      navigate('/cart');
    }
  }

  async function quickAdd(item) {
    if (!authApi.getToken()) {
      notify('Vui lòng đăng nhập để thêm vào giỏ hàng', 'error');
      navigate('/login?redirect=/cart');
      return;
    }

    const detail = await productApi.getProductById(item.id);
    if (detail.variants?.length) {
      notify('Vui lòng chọn phiên bản/màu sắc', 'error');
      navigate(`/products/${item.id}`);
      return;
    }

    try {
      await addItem({ productId: item.id, quantity: 1 });
      notify('Đã thêm vào giỏ hàng', 'success');
    } catch (err) {
      notify(err.message || 'Không thể thêm vào giỏ hàng', 'error');
    }
  }

  const fallbackNotes = options.fallbackNotes;
  const showVersionSelector = options.hasVersionOptions && options.versions.length > 1;
  const showColorSelector = options.hasColorOptions;
  const colorStatusText = showColorSelector ? selectedColor || 'Đang cập nhật' : 'Đang cập nhật';

  return (
    <>
      <Breadcrumb
        items={[
          { label: 'Sản phẩm bán chạy', to: '/products' },
          { label: product?.name || 'Chi tiết sản phẩm' },
        ]}
      />

      <section className="bg-[#f7f7f7] py-8 sm:py-10">
        <div className="mx-auto w-full max-w-[1280px] px-4">
          {loading && <LoadingState />}
          {error && <ErrorState message={error.message} onRetry={run} />}

          {product && (
            <div className="space-y-8">
              <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px] xl:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)_320px]">
                <ProductImageGallery product={product} images={visibleImages} selectedImage={selectedImage} onSelectImage={handleSelectImage} />

                <div className="space-y-5">
                  <ProductInfoBox
                    product={product}
                    quantity={quantity}
                    onQuantityChange={setQuantity}
                    onAddToCart={() => addToCart(false)}
                    onBuyNow={() => addToCart(true)}
                    selectedVersion={selectedVersion}
                    onSelectVersion={handleSelectVersion}
                    selectedColor={selectedColor}
                    onSelectColor={handleSelectColor}
                    selectedVariant={selectedVariant}
                    versionOptions={options.versions}
                    colorOptions={options.colors}
                    availableColorOptions={availableColorOptions}
                    showVersionSelector={showVersionSelector}
                    showColorSelector={showColorSelector}
                    colorStatusText={colorStatusText}
                    fallbackNotes={fallbackNotes}
                  />
                  <ProductPromotionBox promotions={defaultPromotions} />
                </div>

                <div className="space-y-5 lg:hidden xl:block">
                  <ProductCommitmentSidebar />
                </div>
              </div>

              <ProductVoucherBox product={product} />

              <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px]">
                <ProductTabs product={product} />
                <div className="hidden lg:block xl:hidden">
                  <ProductCommitmentSidebar />
                </div>
              </div>
            </div>
          )}
        </div>
      </section>

      {product && (
        <div className="bg-white py-10">
          <div className="mx-auto flex w-full max-w-[1280px] flex-col gap-10 px-4">
            <RelatedProductSection
              title="Cùng thương hiệu"
              products={brandProducts}
              onAddToCart={quickAdd}
              emptyMessage="Chưa có sản phẩm cùng thương hiệu."
            />
            <RelatedProductSection
              title="Cùng loại"
              products={typeProducts}
              onAddToCart={quickAdd}
              emptyMessage="Chưa có sản phẩm cùng loại."
            />
            <RelatedProductSection
              title="Sản phẩm liên quan"
              products={relatedProducts}
              onAddToCart={quickAdd}
              emptyMessage="Chưa có sản phẩm liên quan."
            />
            {viewedProducts.length > 0 && (
              <RelatedProductSection
                title="Sản phẩm đã xem"
                products={viewedProducts}
                onAddToCart={quickAdd}
                emptyMessage="Chưa có sản phẩm đã xem."
              />
            )}
          </div>
        </div>
      )}
    </>
  );
}

export default ProductDetailPage;

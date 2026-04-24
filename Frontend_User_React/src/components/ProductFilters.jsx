const inputClassName =
  'min-h-12 rounded-2xl border border-zinc-200 bg-zinc-50 px-4 text-sm text-zinc-900 outline-none transition focus:border-[#d71920] focus:bg-white';

function ProductFilters({ filters, values, onChange }) {
  const categories = filters?.categories || [];
  const brands = filters?.brands || [];
  const productTypes = ['Xe tay ga', 'Xe côn tay', 'Xe số'];

  function update(name, value) {
    const nextValues = { ...values, [name]: value, page: 1 };

    if (name === 'categoryId') {
      nextValues.categorySlug = '';
    }

    if (name === 'brandId') {
      nextValues.brandSlug = '';
    }

    onChange(nextValues);
  }

  return (
    <aside className="sticky top-28 overflow-hidden rounded-[30px] border border-zinc-200 bg-white shadow-[0_22px_60px_rgba(15,23,42,0.08)]">
      <div className="bg-[radial-gradient(circle_at_top_left,rgba(255,255,255,0.2),transparent_34%),linear-gradient(135deg,#111111,#d71920)] px-6 py-6 text-white">
        <div className="text-[12px] font-extrabold uppercase tracking-[0.18em] text-white/70">Bộ lọc</div>
        <h2 className="mt-2 text-[24px] leading-tight font-black">Chọn mẫu xe phù hợp</h2>
        <p className="mt-2 text-sm leading-6 text-white/80">Lọc nhanh theo danh mục, hãng, khoảng giá và nhu cầu sử dụng.</p>
      </div>

      <div className="space-y-5 p-6">
        <label className="grid gap-2.5">
          <span className="text-[12px] font-extrabold uppercase tracking-[0.14em] text-zinc-500">Từ khóa</span>
          <input
            className={inputClassName}
            value={values.keyword || ''}
            onChange={(event) => update('keyword', event.target.value)}
            placeholder="Tên xe, phụ tùng..."
          />
        </label>

        <label className="grid gap-2.5">
          <span className="text-[12px] font-extrabold uppercase tracking-[0.14em] text-zinc-500">Danh mục</span>
          <select
            className={inputClassName}
            value={values.categoryId || ''}
            onChange={(event) => update('categoryId', event.target.value)}
          >
            <option value="">Tất cả danh mục</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>

        <label className="grid gap-2.5">
          <span className="text-[12px] font-extrabold uppercase tracking-[0.14em] text-zinc-500">Loại xe</span>
          <select
            className={inputClassName}
            value={values.productType || ''}
            onChange={(event) => update('productType', event.target.value)}
          >
            <option value="">Tất cả loại xe</option>
            {productTypes.map((productType) => (
              <option key={productType} value={productType}>
                {productType}
              </option>
            ))}
          </select>
        </label>

        <label className="grid gap-2.5">
          <span className="text-[12px] font-extrabold uppercase tracking-[0.14em] text-zinc-500">Hãng xe</span>
          <select
            className={inputClassName}
            value={values.brandId || ''}
            onChange={(event) => update('brandId', event.target.value)}
          >
            <option value="">Tất cả hãng</option>
            {brands.map((brand) => (
              <option key={brand.id} value={brand.id}>
                {brand.name}
              </option>
            ))}
          </select>
        </label>

        <label className="grid gap-2.5">
          <span className="text-[12px] font-extrabold uppercase tracking-[0.14em] text-zinc-500">Khoảng giá</span>
          <select
            className={inputClassName}
            value={`${values.minPrice || ''}-${values.maxPrice || ''}`}
            onChange={(event) => {
              const [minPrice, maxPrice] = event.target.value.split('-');
              onChange({ ...values, minPrice, maxPrice, page: 1 });
            }}
          >
            <option value="-">Tất cả mức giá</option>
            <option value="0-10000000">Dưới 10.000.000đ</option>
            <option value="10000000-30000000">10.000.000đ - 30.000.000đ</option>
            <option value="30000000-60000000">30.000.000đ - 60.000.000đ</option>
            <option value="60000000-">Trên 60.000.000đ</option>
          </select>
        </label>

        <label className="grid gap-2.5">
          <span className="text-[12px] font-extrabold uppercase tracking-[0.14em] text-zinc-500">Sắp xếp</span>
          <select
            className={inputClassName}
            value={values.sortBy || ''}
            onChange={(event) => update('sortBy', event.target.value)}
          >
            <option value="">Mặc định</option>
            <option value="price_asc">Giá thấp đến cao</option>
            <option value="price_desc">Giá cao xuống thấp</option>
            <option value="name_asc">Tên A-Z</option>
            <option value="name_desc">Tên Z-A</option>
            <option value="year_desc">Hàng mới</option>
          </select>
        </label>

        <button
          type="button"
          className="inline-flex min-h-12 w-full items-center justify-center rounded-full border border-zinc-200 bg-zinc-950 px-5 text-sm font-extrabold text-white transition hover:bg-[#d71920]"
          onClick={() =>
            onChange({
              ...values,
              keyword: '',
              categoryId: '',
              categorySlug: '',
              productType: '',
              brandId: '',
              brandSlug: '',
              minPrice: '',
              maxPrice: '',
              sortBy: '',
              page: 1,
            })
          }
        >
          Xóa bộ lọc
        </button>
      </div>
    </aside>
  );
}

export default ProductFilters;

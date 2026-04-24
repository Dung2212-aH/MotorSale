import { getProductImage } from '../../utils/formatters.js';

function ProductImageGallery({ product, images = [], selectedImage, onSelectImage }) {
  const fallbackImage = getProductImage(product);
  const activeImage = selectedImage?.imageUrl || images[0]?.imageUrl || fallbackImage;
  const galleryImages = images.length ? images : fallbackImage ? [{ imageUrl: fallbackImage, altText: product?.name || 'Product' }] : [];

  return (
    <div className="overflow-hidden rounded-2xl border border-zinc-200 bg-white">
      <div className="relative border-b border-zinc-100 bg-[radial-gradient(circle_at_top,rgba(215,25,32,0.08),transparent_48%),linear-gradient(180deg,#ffffff,#f7f7f7)] p-4 sm:p-6">
        <div className="absolute left-4 top-4 rounded-full bg-[#d71920] px-3 py-1 text-[11px] font-bold uppercase tracking-[0.18em] text-white">
          EURO Moto
        </div>
        <div className="flex aspect-square items-center justify-center rounded-2xl bg-white p-4 sm:p-8">
          {activeImage ? (
            <img src={activeImage} alt={selectedImage?.altText || product?.name} className="h-full w-full object-contain" />
          ) : (
            <div className="grid h-full w-full place-items-center rounded-2xl bg-zinc-100 text-sm font-bold uppercase tracking-[0.18em] text-zinc-400">
              No Image
            </div>
          )}
        </div>
      </div>

      <div className="overflow-x-auto px-4 py-4 sm:px-6">
        <div className="flex min-w-max gap-3">
          {galleryImages.map((image, index) => {
            const active = image.imageUrl === activeImage;

            return (
              <button
                key={`${image.imageUrl}-${index}`}
                type="button"
                className={`flex h-20 w-20 shrink-0 items-center justify-center rounded-xl border bg-white p-2 transition ${
                  active ? 'border-[#d71920] shadow-[0_10px_24px_rgba(215,25,32,0.16)]' : 'border-zinc-200 hover:border-zinc-300'
                }`}
                onClick={() => onSelectImage?.(image)}
              >
                <img src={image.imageUrl} alt={image.altText || `${product?.name || 'Product'} ${index + 1}`} className="h-full w-full object-contain" />
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default ProductImageGallery;

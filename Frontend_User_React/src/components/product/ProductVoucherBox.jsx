import { useState } from 'react';

const vouchers = [
  {
    code: 'DOLA20',
    description: 'Nhập mã để giảm ngay 20.000đ cho đơn hàng từ 200.000đ.',
  },
  {
    code: 'DOLA50',
    description: 'Nhập mã để giảm ngay 50.000đ cho đơn hàng từ 500.000đ.',
  },
  {
    code: 'FREESHIP',
    description: 'Nhập mã để miễn phí vận chuyển cho đơn từ 300.000đ.',
  },
];

function ProductVoucherBox() {
  const [copiedCode, setCopiedCode] = useState('');

  async function copyCode(code) {
    try {
      await navigator.clipboard.writeText(code);
      setCopiedCode(code);
      window.setTimeout(() => setCopiedCode(''), 1800);
    } catch {
      setCopiedCode('');
    }
  }

  return (
    <div className="rounded-2xl border border-zinc-200 bg-white">
      <div className="flex items-center gap-3 border-b border-zinc-100 bg-[linear-gradient(90deg,#fff5f5,#ffffff)] px-5 py-4">
        <div className="grid h-11 w-11 place-items-center rounded-full bg-[#fff0f0] text-[#d71920]">
          <svg viewBox="0 0 24 24" className="h-6 w-6 fill-current" aria-hidden="true">
            <path d="M5 3h8a2 2 0 0 1 2 2v2h1a3 3 0 0 1 3 3v3h-2v-3a1 1 0 0 0-1-1h-1v2a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Zm0 2v6h8V5H5Zm3 10h11a2 2 0 0 1 2 2v4H10a2 2 0 0 1-2-2v-4Zm2 2v2h9v-2h-9Z" />
          </svg>
        </div>
        <div>
          <div className="text-base font-bold text-zinc-950">Nhận voucher ngay !!!</div>
          <div className="text-sm text-zinc-500">Lưu mã và dùng ở bước thanh toán.</div>
        </div>
      </div>

      <div className="grid gap-4 p-5 lg:grid-cols-3">
        {vouchers.map((voucher) => {
          const copied = copiedCode === voucher.code;

          return (
            <div key={voucher.code} className="rounded-2xl border border-dashed border-[#f3c7c9] bg-[#fff9f9] p-4">
              <div className="text-sm leading-6 text-zinc-700">
                Nhập mã <b>{voucher.code}</b>. {voucher.description}
              </div>
              <button
                type="button"
                className={`mt-4 inline-flex min-h-10 items-center justify-center rounded-xl px-4 text-sm font-bold transition ${
                  copied ? 'bg-zinc-900 text-white' : 'bg-[#d71920] text-white hover:bg-[#b9161c]'
                }`}
                onClick={() => copyCode(voucher.code)}
              >
                {copied ? 'Đã sao chép' : 'Sao chép'}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default ProductVoucherBox;

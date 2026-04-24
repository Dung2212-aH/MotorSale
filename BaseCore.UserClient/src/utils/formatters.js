export function formatCurrency(value) {
  const amount = Number(value || 0);

  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

export function getErrorMessage(error, fallback = 'Đã có lỗi xảy ra. Vui lòng thử lại.') {
  return error?.response?.data?.message || error?.message || fallback;
}

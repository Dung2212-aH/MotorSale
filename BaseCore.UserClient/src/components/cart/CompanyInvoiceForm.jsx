function CompanyInvoiceForm({ enabled, onEnabledChange, values, onChange }) {
  const setValue = (field) => (event) => onChange({ ...values, [field]: event.target.value });

  return (
    <section className="invoice-panel">
      <label className="invoice-toggle">
        <input type="checkbox" checked={enabled} onChange={(event) => onEnabledChange(event.target.checked)} />
        <span> Xuất hóa đơn công ty</span>
      </label>

      {enabled && (
        <div className="invoice-fields">
          <input value={values.companyName} onChange={setValue('companyName')} placeholder="Tên công ty" />
          <input value={values.taxCode} onChange={setValue('taxCode')} placeholder="Mã số thuế" />
          <input value={values.companyAddress} onChange={setValue('companyAddress')} placeholder="Địa chỉ công ty" />
          <input
            type="email"
            value={values.invoiceEmail}
            onChange={setValue('invoiceEmail')}
            placeholder="Email nhận hóa đơn"
          />
        </div>
      )}
    </section>
  );
}

export default CompanyInvoiceForm;

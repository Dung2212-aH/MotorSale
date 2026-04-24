const deliverySlots = ['08h00 - 12h00', '14h00 - 18h00', '19h00 - 21h00'];

function DeliveryTimeSelector({ value, onChange }) {
  return (
    <section className="cart-option-panel">
      <h2>Thời gian giao hàng</h2>
      <div className="delivery-selector">
        <span>Chọn thời gian</span>
        <div className="delivery-options">
          {deliverySlots.map((slot) => (
            <label key={slot} className={value === slot ? 'selected' : ''}>
              <input
                type="radio"
                name="deliveryTime"
                value={slot}
                checked={value === slot}
                onChange={() => onChange(slot)}
              />
              {slot}
            </label>
          ))}
        </div>
      </div>
    </section>
  );
}

export default DeliveryTimeSelector;

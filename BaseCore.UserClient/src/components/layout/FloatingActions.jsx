import Icon from '../common/Icon.jsx';

function FloatingActions() {
  return (
    <div className="floating-actions" aria-label="Liên hệ nhanh">
      <a href="tel:19006750" className="floating-button phone-button" aria-label="Gọi ngay cho chúng tôi">
        <Icon name="phone" />
      </a>
      <a href="https://zalo.me/19006750" className="floating-button zalo-button" aria-label="Chat với chúng tôi qua Zalo">
        Zalo
      </a>
      <a href="https://m.me/" className="floating-button messenger-button" aria-label="Chat với chúng tôi qua Messenger">
        <Icon name="chat" />
      </a>
    </div>
  );
}

export default FloatingActions;

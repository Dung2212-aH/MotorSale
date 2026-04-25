import { BrowserRouter } from 'react-router-dom';
import AppRoutes from './routes/AppRoutes.jsx';
import { CartProvider } from './contexts/CartContext.jsx';
import { NotificationProvider } from './contexts/NotificationContext.jsx';

function App() {
  return (
    <BrowserRouter>
      <NotificationProvider>
        <CartProvider>
          <AppRoutes />
        </CartProvider>
      </NotificationProvider>
    </BrowserRouter>
  );
}

export default App;

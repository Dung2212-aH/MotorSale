import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import CartPage from './pages/CartPage.jsx';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/cart" replace />} />
        <Route path="/cart" element={<CartPage />} />
        <Route path="*" element={<Navigate to="/cart" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

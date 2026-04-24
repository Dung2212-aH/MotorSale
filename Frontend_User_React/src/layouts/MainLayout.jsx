import { Outlet } from 'react-router-dom';
import FloatingActions from '../components/FloatingActions.jsx';
import Footer from '../components/Footer.jsx';
import Header from '../components/Header.jsx';

function MainLayout() {
  return (
    <div className="min-h-screen bg-white text-zinc-900">
      <Header />
      <main className="min-h-[calc(100vh-240px)]">
        <Outlet />
      </main>
      <Footer />
      <FloatingActions />
    </div>
  );
}

export default MainLayout;

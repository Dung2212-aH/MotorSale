import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { authApi } from '../api/authApi.js';
import AuthForm from '../components/AuthForm.jsx';
import Breadcrumb from '../components/Breadcrumb.jsx';

function LoginPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(values) {
    setLoading(true);
    setError('');

    try {
      await authApi.login(values);
      navigate(searchParams.get('redirect') || '/');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <Breadcrumb items={[{ label: 'Đăng nhập' }]} />
      <AuthForm
        title="Đăng nhập tài khoản"
        subtitle="Nhập email hoặc số điện thoại để tiếp tục mua hàng."
        fields={[
          { name: 'username', label: 'Email hoặc số điện thoại', type: 'text', required: true, placeholder: 'Email hoặc số điện thoại' },
          { name: 'password', label: 'Mật khẩu', type: 'password', required: true, placeholder: 'Mật khẩu' },
        ]}
        submitLabel="Đăng nhập"
        loading={loading}
        error={error}
        onSubmit={handleSubmit}
        footer={<p>Chưa có tài khoản? <Link to="/register">Đăng ký</Link></p>}
      />
    </>
  );
}

export default LoginPage;

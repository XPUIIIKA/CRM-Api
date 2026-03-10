import { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';

// Імпортуємо наші екшени та клієнт
import { logout, setUser } from './store/authSlice';
import { apiClient } from './api/apiClient';

// Імпорти всіх сторінок
import Layout from './components/Layout';
import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import ProfilePage from './pages/ProfilePage';
import OrdersPage from './pages/OrdersPage';
import ProductsPage from './pages/ProductsPage';
import TeamPage from './pages/TeamPage';
import StatisticsPage from './pages/StatisticsPage';
import MessagesPage from './pages/MessagesPage';
import SystemAdminPage from './pages/SystemAdminPage';

const ProtectedRoute = ({ children }) => {
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return children;
};

export default function App() {
  const dispatch = useDispatch();
  const token = useSelector((state) => state.auth.token);
  
  // Стейт для того, щоб не показувати сторінки, поки йде перевірка токена
  const [isInitializing, setIsInitializing] = useState(true);

  // Кусочек кода из App.jsx
  useEffect(() => {
    const initAuth = async () => {
      if (token) {
        try {
          // МЕНЯЕМ ЗДЕСЬ НА НОВЫЙ ЭНДПОИНТ
          const response = await apiClient.get('/api/auth/me/full');
          
          dispatch(setUser(response.data));
        } catch (error) {
          console.error('Токен недійсний або прострочений', error);
          dispatch(logout()); 
        }
      }
      setIsInitializing(false); 
    };

    initAuth();
  }, [token, dispatch]);

  // Показуємо екран завантаження, поки йде запит до бекенда
  if (isInitializing) {
    return (
      <div className="h-screen w-full bg-falcon-dark flex items-center justify-center">
        <div className="text-white text-xl animate-pulse font-light tracking-widest">
          Завантаження Falcon CRM...
        </div>
      </div>
    );
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        
        <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
          <Route index element={<HomePage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="orders" element={<OrdersPage />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="managers" element={<TeamPage />} />
          <Route path="statistics" element={<StatisticsPage />} />
          <Route path="messages" element={<MessagesPage />} />
          <Route path="admin" element={<SystemAdminPage />} />
        </Route>
        
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
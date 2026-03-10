import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { loginSuccess } from '../store/authSlice';
import { apiClient } from '../api/apiClient'; // Наш налаштований axios
import logoImage from '../assets/logo.svg'; 

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(''); // Стейт для помилки
  const [isLoading, setIsLoading] = useState(false); // Стейт для завантаження
  
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await apiClient.post('/api/auth/login', { 
        email, 
        password 
      });
      
      const token = response.data.token || response.data; 

      if (token) {
        dispatch(loginSuccess({ token }));
        navigate('/');
      } else {
        setError('Сервер не повернув токен авторизації');
      }
      
    } catch (err) {
      console.error('Помилка авторизації:', err);
      
      // Перевіряємо, чи є відповідь від сервера
      if (err.response) {
        // Якщо сервер повернув 400 (Погана перевірка) або 401 (Не авторизовано)
        if (err.response.status === 400 || err.response.status === 401) {
          
          // Пробуємо дістати текст помилки від самого бекенда (якщо він його надсилає)
          // Зазвичай бекенд шле щось типу { message: "Невірний пароль" } або { title: "..." }
          const serverMessage = err.response.data?.message || err.response.data?.title;
          
          // Якщо бекенд прислав текст - показуємо його. Якщо ні - показуємо стандартний текст.
          setError(serverMessage ? serverMessage : 'Невірний email або пароль');
        } 
        // Обробка інших помилок сервера (наприклад, 500)
        else if (err.response.status >= 500) {
          setError('Внутрішня помилка сервера. Спробуйте пізніше.');
        } 
        else {
          setError(`Помилка: ${err.response.status}`);
        }
      } else {
        // Якщо сервер взагалі не відповів (впав або немає інтернету)
        setError('Помилка з\'єднання з сервером');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="w-full min-h-screen bg-falcon-dark flex flex-col items-center justify-center font-sans">
      
      <div className="flex items-center gap-3 mb-8 text-white">
        <img src={logoImage} alt="Falcon Logo" className="h-14 w-auto object-contain" />
        <span className="text-4xl font-light tracking-wide">Falcon</span>
      </div>

      <div className="bg-white rounded-xl shadow-2xl w-full max-w-sm px-10 py-10">
        <h2 className="text-2xl text-falcon-dark text-center mb-6">Вхід</h2>

        {/* Вивід помилки, якщо вона є */}
        {error && (
          <div className="bg-red-50 text-red-500 text-sm p-3 rounded mb-4 text-center border border-red-100">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <input
              type="email"
              placeholder="Пошта"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full bg-falcon-light text-falcon-dark placeholder-falcon-dark/60 rounded px-4 py-2.5 focus:outline-none focus:ring-1 focus:ring-falcon-dark transition-all"
            />
          </div>

          <div>
            <input
              type="password"
              placeholder="Пароль"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full bg-falcon-light text-falcon-dark placeholder-falcon-dark/60 rounded px-4 py-2.5 focus:outline-none focus:ring-1 focus:ring-falcon-dark transition-all"
            />
          </div>

          <div className="text-right">
            <a href="#" className="text-xs text-falcon-dark/70 hover:text-falcon-dark underline decoration-falcon-dark/40 hover:decoration-falcon-dark transition-colors">
              Забули пароль?
            </a>
          </div>

          <div className="relative flex items-center justify-center pt-6">
            <div className="absolute w-full border-t border-gray-300 top-[65%]"></div>
            <button
              type="submit"
              disabled={isLoading}
              className={`relative z-10 text-white px-12 py-2 rounded transition-all text-lg ${
                isLoading ? 'bg-gray-400 cursor-not-allowed' : 'bg-falcon-dark hover:brightness-110'
              }`}
            >
              {isLoading ? 'Вхід...' : 'Вхід'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
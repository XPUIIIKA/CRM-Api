import { useState, useEffect } from 'react';
import { apiClient } from '../api/apiClient';

export default function TeamPage() {
  const [team, setTeam] = useState([]);
  const [roles, setRoles] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Фільтри та пошук
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedRole, setSelectedRole] = useState('');
  const [sortBy, setSortBy] = useState('name');

  // Модалка створення співробітника
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [newUser, setNewUser] = useState({
    fullName: '',
    email: '',
    phoneNumber: '',
    password: '',
    roleId: ''
  });

  // Завантаження даних
  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [usersRes, rolesRes] = await Promise.all([
        apiClient.get('/api/user'),
        apiClient.get('/api/role')
      ]);
      setTeam(usersRes.data || []);
      setRoles(rolesRes.data || []);
    } catch (err) {
      console.error('Помилка завантаження команди:', err);
      setError('Не вдалося завантажити список співробітників.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Видалення
  const handleDeleteUser = async (userId) => {
    if (!userId) {
      alert("Помилка: ID користувача відсутній");
      return;
    }

    if (window.confirm('Ви впевнені, що хочете видалити цього користувача?')) {
      try {
        await apiClient.delete(`/api/user/${userId}`);
        setTeam(prev => prev.filter(user => user.id !== userId && user.Id !== userId));
        alert('Користувача видалено!');
      } catch (err) {
        console.error('Помилка видалення:', err);
        // Додаємо вивід тексту помилки з бекенда (якщо він є)
        const serverMsg = err.response?.data?.message || err.response?.data?.title || '';
        alert(`Не вдалося видалити користувача. ${serverMsg}`);
      }
    }
  };

  // Створення
  const handleCreateUser = async (e) => {
    e.preventDefault();
    try {
      await apiClient.post('/api/user', newUser);
      alert('Співробітника успішно додано!');
      setIsAddModalOpen(false);
      setNewUser({ fullName: '', email: '', phoneNumber: '', password: '', roleId: '' });
      await fetchData(); // Оновлюємо список
    } catch (err) {
      console.error('Помилка створення:', err);
      alert('Не вдалося створити співробітника. Перевірте дані.');
    }
  };

  // Логіка фільтрації
  const getFilteredTeam = () => {
    let result = [...team];

    if (selectedRole) {
      result = result.filter(u => u.roleId === selectedRole || u.role?.id === selectedRole);
    }

    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      result = result.filter(u => 
        (u.fullName || '').toLowerCase().includes(q) || 
        (u.email || '').toLowerCase().includes(q) ||
        (u.phoneNumber || '').includes(q)
      );
    }

    if (sortBy === 'name') {
      result.sort((a, b) => (a.fullName || '').localeCompare(b.fullName || ''));
    }

    return result;
  };

  const displayedTeam = getFilteredTeam();

  if (isLoading) return <div className="p-8 text-center text-falcon-dark">Завантаження команди...</div>;
  if (error) return <div className="p-8 text-center text-red-500">{error}</div>;

  return (
    <div className="w-full h-full flex flex-col relative">
      {/* Шапка */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl text-falcon-dark font-medium">Наша команда</h1>
        
        <div className="flex gap-4 items-center">
          <button 
            onClick={() => setIsAddModalOpen(true)}
            className="w-9 h-9 bg-falcon-accent text-falcon-dark font-medium rounded hover:brightness-110 transition-all flex items-center justify-center text-xl shadow-sm"
            title="Додати співробітника"
          >
            +
          </button>
          
          <input 
            type="text" 
            placeholder="Пошук (Ім'я, Email, Телефон)" 
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-4 py-2 rounded-md w-64 focus:outline-none focus:ring-1 focus:ring-falcon-dark text-sm"
          />
        </div>
      </div>

      {/* Фільтри */}
      <div className="flex justify-between mb-8">
        <div className="flex items-center gap-3">
          <select 
            value={selectedRole}
            onChange={(e) => setSelectedRole(e.target.value)}
            className="bg-falcon-light text-falcon-dark px-4 py-1.5 rounded-md min-w-[150px] focus:outline-none text-sm cursor-pointer border-none shadow-sm"
          >
            <option value="">Всі посади</option>
            {roles.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
          </select>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-falcon-dark">Сортувати за:</span>
          <select 
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            className="bg-falcon-light text-falcon-dark px-4 py-1.5 rounded-md min-w-[150px] focus:outline-none text-sm cursor-pointer border-none shadow-sm"
          >
            <option value="name">Ім'ям</option>
          </select>
        </div>
      </div>

      {/* Сітка карток */}
      {displayedTeam.length === 0 ? (
        <div className="text-center py-12 text-gray-500">Співробітників не знайдено</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {displayedTeam.map((user) => {
            const userId = user.id || user.Id; // Перестраховка на випадок великої літери
            const userRoleName = user.role?.name || roles.find(r => r.id === user.roleId)?.name || 'Не вказано';

            return (
              <div key={userId} className="relative flex rounded-xl overflow-hidden shadow-md h-full group">
                {/* Основна частина картки */}
                <div className="flex-1 bg-falcon-dark flex flex-col border border-falcon-light/20 relative transition-colors group-hover:border-falcon-light/40 py-6">
                  
                  <div className="px-5 flex flex-col items-center flex-1">
                    {/* КРУГЛА ФОТОГРАФІЯ (rounded-full) */}
                    <img 
                      src={`https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName || 'User')}&background=random&color=fff&size=150`}
                      alt={user.fullName} 
                      className="w-24 h-24 object-cover rounded-full mb-6 shadow-md border-2 border-falcon-light/30"
                    />

                    {/* Інфо */}
                    <div className="text-center w-full space-y-4">
                      <div>
                        <p className="text-[10px] text-falcon-light/70 uppercase tracking-wider mb-0.5">ПІБ:</p>
                        <p className="text-white text-base font-medium leading-tight">{user.fullName || user.login || 'Невідомо'}</p>
                      </div>

                      <div>
                        <p className="text-[10px] text-falcon-light/70 uppercase tracking-wider mb-0.5">Посада</p>
                        <p className="text-falcon-accent font-medium text-sm">{userRoleName}</p>
                      </div>

                      <div>
                        <p className="text-[10px] text-falcon-light/70 uppercase tracking-wider mb-0.5">Зв'язатися:</p>
                        <p className="text-white text-sm truncate mb-1" title={user.email}>{user.email}</p>
                        <p className="text-white text-sm">{user.phoneNumber || 'Телефон не вказано'}</p>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Права панель кнопок */}
                <div className="w-10 bg-falcon-accent flex flex-col items-center py-3 gap-4 border-l border-falcon-dark/10">
                  <button 
                    onClick={() => handleDeleteUser(userId)}
                    className="text-falcon-dark hover:scale-125 hover:text-red-600 transition-all" 
                    title="Видалити співробітника"
                  >
                    ✕
                  </button>
                  <button className="text-falcon-dark hover:scale-125 transition-all text-lg" title="Налаштування ролі">
                    ⚙
                  </button>
                  
                  <div className="mt-auto mb-1">
                    <button className="text-falcon-dark hover:scale-125 transition-all">•••</button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* МОДАЛКА ДОДАВАННЯ КОРИСТУВАЧА */}
      {isAddModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col">
            <div className="bg-falcon-dark px-6 py-4 flex justify-between items-center">
              <h3 className="text-white text-lg font-medium">Додати співробітника</h3>
              <button onClick={() => setIsAddModalOpen(false)} className="text-white hover:text-falcon-light text-xl">✕</button>
            </div>
            
            <form onSubmit={handleCreateUser} className="p-6 flex flex-col gap-4">
              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">ПІБ <span className="text-red-500">*</span></span>
                <input 
                  type="text" required
                  value={newUser.fullName}
                  onChange={(e) => setNewUser({...newUser, fullName: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                  placeholder="Іванов Іван Іванович"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Email <span className="text-red-500">*</span></span>
                <input 
                  type="email" required
                  value={newUser.email}
                  onChange={(e) => setNewUser({...newUser, email: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                  placeholder="ivanov@example.com"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Телефон</span>
                <input 
                  type="tel"
                  value={newUser.phoneNumber}
                  onChange={(e) => setNewUser({...newUser, phoneNumber: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                  placeholder="+380..."
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Пароль <span className="text-red-500">*</span></span>
                <input 
                  type="password" required
                  value={newUser.password}
                  onChange={(e) => setNewUser({...newUser, password: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                  placeholder="Мінімум 6 символів"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Роль (Посада) <span className="text-red-500">*</span></span>
                <select 
                  required
                  value={newUser.roleId}
                  onChange={(e) => setNewUser({...newUser, roleId: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none cursor-pointer"
                >
                  <option value="">Оберіть роль...</option>
                  {roles.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
                </select>
              </label>

              <div className="flex gap-3 pt-4 border-t border-gray-100 justify-end mt-2">
                <button type="button" onClick={() => setIsAddModalOpen(false)} className="px-4 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-md transition-colors font-medium">
                  Скасувати
                </button>
                <button type="submit" className="px-6 py-2 text-sm bg-falcon-dark text-white rounded-md hover:brightness-110 transition-all font-medium shadow-sm">
                  Додати
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
import { useState, useEffect } from 'react';
import { apiClient } from '../api/apiClient';

export default function SystemAdminPage() {
  const [activeTab, setActiveTab] = useState('companies'); 
  
  const [companies, setCompanies] = useState([]);
  const [admins, setAdmins] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  // Стейти для модалок
  const [isCompanyModalOpen, setIsCompanyModalOpen] = useState(false);
  const [isAdminModalOpen, setIsAdminModalOpen] = useState(false);

  // СТЕЙТ ДЛЯ ПОКАЗУ ЗГЕНЕРОВАНОГО ПАРОЛЮ
  const [createdCredentials, setCreatedCredentials] = useState(null);

  // Форма нової компанії
  const [newCompany, setNewCompany] = useState({
    companyName: '',
    ownerFullName: '',
    ownerEmail: ''
  });

  // Форма нового сисадміна
  const [newAdmin, setNewAdmin] = useState({
    login: '',
    email: '',
    password: '',
    isRoot: false
  });

  // Завантаження даних
  const fetchData = async () => {
    setIsLoading(true);
    try {
      if (activeTab === 'companies') {
        const res = await apiClient.get('/api/companies', { params: { page: 1, pageSize: 100 } });
        setCompanies(res.data?.items || res.data || []);
      } else {
        const res = await apiClient.get('/api/system-admin');
        setAdmins(res.data || []);
      }
    } catch (error) {
      console.error('Помилка завантаження даних:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  // --- ЛОГІКА КОМПАНІЙ ---
  const handleCreateCompany = async (e) => {
    e.preventDefault();
    try {
      const res = await apiClient.post('/api/companies', newCompany);
      
      // Зберігаємо дані (Email та згенерований пароль) для показу у модалці успіху
      setCreatedCredentials({
        email: newCompany.ownerEmail,
        password: res.data.generatedPassword
      });

      // Закриваємо форму створення і очищаємо її
      setIsCompanyModalOpen(false);
      setNewCompany({ companyName: '', ownerFullName: '', ownerEmail: '' });
      fetchData();
    } catch (err) {
      console.error(err);
      alert('Не вдалося створити компанію.');
    }
  };

  const handleToggleCompanyStatus = async (companyId, currentStatus) => {
    if (window.confirm(`Ви впевнені, що хочете ${currentStatus ? 'заблокувати' : 'розблокувати'} цю компанію?`)) {
      try {
        await apiClient.patch('/api/system-admin/companies/status', {
          companyId: companyId,
          isActive: !currentStatus
        });
        fetchData();
      } catch (err) {
        console.error(err);
        alert('Не вдалося змінити статус компанії.');
      }
    }
  };

  // --- ЛОГІКА СИСАДМІНІВ ---
  const handleCreateAdmin = async (e) => {
    e.preventDefault();
    try {
      await apiClient.post('/api/system-admin/register', newAdmin);
      alert('Системного адміністратора успішно створено!');
      setIsAdminModalOpen(false);
      setNewAdmin({ login: '', email: '', password: '', isRoot: false });
      fetchData();
    } catch (err) {
      console.error(err);
      alert('Не вдалося створити адміністратора.');
    }
  };

  // Копіювання пароля
  const handleCopyPassword = () => {
    if (createdCredentials?.password) {
      navigator.clipboard.writeText(createdCredentials.password);
      alert('Пароль скопійовано в буфер обміну!');
    }
  };

  return (
    <div className="w-full h-full flex flex-col relative pb-10">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl text-falcon-dark font-medium">Панель Супер-Адміна</h1>
        
        {activeTab === 'companies' ? (
          <button 
            onClick={() => setIsCompanyModalOpen(true)}
            className="px-6 py-2 bg-falcon-accent text-falcon-dark font-medium rounded-lg hover:brightness-110 transition-all shadow-sm"
          >
            + Нова компанія
          </button>
        ) : (
          <button 
            onClick={() => setIsAdminModalOpen(true)}
            className="px-6 py-2 bg-falcon-dark text-white font-medium rounded-lg hover:brightness-110 transition-all shadow-sm"
          >
            + Новий Сисадмін
          </button>
        )}
      </div>

      {/* Вкладки */}
      <div className="flex gap-4 mb-6 border-b border-gray-200 pb-2">
        <button
          onClick={() => setActiveTab('companies')}
          className={`px-4 py-2 font-medium text-lg transition-colors ${
            activeTab === 'companies' ? 'text-falcon-dark border-b-2 border-falcon-dark' : 'text-gray-400 hover:text-gray-600'
          }`}
        >
          Компанії
        </button>
        <button
          onClick={() => setActiveTab('admins')}
          className={`px-4 py-2 font-medium text-lg transition-colors ${
            activeTab === 'admins' ? 'text-falcon-dark border-b-2 border-falcon-dark' : 'text-gray-400 hover:text-gray-600'
          }`}
        >
          Системні адміністратори
        </button>
      </div>

      {/* КОНТЕНТ: КОМПАНІЇ */}
      {activeTab === 'companies' && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden flex-1 flex flex-col">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm whitespace-nowrap">
              <thead className="bg-falcon-dark text-white">
                <tr>
                  <th className="px-6 py-4 font-medium">Назва компанії</th>
                  <th className="px-6 py-4 font-medium">ПІБ Власника</th>
                  <th className="px-6 py-4 font-medium">Email Власника</th>
                  <th className="px-6 py-4 font-medium">Статус</th>
                  <th className="px-6 py-4 font-medium text-center">Дії</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-gray-700">
                {isLoading ? (
                  <tr><td colSpan="5" className="text-center py-10">Завантаження...</td></tr>
                ) : companies.length === 0 ? (
                  <tr><td colSpan="5" className="text-center py-10">Компаній не знайдено</td></tr>
                ) : (
                  companies.map(company => (
                    <tr key={company.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-6 py-4 font-medium text-falcon-dark">{company.name || company.companyName}</td>
                      <td className="px-6 py-4">{company.ownerFullName || company.ownerName}</td>
                      <td className="px-6 py-4 text-blue-600">{company.ownerEmail || company.email}</td>
                      <td className="px-6 py-4">
                        <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                          company.isActive !== false ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                        }`}>
                          {company.isActive !== false ? 'Активна' : 'Заблокована'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-center">
                        <button 
                          onClick={() => handleToggleCompanyStatus(company.id, company.isActive !== false)}
                          className="text-xs font-medium text-falcon-dark underline hover:text-falcon-accent transition-colors"
                        >
                          {company.isActive !== false ? 'Заблокувати' : 'Розблокувати'}
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* КОНТЕНТ: СИСАДМІНИ */}
      {activeTab === 'admins' && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden flex-1 flex flex-col">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm whitespace-nowrap">
              <thead className="bg-falcon-dark text-white">
                <tr>
                  <th className="px-6 py-4 font-medium">Логін</th>
                  <th className="px-6 py-4 font-medium">Email</th>
                  <th className="px-6 py-4 font-medium">Рівень доступу</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-gray-700">
                {isLoading ? (
                  <tr><td colSpan="3" className="text-center py-10">Завантаження...</td></tr>
                ) : admins.length === 0 ? (
                  <tr><td colSpan="3" className="text-center py-10">Адміністраторів не знайдено</td></tr>
                ) : (
                  admins.map(admin => (
                    <tr key={admin.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-6 py-4 font-medium text-falcon-dark flex items-center gap-3">
                        <div className="w-8 h-8 bg-falcon-accent rounded-full flex items-center justify-center font-bold text-falcon-dark">
                          {admin.login?.charAt(0).toUpperCase() || 'A'}
                        </div>
                        {admin.login}
                      </td>
                      <td className="px-6 py-4">{admin.email}</td>
                      <td className="px-6 py-4">
                        {admin.isRoot ? (
                          <span className="bg-purple-100 text-purple-700 px-3 py-1 rounded-full text-xs font-medium border border-purple-200">
                            Root (Головний)
                          </span>
                        ) : (
                          <span className="bg-blue-100 text-blue-700 px-3 py-1 rounded-full text-xs font-medium border border-blue-200">
                            Системний Адмін
                          </span>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* МОДАЛКА: СТВОРЕННЯ КОМПАНІЇ */}
      {isCompanyModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col">
            <div className="bg-falcon-dark px-6 py-4 flex justify-between items-center">
              <h3 className="text-white text-lg font-medium">Реєстрація компанії</h3>
              <button onClick={() => setIsCompanyModalOpen(false)} className="text-white hover:text-falcon-light text-xl">✕</button>
            </div>
            
            <form onSubmit={handleCreateCompany} className="p-6 flex flex-col gap-4">
              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Назва компанії <span className="text-red-500">*</span></span>
                <input 
                  type="text" required
                  value={newCompany.companyName}
                  onChange={(e) => setNewCompany({...newCompany, companyName: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">ПІБ Власника <span className="text-red-500">*</span></span>
                <input 
                  type="text" required
                  value={newCompany.ownerFullName}
                  onChange={(e) => setNewCompany({...newCompany, ownerFullName: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Email Власника <span className="text-red-500">*</span></span>
                <input 
                  type="email" required
                  value={newCompany.ownerEmail}
                  onChange={(e) => setNewCompany({...newCompany, ownerEmail: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                />
              </label>

              <div className="mt-4 flex justify-end gap-3">
                <button type="button" onClick={() => setIsCompanyModalOpen(false)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-md">Скасувати</button>
                <button type="submit" className="px-6 py-2 bg-falcon-dark text-white rounded-md hover:brightness-110">Створити</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* НОВА МОДАЛКА: ПОКАЗ ЗГЕНЕРОВАНОГО ПАРОЛЮ */}
      {createdCredentials && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col p-8 text-center animate-fade-in">
            <div className="w-16 h-16 bg-green-100 text-green-600 rounded-full flex items-center justify-center mx-auto mb-4 text-3xl shadow-sm">
              ✓
            </div>
            
            <h3 className="text-2xl font-medium text-falcon-dark mb-2">Компанію створено!</h3>
            <p className="text-sm text-gray-500 mb-6">
              Обов'язково збережіть цей пароль та передайте його власнику компанії. <br/>
              <b>З міркувань безпеки він показується лише один раз!</b>
            </p>

            <div className="bg-gray-50 rounded-xl p-5 mb-6 text-left border border-gray-200 shadow-inner">
              <div className="mb-4">
                <span className="text-xs text-gray-400 uppercase font-bold tracking-wider">Логін (Email)</span>
                <div className="font-medium text-falcon-dark mt-1 text-lg">{createdCredentials.email}</div>
              </div>
              
              <div>
                <span className="text-xs text-gray-400 uppercase font-bold tracking-wider">Згенерований пароль</span>
                <div className="font-mono text-xl text-falcon-dark bg-white border border-gray-300 px-4 py-2 mt-1 rounded-lg flex justify-between items-center shadow-sm">
                  <span className="tracking-widest">{createdCredentials.password}</span>
                  <button 
                    onClick={handleCopyPassword}
                    className="text-falcon-dark hover:text-falcon-accent bg-falcon-light/30 p-2 rounded-md transition-colors"
                    title="Скопіювати пароль"
                  >
                    📋
                  </button>
                </div>
              </div>
            </div>

            <button 
              onClick={() => setCreatedCredentials(null)}
              className="w-full py-3 bg-falcon-dark text-white rounded-lg hover:brightness-110 transition-all font-medium text-lg shadow-md"
            >
              Я зберіг пароль, закрити
            </button>
          </div>
        </div>
      )}

      {/* МОДАЛКА: СТВОРЕННЯ СИСАДМІНА */}
      {isAdminModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col">
            <div className="bg-falcon-dark px-6 py-4 flex justify-between items-center">
              <h3 className="text-white text-lg font-medium">Новий Системний Адміністратор</h3>
              <button onClick={() => setIsAdminModalOpen(false)} className="text-white hover:text-falcon-light text-xl">✕</button>
            </div>
            
            <form onSubmit={handleCreateAdmin} className="p-6 flex flex-col gap-4">
              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Логін <span className="text-red-500">*</span></span>
                <input 
                  type="text" required
                  value={newAdmin.login}
                  onChange={(e) => setNewAdmin({...newAdmin, login: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Email <span className="text-red-500">*</span></span>
                <input 
                  type="email" required
                  value={newAdmin.email}
                  onChange={(e) => setNewAdmin({...newAdmin, email: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Пароль <span className="text-red-500">*</span></span>
                <input 
                  type="password" required
                  value={newAdmin.password}
                  onChange={(e) => setNewAdmin({...newAdmin, password: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark outline-none"
                />
              </label>

              <label className="flex items-center gap-2 mt-2 cursor-pointer">
                <input 
                  type="checkbox" 
                  checked={newAdmin.isRoot}
                  onChange={(e) => setNewAdmin({...newAdmin, isRoot: e.target.checked})}
                  className="w-4 h-4 accent-falcon-dark cursor-pointer"
                />
                <span className="text-sm font-medium text-gray-800">Root права (Головний адмін)</span>
              </label>

              <div className="mt-4 flex justify-end gap-3">
                <button type="button" onClick={() => setIsAdminModalOpen(false)} className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-md">Скасувати</button>
                <button type="submit" className="px-6 py-2 bg-falcon-dark text-white rounded-md hover:brightness-110">Створити</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
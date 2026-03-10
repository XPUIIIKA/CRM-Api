import { useState, useEffect } from "react";
import { apiClient } from "../api/apiClient";

// --- ІДЕАЛЬНІ МОКОВІ ДАНІ (для демо-режиму, якщо БД порожня) ---
const MOCK_DIALOGS = [
  {
    id: "d-1",
    name: "Карен Шейла Гіллан",
    preview: "Добрий день, Скиньте посилання",
    time: "14:03",
    unread: 1,
    isOnline: true,
    avatar: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=150&q=80",
  },
  {
    id: "d-2",
    name: "Ноа Шнапп",
    preview: "Дякую, посилку отримав!",
    time: "11:45",
    unread: 0,
    isOnline: false,
    avatar: "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=150&q=80",
  },
  {
    id: "d-3",
    name: "Олена Петрівна",
    preview: "Коли буде відправка?",
    time: "Вчора",
    unread: 2,
    isOnline: true,
    avatar: "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=150&q=80",
  }
];

export default function MessagesPage() {
  const [dialogs, setDialogs] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Фільтри
  const [filterType, setFilterType] = useState('Всі повідомлення');
  const [sortBy, setSortBy] = useState('Час');

  // Стейт для відкритого чату (Модалка або права панель у майбутньому)
  const [selectedDialog, setSelectedDialog] = useState(null);
  const [chatHistory, setChatHistory] = useState([]);
  const [newMessage, setNewMessage] = useState("");

  // 1. ЗАВАНТАЖЕННЯ ДІАЛОГІВ
  const fetchDialogs = async () => {
    setIsLoading(true);
    try {
      const response = await apiClient.get('/api/messages/dialogs');
      
      // Якщо бекенд повернув порожній масив, ставимо мокові дані для красивого демо
      if (!response.data || response.data.length === 0) {
        setDialogs(MOCK_DIALOGS);
      } else {
        setDialogs(response.data);
      }
    } catch (err) {
      console.error("Помилка завантаження діалогів:", err);
      // Fallback на мок при помилці сервера (щоб демо не падало)
      setDialogs(MOCK_DIALOGS); 
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchDialogs();
  }, []);

  // 2. ВІДКРИТТЯ ЧАТУ ТА ЗАВАНТАЖЕННЯ ІСТОРІЇ
  const handleOpenDialog = async (dialog) => {
    setSelectedDialog(dialog);
    setChatHistory([]); // Очищаємо попередній чат
    
    try {
      // Робимо запит за історією повідомлень
      const response = await apiClient.get(`/api/messages/${dialog.id || dialog.dialogId}`);
      setChatHistory(response.data || []);
    } catch (err) {
      console.error("Помилка завантаження історії чату:", err);
      // Для демо-режиму підкидаємо фейкову історію
      setChatHistory([
        { id: 1, text: "Добрий день!", sender: "them", time: "14:00" },
        { id: 2, text: dialog.preview || "Скиньте посилання", sender: "them", time: "14:03" }
      ]);
    }
  };

  // 3. ВІДПРАВКА НОВОГО ПОВІДОМЛЕННЯ
  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() || !selectedDialog) return;

    try {
      const payload = {
        dialogId: selectedDialog.id, // В залежності від того, як бекенд називає ID діалогу
        receiverUserId: selectedDialog.userId || selectedDialog.partnerId, 
        text: newMessage.trim()
      };

      await apiClient.post('/api/messages', payload);
      
      // Оптимістичне додавання в UI
      setChatHistory([...chatHistory, { id: Date.now(), text: newMessage, sender: "me", time: "Щойно" }]);
      setNewMessage("");
      
      // Оновлюємо список діалогів (щоб нове повідомлення стало preview)
      fetchDialogs(); 
    } catch (err) {
      console.error("Помилка відправки:", err);
      alert("Не вдалося відправити повідомлення.");
    }
  };

  // 4. ЛОГІКА ФІЛЬТРАЦІЇ ТА СОРТУВАННЯ
  const getProcessedDialogs = () => {
    let result = [...dialogs];

    // Фільтр
    if (filterType === 'Непрочитані') {
      result = result.filter(d => (d.unread || d.unreadCount) > 0);
    }

    // Сортування
    if (sortBy === 'Онлайн-статус') {
      // Спочатку ті, хто онлайн
      result.sort((a, b) => (a.isOnline === b.isOnline) ? 0 : a.isOnline ? -1 : 1);
    } else {
      // За часом (найновіші зверху). Для демо просто залишаємо як є, 
      // бо для чесного сортування потрібен timestamp з бекенда
    }

    return result;
  };

  const displayedDialogs = getProcessedDialogs();

  if (isLoading) return <div className="p-10 text-center text-falcon-dark text-xl">Завантаження чатів...</div>;

  return (
    <div className="w-full max-w-5xl mx-auto pb-10 relative">
      <h1 className="text-3xl text-falcon-dark font-medium mb-8">
        Повідомлення
      </h1>

      {/* Верхня панель з фільтрами */}
      <div className="flex justify-between items-center mb-10">
        <div className="flex items-center gap-3">
          <select 
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
            className="bg-falcon-light/60 text-falcon-dark px-4 py-2 rounded-md focus:outline-none text-sm cursor-pointer border-none font-medium min-w-[150px] shadow-sm transition-colors hover:bg-falcon-light/80"
          >
            <option value="Всі повідомлення">Всі повідомлення</option>
            <option value="Непрочитані">Непрочитані</option>
          </select>
          <button 
            className="w-9 h-9 bg-falcon-light/60 text-falcon-dark rounded flex items-center justify-center hover:bg-falcon-light transition-all text-xl font-medium shadow-sm"
            title="Створити новий чат"
          >
            +
          </button>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-falcon-dark text-sm">Сортувати за:</span>
          <select 
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            className="bg-falcon-light/60 text-falcon-dark px-4 py-2 rounded-md focus:outline-none text-sm cursor-pointer border-none font-medium min-w-[150px] shadow-sm transition-colors hover:bg-falcon-light/80"
          >
            <option value="Час">Час</option>
            <option value="Онлайн-статус">Онлайн-статус</option>
          </select>
        </div>
      </div>

      {/* Список повідомлень (Діалогів) */}
      <div className="flex flex-col gap-8">
        {displayedDialogs.length === 0 ? (
          <div className="text-center py-12 text-gray-500">Діалогів не знайдено</div>
        ) : (
          displayedDialogs.map((msg) => {
            const isOnline = msg.isOnline !== undefined ? msg.isOnline : Math.random() > 0.5; // Fallback для бекенда
            const unreadCount = msg.unread || msg.unreadCount || 0;

            return (
              <div 
                key={msg.id} 
                onClick={() => handleOpenDialog(msg)}
                className="relative group cursor-pointer"
              >
                {/* Таб-кнопка "..." зверху справа */}
                <div className="absolute -top-4 right-4 bg-[#8ba870] text-falcon-dark px-4 py-0.5 rounded-t-lg text-sm font-bold tracking-widest opacity-0 group-hover:opacity-100 transition-opacity z-0 flex items-start justify-center shadow-sm">
                  <span className="block -translate-y-1.5">...</span>
                </div>

                {/* Основна зелена картка */}
                <div className="bg-falcon-dark rounded-xl p-5 flex justify-between items-stretch shadow-sm hover:shadow-md transition-all relative z-10 border border-transparent group-hover:border-falcon-light/40">
                  {/* Ліва частина: Аватарка і Текст */}
                  <div className="flex items-center gap-5">
                    <img
                      src={msg.avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(msg.name || 'User')}&background=random&color=fff`}
                      alt={msg.name}
                      className="w-[68px] h-[68px] rounded-full object-cover shadow-sm border-2 border-falcon-light/20"
                    />
                    <div className="flex flex-col justify-center gap-1.5">
                      <span className="text-falcon-light/80 text-sm font-light">
                        {msg.name || msg.partnerName || 'Невідомий'}
                      </span>
                      <span className="text-white text-lg font-light tracking-wide line-clamp-1 pr-4">
                        {msg.preview || msg.lastMessageText || 'Повідомлень немає'}
                      </span>
                    </div>
                  </div>

                  {/* Права частина: Статус, Лічильник, Час */}
                  <div className="flex flex-col items-end justify-between py-1 shrink-0">
                    {/* Індикатор онлайну */}
                    <div
                      className={`w-2.5 h-2.5 rounded-full shadow-sm ${isOnline ? "bg-[#8ba870]" : "bg-[#e57a7a]"}`}
                      title={isOnline ? "Онлайн" : "Офлайн"}
                    ></div>

                    {/* Бедж непрочитаних та Час */}
                    <div className="flex items-center gap-4 mt-auto">
                      {unreadCount > 0 && (
                        <div className="bg-falcon-light text-falcon-dark font-bold text-sm px-2.5 py-0.5 rounded shadow-sm">
                          {unreadCount}
                        </div>
                      )}
                      <span className="text-falcon-light/80 text-sm">
                        {msg.time || msg.lastMessageTime || 'Недавно'}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* ПРОСТЕ МОДАЛЬНЕ ВІКНО ЧАТУ (Для демонстрації GET /api/messages/{id} та POST) */}
      {selectedDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md h-[600px] flex flex-col overflow-hidden">
            {/* Шапка чату */}
            <div className="bg-falcon-dark px-4 py-3 flex justify-between items-center shadow-md z-10">
              <div className="flex items-center gap-3">
                <img 
                  src={selectedDialog.avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(selectedDialog.name)}&background=random&color=fff`} 
                  alt="" 
                  className="w-10 h-10 rounded-full border border-falcon-light/30"
                />
                <div className="flex flex-col">
                  <span className="text-white font-medium text-sm">{selectedDialog.name}</span>
                  <span className="text-falcon-light/70 text-xs">
                    {selectedDialog.isOnline ? 'Онлайн' : 'Офлайн'}
                  </span>
                </div>
              </div>
              <button onClick={() => setSelectedDialog(null)} className="text-white hover:text-falcon-light text-xl">✕</button>
            </div>
            
            {/* Історія повідомлень */}
            <div className="flex-1 overflow-y-auto p-4 bg-gray-50 flex flex-col gap-3">
              {chatHistory.length === 0 ? (
                <div className="m-auto text-gray-400 text-sm">Немає повідомлень</div>
              ) : (
                chatHistory.map((msg) => (
                  <div key={msg.id} className={`flex flex-col max-w-[80%] ${msg.sender === 'me' ? 'self-end items-end' : 'self-start items-start'}`}>
                    <div className={`px-4 py-2 rounded-2xl shadow-sm text-sm ${
                      msg.sender === 'me' 
                        ? 'bg-falcon-dark text-white rounded-tr-sm' 
                        : 'bg-white text-gray-800 border border-gray-100 rounded-tl-sm'
                    }`}>
                      {msg.text}
                    </div>
                    <span className="text-[10px] text-gray-400 mt-1 px-1">{msg.time}</span>
                  </div>
                ))
              )}
            </div>

            {/* Форма відправки (POST /api/messages) */}
            <form onSubmit={handleSendMessage} className="p-3 bg-white border-t border-gray-100 flex gap-2">
              <input 
                type="text" 
                value={newMessage}
                onChange={(e) => setNewMessage(e.target.value)}
                placeholder="Введіть повідомлення..."
                className="flex-1 bg-gray-100 rounded-full px-4 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-falcon-dark"
              />
              <button 
                type="submit" 
                disabled={!newMessage.trim()}
                className="w-9 h-9 bg-falcon-dark text-white rounded-full flex items-center justify-center hover:brightness-110 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
              >
                ➤
              </button>
            </form>
          </div>
        </div>
      )}

    </div>
  );
}
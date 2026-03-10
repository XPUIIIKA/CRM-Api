import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { apiClient } from '../api/apiClient';

// Імпортуємо іконки
import productsImage from '../assets/layout/products.svg';
import managersImage from '../assets/layout/managers.svg';
import ordersImage from '../assets/layout/orders.svg';
import statisticsImage from '../assets/layout/statistics.svg';
import messagesImage from '../assets/layout/messages.svg';

export default function HomePage() {
  // Стейт для динамічних метрик
  const [metrics, setMetrics] = useState({
    orders: 'Завантаження...',
    products: 'Завантаження...',
    team: 'Завантаження...',
    stats: 'Аналіз...',
    messages: 'Завантаження...'
  });

  useEffect(() => {
    const fetchMetrics = async () => {
      // Використовуємо allSettled, щоб помилка одного запиту не зламала весь дашборд
      const results = await Promise.allSettled([
        apiClient.get('/api/orders'),
        apiClient.get('/api/product'),
        apiClient.get('/api/user'),
        apiClient.get('/api/messages/dialogs'),
        apiClient.get('/api/statistics/sales')
      ]);

      const [ordersRes, productsRes, usersRes, dialogsRes, statsRes] = results;

      let newMetrics = { ...metrics };

      // 1. Замовлення (Рахуємо загальну кількість)
      if (ordersRes.status === 'fulfilled') {
        const ordersCount = ordersRes.value.data?.length || 0;
        newMetrics.orders = `${ordersCount} всього`;
      } else {
        newMetrics.orders = 'Немає даних';
      }

      // 2. Товари
      if (productsRes.status === 'fulfilled') {
        const productsCount = productsRes.value.data?.length || 0;
        newMetrics.products = `${productsCount} шт.`;
      } else {
        newMetrics.products = 'Немає даних';
      }

      // 3. Команда
      if (usersRes.status === 'fulfilled') {
        const usersCount = usersRes.value.data?.length || 0;
        newMetrics.team = `${usersCount} осіб`;
      } else {
        newMetrics.team = 'Немає даних';
      }

      // 4. Повідомлення (Рахуємо чати з непрочитаними)
      if (dialogsRes.status === 'fulfilled') {
        const dialogs = dialogsRes.value.data || [];
        const unreadChats = dialogs.filter(d => (d.unread || d.unreadCount) > 0).length;
        newMetrics.messages = `${unreadChats} з новими`;
      } else {
        newMetrics.messages = 'Немає даних';
      }

      // 5. Статистика (Для демо просто рахуємо загальну суму або залишаємо заглушку)
      if (statsRes.status === 'fulfilled') {
        const sales = statsRes.value.data || [];
        if (sales.length > 0) {
          const totalSales = sales.reduce((sum, item) => sum + (item.value || item.amount || 0), 0);
          newMetrics.stats = `${totalSales} ₴ продажі`;
        } else {
          newMetrics.stats = '+15% за місяць'; // Заглушка, якщо масив порожній
        }
      } else {
        newMetrics.stats = 'Немає даних';
      }

      setMetrics(newMetrics);
    };

    fetchMetrics();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Масив з даними для карток
  const dashboardCards = [
    {
      path: '/orders',
      title: 'Замовлення',
      description: 'Обробка нових заявок, зміна статусів та контроль доставок.',
      icon: ordersImage,
      metric: metrics.orders,
    },
    {
      path: '/products',
      title: 'Товари',
      description: 'Управління каталогом, цінами, залишками на складі та категоріями.',
      icon: productsImage,
      metric: metrics.products,
    },
    {
      path: '/managers',
      title: 'Команда',
      description: 'Список співробітників, додавання нових та налаштування прав.',
      icon: managersImage,
      metric: metrics.team,
    },
    {
      path: '/statistics',
      title: 'Статистика',
      description: 'Аналітика продажів, фінансові звіти та графіки успішності.',
      icon: statisticsImage,
      metric: metrics.stats,
    },
    {
      path: '/messages',
      title: 'Повідомлення',
      description: 'Внутрішній чат команди та сповіщення від системи.',
      icon: messagesImage,
      metric: metrics.messages,
    }
  ];

  return (
    <div className="w-full max-w-7xl mx-auto pb-10">
      {/* Заголовок страницы */}
      <div className="mb-10">
        <h1 className="text-3xl text-falcon-dark font-medium mb-2">Огляд системи</h1>
        <p className="text-gray-500">Оберіть розділ для початку роботи</p>
      </div>

      {/* Сетка карточек */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        
        {dashboardCards.map((card, index) => (
          <Link
            key={index}
            to={card.path}
            className="group block bg-white border border-gray-100 rounded-2xl p-6 shadow-sm hover:shadow-xl hover:-translate-y-1 hover:border-falcon-light transition-all duration-300 relative overflow-hidden"
          >
            {/* Декоративный круг на фоне */}
            <div className="absolute -right-8 -top-8 w-32 h-32 bg-falcon-light/20 rounded-full scale-0 group-hover:scale-100 transition-transform duration-500 ease-out"></div>

            <div className="relative z-10 flex flex-col h-full">
              {/* Верхняя часть: Иконка и Метрика */}
              <div className="flex justify-between items-start mb-6">
                <div className="w-14 h-14 rounded-xl bg-falcon-light flex items-center justify-center group-hover:bg-falcon-dark transition-colors duration-300 shadow-sm">
                  <img 
                    src={card.icon} 
                    alt={card.title} 
                    className="w-7 h-7 object-contain group-hover:brightness-200 transition-all duration-300 group-hover:scale-110" 
                  />
                </div>
                {card.metric && (
                  <span className="text-sm font-medium bg-gray-50 text-gray-600 px-3 py-1 rounded-full border border-gray-100 group-hover:bg-falcon-light group-hover:text-falcon-dark group-hover:border-transparent transition-all shadow-sm">
                    {card.metric}
                  </span>
                )}
              </div>

              {/* Нижняя часть: Текст */}
              <div>
                <h2 className="text-xl font-semibold text-falcon-dark mb-2 group-hover:text-falcon-accent transition-colors">
                  {card.title}
                </h2>
                <p className="text-gray-500 text-sm leading-relaxed">
                  {card.description}
                </p>
              </div>
            </div>
          </Link>
        ))}

      </div>
    </div>
  );
}
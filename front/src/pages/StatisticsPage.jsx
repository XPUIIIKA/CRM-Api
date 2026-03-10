import { useState, useEffect } from 'react';
// import { apiClient } from '../api/apiClient'; // Закоментовано для демо

const CHART_COLORS = ['#F6B97A', '#CAFF7A', '#E57AFF', '#7AFFFF', '#897AFF', '#CC8B7A', '#7ACA95', '#8B7AFF'];

// --- МОКОВІ ДАНІ ДЛЯ ДЕМОНСТРАЦІЇ ---
const MOCK_CHANNELS = [
  { id: 'ch1', name: 'Instagram', value: 51.3 },
  { id: 'ch2', name: 'Prom', value: 16.6 },
  { id: 'ch3', name: 'Facebook', value: 16.3 },
  { id: 'ch4', name: 'Rozetka', value: 11.4 },
  { id: 'ch5', name: 'Google', value: 4.7 }
];

const MOCK_SALES = [
  { label: 'Січень', value: 70 },
  { label: 'Лютий', value: 50 },
  { label: 'Березень', value: 35 },
  { label: 'Квітень', value: 80 },
  { label: 'Травень', value: 55 }
];

const MOCK_CATEGORIES = [
  { id: 'cat1', name: 'Джем (Апельсиновий)', value: 51.3, color: '#CC8B7A' },
  { id: 'cat2', name: 'Пастіла (вишнева)', value: 26.7, color: '#7ACA95' },
  { id: 'cat3', name: 'Пастіла (мікс)', value: 22.0, color: '#8B7AFF' },
  { id: 'cat4', name: 'Соки (Яблучний)', value: 15.5, color: '#F6B97A' }
];

export default function StatisticsPage() {
  const [isLoading, setIsLoading] = useState(true);

  // Стейти для даних
  const [salesData, setSalesData] = useState([]);
  const [channelsData, setChannelsData] = useState([]);
  const [categoriesData, setCategoriesData] = useState([]);

  const [catFilterType, setCatFilterType] = useState('Кількість упаковок');
  const [activeCategoryIds, setActiveCategoryIds] = useState([]);

  useEffect(() => {
    // Імітація завантаження з API (півсекунди для красивої анімації появи)
    const fetchStatistics = () => {
      setIsLoading(true);
      setTimeout(() => {
        /* * КОЛИ БЕКЕНД БУДЕ ГОТОВИЙ, РОЗКОМЕНТУЙ ЦЕ:
         * const [salesRes, channelsRes, categoriesRes] = await Promise.all([
         * apiClient.get('/api/statistics/sales'),
         * apiClient.get('/api/statistics/channels'),
         * apiClient.get('/api/statistics/categories')
         * ]);
         * setSalesData(salesRes.data); ...
         */

        // А поки використовуємо мокові дані:
        setSalesData(MOCK_SALES);
        setChannelsData(MOCK_CHANNELS);
        setCategoriesData(MOCK_CATEGORIES);
        
        // Робимо перші 3 категорії активними за замовчуванням (як у дизайні)
        setActiveCategoryIds(['cat1', 'cat2', 'cat3']);
        setIsLoading(false);
      }, 500);
    };

    fetchStatistics();
  }, []);

  // Хелпер для кругових діаграм
  const generatePieChartStyle = (dataItems) => {
    if (!dataItems || dataItems.length === 0) return { background: '#f8f9fa' };

    const total = dataItems.reduce((sum, item) => sum + (item.value || 0), 0);
    if (total === 0) return { background: '#f8f9fa' };

    let currentPercent = 0;
    const gradientStops = dataItems.map((item, index) => {
      const percent = ((item.value || 0) / total) * 100;
      const start = currentPercent;
      currentPercent += percent;
      const color = item.color || CHART_COLORS[index % CHART_COLORS.length];
      return `${color} ${start}% ${currentPercent}%`;
    });

    return { background: `conic-gradient(${gradientStops.join(', ')})` };
  };

  const maxSalesValue = salesData.length > 0 ? Math.max(...salesData.map(s => s.value || 0)) : 100;

  const toggleCategoryFilter = (id) => {
    setActiveCategoryIds(prev => 
      prev.includes(id) ? prev.filter(catId => catId !== id) : [...prev, id]
    );
  };

  const filteredCategoriesData = categoriesData.filter(c => activeCategoryIds.includes(c.id));

  if (isLoading) return <div className="p-10 text-center text-falcon-dark text-xl">Формування звіту...</div>;

  return (
    <div className="w-full h-full flex flex-col pb-10">
      <h1 className="text-3xl text-falcon-dark font-medium mb-8">Статистика</h1>

      {/* Верхній ряд */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        
        {/* 1. Канали збуту */}
        <div className="bg-falcon-dark rounded-2xl p-6 relative flex flex-col text-white shadow-md border border-falcon-light/10">
          <div className="flex justify-between items-center mb-6 z-10">
            <h2 className="text-xl font-medium">Канали збуту</h2>
            <select className="bg-transparent border-none text-sm text-falcon-light focus:outline-none cursor-pointer appearance-none">
              <option className="text-black">цей рік</option>
              <option className="text-black">минулий рік</option>
            </select>
          </div>

          <div className="flex items-center justify-between px-4 flex-1">
            <div 
              className="relative w-48 h-48 rounded-full shadow-lg transition-all duration-700" 
              style={generatePieChartStyle(channelsData)}
            >
              {/* Відсотки поверх діаграми (приблизно як у макеті) */}
              <span className="absolute top-1/2 right-1/4 text-xs font-bold text-black drop-shadow-md">51%</span>
              <span className="absolute bottom-1/4 left-1/3 text-xs font-bold text-black drop-shadow-md">17%</span>
              <span className="absolute top-1/4 left-1/2 -translate-x-1/2 text-xs font-bold text-black drop-shadow-md">16%</span>
            </div>

            <div className="space-y-3 pl-8">
              {channelsData.map((item, i) => (
                <div key={item.id} className="flex items-center gap-3">
                  <div className="w-8 h-4 rounded-full shadow-sm" style={{ backgroundColor: CHART_COLORS[i % CHART_COLORS.length] }}></div>
                  <span className="text-sm font-light text-falcon-light/90">- {item.name}</span>
                </div>
              ))}
            </div>
          </div>
          <div className="absolute bottom-4 right-6 text-falcon-light/50">→</div>
        </div>

        {/* 2. Продажі % */}
        <div className="bg-falcon-dark rounded-2xl p-6 relative flex flex-col text-white shadow-md border border-falcon-light/10">
          <div className="flex justify-between items-center mb-6 z-10">
            <h2 className="text-xl font-medium">Продажі %</h2>
            <select className="bg-transparent border-none text-sm text-falcon-light focus:outline-none cursor-pointer appearance-none">
              <option className="text-black">цей рік</option>
              <option className="text-black">минулий рік</option>
            </select>
          </div>

          <div className="flex items-end justify-between px-2 flex-1 relative">
            <div className="absolute inset-0 flex flex-col justify-between py-4 pl-28 pr-4 pointer-events-none">
              {[...Array(5)].map((_, i) => <div key={i} className="w-full border-t border-falcon-light/20"></div>)}
              <div className="w-full border-t border-falcon-light/60 mt-1"></div>
            </div>

            <div className="space-y-4 mb-4 z-10 w-28 pr-4">
              {salesData.map((item, i) => (
                <div key={i} className="flex items-center justify-between gap-2">
                  <span className="text-xs font-light">{item.label}</span>
                  <div className="w-3 h-1 rounded-sm" style={{ backgroundColor: CHART_COLORS[i % CHART_COLORS.length] }}></div>
                </div>
              ))}
            </div>

            <div className="flex gap-4 items-end h-[160px] pb-[17px] z-10 pr-10 flex-1 justify-around">
              {salesData.map((item, i) => {
                const heightPercent = Math.max((item.value / maxSalesValue) * 100, 5);
                return (
                  <div 
                    key={i} 
                    className="w-10 rounded-t-sm transition-all duration-1000 ease-out group relative cursor-pointer" 
                    style={{ height: `${heightPercent}%`, backgroundColor: CHART_COLORS[i % CHART_COLORS.length] }}
                  >
                    <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-white text-falcon-dark text-xs font-bold px-2 py-1 rounded opacity-0 group-hover:opacity-100 transition-opacity shadow-lg">
                      {item.value}%
                    </span>
                  </div>
                );
              })}
            </div>
          </div>
          <div className="absolute bottom-4 right-6 text-falcon-light/50">→</div>
        </div>
      </div>

      {/* Нижній блок: Продажі за категоріями */}
      <div className="bg-falcon-light/20 rounded-2xl p-8 border border-falcon-light/40 shadow-sm relative">
        <div className="flex justify-between items-center mb-8">
          <h2 className="text-2xl text-falcon-dark font-medium">Продажі за категоріями</h2>
          <div className="flex items-center gap-4">
            <select 
              value={catFilterType}
              onChange={(e) => setCatFilterType(e.target.value)}
              className="bg-[#b6c7a3] text-falcon-dark px-4 py-1.5 rounded-md focus:outline-none text-sm cursor-pointer border-none font-medium appearance-none shadow-sm"
            >
              <option>Кількість упаковок</option>
              <option>Сума продажів</option>
            </select>
          </div>
        </div>

        <div className="flex justify-between items-start">
          {/* Чекбокси */}
          <div className="w-64 space-y-4">
            <h3 className="text-xl text-falcon-dark font-medium mb-4">Категорії</h3>
            
            <div className="space-y-3">
              {categoriesData.map(cat => (
                <div key={cat.id} className="flex justify-between items-center text-falcon-dark font-medium text-base hover:bg-white/50 p-1.5 rounded transition-colors cursor-pointer" onClick={() => toggleCategoryFilter(cat.id)}>
                  <span className="truncate pr-2">{cat.name}</span>
                  <input 
                    type="checkbox" 
                    checked={activeCategoryIds.includes(cat.id)} 
                    readOnly
                    className="w-4 h-4 accent-falcon-dark cursor-pointer rounded-sm shrink-0" 
                  />
                </div>
              ))}
            </div>
          </div>

          {/* Велика діаграма */}
          <div className="flex-1 flex justify-center items-center">
            <div 
              className="relative w-80 h-80 rounded-full shadow-xl border-4 border-white transition-all duration-700 flex items-center justify-center" 
              style={generatePieChartStyle(filteredCategoriesData)}
            >
              {filteredCategoriesData.length === 0 && <span className="text-falcon-dark/50 font-medium">Оберіть категорію</span>}
              <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-4 h-4 bg-falcon-dark rounded-full shadow-inner"></div>
            </div>
          </div>

          {/* Легенда великої діаграми */}
          <div className="w-64 flex flex-col justify-center space-y-6 pt-4">
            {filteredCategoriesData.map((item) => (
              <div key={item.id} className="flex items-center gap-4 animate-fade-in">
                <div className="w-10 h-6 rounded-full shadow-sm shrink-0" style={{ backgroundColor: item.color || '#333' }}></div>
                <div className="flex flex-col">
                  <span className="text-sm font-medium text-falcon-dark/90 leading-tight">
                    - {item.name}
                  </span>
                  <span className="text-xs text-falcon-dark/60 font-bold mt-1">
                    {item.value} {catFilterType === 'Сума продажів' ? '₴' : '%'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
import { useState, useEffect } from 'react';
import { apiClient } from '../api/apiClient';

export default function OrdersPage() {
  const [activeTabId, setActiveTabId] = useState('ALL');

  // Основні дані з АПІ
  const [orders, setOrders] = useState([]);
  const [managers, setManagers] = useState([]);
  const [statuses, setStatuses] = useState([]);
  const [clients, setClients] = useState([]);
  const [products, setProducts] = useState([]);
  
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Стейти для модального вікна замовлення
  const [isOrderModalOpen, setIsOrderModalOpen] = useState(false);
  const [editingOrder, setEditingOrder] = useState(null);
  
  const [orderForm, setOrderForm] = useState({
    clientId: '',
    deliveryAddress: '',
    salesChannel: '',
    notes: '',
    statusId: '',
    managerId: '',
    items: [] 
  });

  const getStatusColor = (statusName) => {
    const name = statusName?.toLowerCase() || '';
    if (name.includes('нов')) return 'bg-green-100 text-green-700 border border-green-200';
    if (name.includes('недозвон')) return 'bg-orange-100 text-orange-700 border border-orange-200';
    if (name.includes('дорозі') || name.includes('відправлен')) return 'bg-blue-100 text-blue-700 border border-blue-200';
    if (name.includes('виконан') || name.includes('успіш')) return 'bg-gray-100 text-gray-700 border border-gray-200';
    if (name.includes('скасован') || name.includes('відмов')) return 'bg-red-100 text-red-700 border border-red-200';
    return 'bg-falcon-light/30 text-falcon-dark border border-falcon-light/50'; 
  };

  const getClientName = (order) => {
    if (order.client?.firstName) return `${order.client.firstName} ${order.client.surname || ''}`.trim();
    if (order.client?.name) return order.client.name;
    return order.clientFullName || order.clientName || 'Невідомий клієнт';
  };

  const getClientPhone = (order) => {
    if (order.client?.phone) return order.client.phone;
    return order.clientPhone || order.phoneNumber || '';
  };

  // --- МАКСИМАЛЬНО ЖОРСТКІ ФУНКЦІЇ ОТРИМАННЯ ID (ДОДАНО currentStatusId) ---
  const getSafeStatusId = (order) => {
    if (!order) return '';
    // Тепер ми знаємо, що бекенд віддає CurrentStatusId !
    const id = order.currentStatusId || order.CurrentStatusId || order.statusId || order.StatusId || order.status?.id || '';
    return id ? String(id).toLowerCase().trim() : '';
  };

  const getSafeManagerId = (order) => {
    if (!order) return '';
    const id = order.assignedManagerId || order.AssignedManagerId || order.managerId || order.ManagerId || order.manager?.id || '';
    return id ? String(id).toLowerCase().trim() : '';
  };

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [ordersRes, usersRes, statusesRes, clientsRes, productsRes] = await Promise.all([
        apiClient.get('/api/orders'),
        apiClient.get('/api/user'),
        apiClient.get('/api/statuses'),
        apiClient.get('/api/clients'),
        apiClient.get('/api/product')
      ]);
      
      setOrders(ordersRes.data?.items || ordersRes.data || []);
      setManagers(usersRes.data || []);
      setStatuses(statusesRes.data || []);
      setClients(clientsRes.data || []);
      setProducts(productsRes.data || []);
    } catch (err) {
      console.error('Помилка завантаження даних:', err);
      setError('Не вдалося завантажити дані.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleAddStatus = async () => {
    const name = window.prompt('Введіть назву нового статусу:');
    if (!name || name.trim() === '') return;
    try {
      await apiClient.post('/api/statuses', { name: name.trim() });
      await fetchData(); 
    } catch (error) {
      alert('Не вдалося створити статус.');
    }
  };

  const handleDeleteStatus = async (e, id, name) => {
    e.stopPropagation();
    if (window.confirm(`Видалити статус "${name}"?`)) {
      try {
        await apiClient.delete(`/api/statuses/${id}`);
        if (activeTabId === id) setActiveTabId('ALL');
        await fetchData();
      } catch (error) {
        alert('Не вдалося видалити статус.');
      }
    }
  };

  const handleManagerChange = async (orderId, newManagerId) => {
    const previousOrders = [...orders];
    setOrders(prev => prev.map(o => o.id === orderId ? { ...o, assignedManagerId: newManagerId } : o));

    try {
      const payload = { managerId: newManagerId === "" ? null : newManagerId };
      await apiClient.patch(`/api/orders/${orderId}/assign`, payload);
    } catch (error) {
      alert('Не вдалося змінити менеджера.');
      setOrders(previousOrders);
    }
  };

  const handleStatusChange = async (orderId, newStatusId) => {
    if (!newStatusId) return;
    const previousOrders = [...orders];
    setOrders(prev => prev.map(o => o.id === orderId ? { ...o, currentStatusId: newStatusId } : o));

    try {
      await apiClient.patch(`/api/orders/${orderId}/status`, { statusId: newStatusId });
    } catch (error) {
      alert('Не вдалося змінити статус.');
      setOrders(previousOrders);
    }
  };

  const handleDeleteOrder = async (orderId) => {
    if (window.confirm('Ви впевнені, що хочете видалити це замовлення?')) {
      try {
        await apiClient.delete(`/api/orders/${orderId}`);
        await fetchData();
      } catch (error) {
        alert('Не вдалося видалити замовлення.');
      }
    }
  };

  const openOrderModal = (order = null) => {
    setEditingOrder(order);
    if (order) {
      setOrderForm({
        clientId: order.clientId || order.client?.id || '',
        deliveryAddress: order.deliveryAddress || order.client?.address || '',
        salesChannel: order.salesChannel || '',
        notes: order.notes || '',
        managerId: getSafeManagerId(order),
        statusId: getSafeStatusId(order),
        items: order.items?.map(i => ({ 
          productId: i.productId, 
          quantity: i.quantity, 
          priceAtOrder: i.priceAtOrder 
        })) || []
      });
    } else {
      const defaultStatus = statuses.length > 0 ? String(statuses[0].id || statuses[0].Id).toLowerCase().trim() : '';
      setOrderForm({ 
        clientId: '', deliveryAddress: '', salesChannel: '', notes: '', 
        statusId: defaultStatus, managerId: '', items: [] 
      });
    }
    setIsOrderModalOpen(true);
  };

  const handleSaveOrder = async (e) => {
    e.preventDefault();
    if (!orderForm.clientId) {
      alert('Оберіть клієнта!');
      return;
    }

    try {
      const payload = {
        clientId: orderForm.clientId,
        deliveryAddress: orderForm.deliveryAddress,
        salesChannel: orderForm.salesChannel,
        notes: orderForm.notes,
        statusId: orderForm.statusId === "" ? null : orderForm.statusId,
        managerId: orderForm.managerId === "" ? null : orderForm.managerId
      };

      if (editingOrder) {
        await apiClient.put(`/api/orders/${editingOrder.id}`, payload);
        if (orderForm.items.length > 0) {
           await apiClient.patch(`/api/orders/${editingOrder.id}/items`, orderForm.items);
        }
      } else {
        await apiClient.post('/api/orders', { ...payload, items: orderForm.items });
      }
      
      setIsOrderModalOpen(false);
      await fetchData(); 
    } catch (error) {
      console.error('Помилка збереження замовлення:', error);
      alert('Помилка при збереженні замовлення. Перевірте дані.');
    }
  };

  const handleAddQuickClient = async () => {
    const name = window.prompt("Введіть ім'я нового клієнта:");
    if (!name) return;
    try {
      const res = await apiClient.post('/api/clients', { firstName: name.trim() });
      await fetchData(); 
      if (res.data?.id) {
        setOrderForm(prev => ({ ...prev, clientId: res.data.id }));
      }
    } catch (err) {
      alert('Не вдалося створити клієнта.');
    }
  };

  const handleAddItem = () => {
    setOrderForm({
      ...orderForm,
      items: [...orderForm.items, { productId: '', quantity: 1, priceAtOrder: 0 }]
    });
  };

  const handleRemoveItem = (index) => {
    const newItems = [...orderForm.items];
    newItems.splice(index, 1);
    setOrderForm({ ...orderForm, items: newItems });
  };

  const handleItemChange = (index, field, value) => {
    const newItems = [...orderForm.items];
    newItems[index][field] = value;
    if (field === 'productId') {
      const selectedProduct = products.find(p => p.id === value);
      if (selectedProduct) {
        newItems[index].priceAtOrder = selectedProduct.price;
      }
    }
    setOrderForm({ ...orderForm, items: newItems });
  };

  const displayedOrders = activeTabId === 'ALL' 
    ? orders 
    : orders.filter(o => getSafeStatusId(o) === String(activeTabId).toLowerCase().trim());

  if (isLoading) return <div className="p-8 text-center text-falcon-dark">Завантаження замовлень...</div>;
  if (error) return <div className="p-8 text-center text-red-500">{error}</div>;

  return (
    <div className="w-full h-full flex flex-col relative pb-10">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl text-falcon-dark font-medium">Замовлення</h1>
        <div className="flex gap-4 items-center">
          <button onClick={() => openOrderModal()} className="h-9 px-4 bg-falcon-accent text-falcon-dark font-medium rounded hover:brightness-110 transition-all flex items-center gap-2 text-sm shadow-sm">
            <span className="text-lg leading-none">+</span> Нове замовлення
          </button>
          <input type="text" placeholder="Пошук по ТТН..." className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-4 py-2 rounded-md w-64 focus:outline-none focus:ring-1 focus:ring-falcon-dark text-sm" />
        </div>
      </div>

      <div className="flex gap-2 mb-4 overflow-x-auto pb-2 items-center scrollbar-hide">
        <button onClick={() => setActiveTabId('ALL')} className={`whitespace-nowrap px-4 py-1.5 rounded-full text-sm font-medium transition-colors ${activeTabId === 'ALL' ? 'bg-falcon-dark text-white shadow-md' : 'bg-white text-falcon-dark border border-falcon-light'}`}>
          Всі <span className="text-xs opacity-70 ml-1">{orders.length}</span>
        </button>

        {statuses.map(status => {
          const statusIdClean = String(status.id || status.Id).toLowerCase().trim();
          const count = orders.filter(o => getSafeStatusId(o) === statusIdClean).length;
          
          return (
            <div key={status.id} className="relative group flex items-center">
              <button onClick={() => setActiveTabId(statusIdClean)} className={`whitespace-nowrap px-4 py-1.5 rounded-full text-sm font-medium transition-colors flex items-center gap-2 ${String(activeTabId).toLowerCase().trim() === statusIdClean ? 'bg-falcon-dark text-white shadow-md' : 'bg-white text-falcon-dark border border-falcon-light'}`}>
                {status.name} <span className="text-xs opacity-70">{count}</span>
              </button>
              <button onClick={(e) => handleDeleteStatus(e, status.id, status.name)} className="absolute -top-1 -right-1 w-4 h-4 bg-red-100 text-red-600 rounded-full flex items-center justify-center text-[10px] opacity-0 group-hover:opacity-100 hover:bg-red-500 hover:text-white">✕</button>
            </div>
          );
        })}
        <button onClick={handleAddStatus} className="w-8 h-8 min-w-[32px] rounded-full border border-dashed border-falcon-dark/50 text-falcon-dark flex items-center justify-center hover:bg-falcon-light/30">+</button>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 flex-1 overflow-hidden flex flex-col">
        <div className="overflow-x-auto flex-1">
          <table className="w-full text-left text-sm whitespace-nowrap min-w-[1000px]">
            <thead className="bg-falcon-dark text-white sticky top-0 z-10 shadow-sm">
              <tr>
                <th className="px-4 py-3 font-medium w-12"><input type="checkbox" className="rounded accent-falcon-accent" /></th>
                <th className="px-4 py-3 font-medium">№ / Дата</th>
                <th className="px-4 py-3 font-medium">Клієнт / Доставка</th>
                <th className="px-4 py-3 font-medium">Склад замовлення</th>
                <th className="px-4 py-3 font-medium text-right">Сума</th>
                <th className="px-4 py-3 font-medium">Менеджер</th>
                <th className="px-4 py-3 font-medium">Статус</th>
                <th className="px-4 py-3 font-medium text-center">Дії</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 text-gray-700">
              {displayedOrders.length === 0 ? (
                <tr><td colSpan="8" className="text-center py-10 text-gray-500">Замовлень не знайдено</td></tr>
              ) : (
                displayedOrders.map((order) => {
                  const safeStatusId = getSafeStatusId(order);
                  const safeManagerId = getSafeManagerId(order);

                  // Шукаємо статус у нашому загальному списку
                  const matchedStatus = statuses.find(s => 
                    String(s.id || s.Id).toLowerCase().trim() === safeStatusId
                  );
                  
                  // Розумний Fallback: якщо не знайшли ID, але бекенд прислав назву — беремо її!
                  const statusNameToDisplay = matchedStatus?.name || order.currentStatusName || order.CurrentStatusName || 'Невідомий';
                  const statusColor = getStatusColor(statusNameToDisplay);
                  
                  const clientName = getClientName(order);
                  const clientPhone = getClientPhone(order);

                  return (
                    <tr key={order.id} className="hover:bg-gray-50 transition-colors group">
                      <td className="px-4 py-3 align-top"><input type="checkbox" className="rounded accent-falcon-accent mt-1" /></td>
                      <td className="px-4 py-3 align-top">
                        <div className="font-semibold text-falcon-dark" title={order.id}>{order.id?.substring(0,8) || 'Нове'}</div>
                        <div className="text-xs text-gray-400 mt-1">{new Date(order.createdAt || Date.now()).toLocaleDateString('uk-UA')}</div>
                      </td>
                      <td className="px-4 py-3 align-top whitespace-normal min-w-[200px]">
                        <div className="font-medium text-falcon-dark">{clientName}</div>
                        {clientPhone && <div className="text-xs text-blue-600 hover:underline cursor-pointer">{clientPhone}</div>}
                        <div className="text-xs text-gray-500 mt-1 line-clamp-2">{order.deliveryAddress || order.client?.address || 'Адресу не вказано'}</div>
                      </td>
                      <td className="px-4 py-3 align-top whitespace-normal min-w-[200px]">
                        <ul className="text-xs space-y-1">
                          {(order.items || []).map((item, i) => (
                            <li key={i} className="flex justify-between items-center gap-4 bg-gray-100 px-2 py-1 rounded">
                              <span className="text-falcon-dark truncate" title={item.productName || 'Товар'}>{item.productName || 'Товар'} ({item.quantity} шт.)</span>
                              <span className="font-medium text-gray-600 shrink-0">{item.priceAtOrder} ₴</span>
                            </li>
                          ))}
                        </ul>
                      </td>
                      <td className="px-4 py-3 align-top text-right font-medium text-falcon-dark">{order.totalAmount || order.total || 0} ₴</td>
                      <td className="px-4 py-3 align-top">
                        <select 
                          value={safeManagerId}
                          onChange={(e) => handleManagerChange(order.id, e.target.value)}
                          className="text-xs border border-gray-300 rounded-md px-2 py-1.5 bg-white focus:outline-none focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark w-full max-w-[140px] cursor-pointer"
                        >
                          <option value="">Не призначено</option>
                          {managers.map(m => {
                            const val = String(m.id || m.Id).toLowerCase().trim();
                            return <option key={val} value={val}>{m.fullName || m.login || m.email}</option>
                          })}
                        </select>
                      </td>
                      <td className="px-4 py-3 align-top">
                        <select 
                          value={safeStatusId}
                          onChange={(e) => handleStatusChange(order.id, e.target.value)}
                          className={`text-xs font-medium rounded px-2 py-1.5 focus:outline-none w-full max-w-[130px] cursor-pointer ${statusColor}`}
                        >
                          <option value="" disabled hidden>Оберіть статус</option>
                          
                          {/* Якщо бекенд прислав статус, якого чомусь немає в нашому масиві, ми додаємо його як опцію */}
                          {safeStatusId && !matchedStatus && (
                             <option value={safeStatusId} className="bg-white text-gray-800">{statusNameToDisplay}</option>
                          )}
                          
                          {statuses.map(s => {
                            const val = String(s.id || s.Id).toLowerCase().trim();
                            return <option key={val} value={val} className="bg-white text-gray-800">{s.name}</option>
                          })}
                        </select>
                      </td>
                      <td className="px-4 py-3 align-top text-center text-gray-400">
                        <button onClick={() => openOrderModal(order)} className="hover:text-falcon-dark px-1 mr-2 text-lg transition-colors">✎</button>
                        <button onClick={() => handleDeleteOrder(order.id)} className="hover:text-red-500 px-1 text-lg transition-colors">✕</button>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* МОДАЛКА */}
      {isOrderModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-hidden flex flex-col animate-fade-in">
            <div className="bg-falcon-dark px-6 py-4 flex justify-between items-center shrink-0">
              <h3 className="text-white text-lg font-medium">{editingOrder ? `Редагування замовлення` : 'Нове замовлення'}</h3>
              <button onClick={() => setIsOrderModalOpen(false)} className="text-white hover:text-falcon-light text-xl transition-colors">✕</button>
            </div>
            
            <form onSubmit={handleSaveOrder} className="p-6 overflow-y-auto flex-1 flex flex-col gap-5">
              <div className="grid grid-cols-2 gap-6">
                <label className="flex flex-col">
                  <span className="text-sm font-medium text-gray-700 mb-1 flex justify-between">
                    <span>Клієнт <span className="text-red-500">*</span></span>
                    <button type="button" onClick={handleAddQuickClient} className="text-falcon-accent hover:underline text-xs font-medium">Створити нового</button>
                  </span>
                  <select required value={orderForm.clientId} onChange={(e) => setOrderForm({...orderForm, clientId: e.target.value})} className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:border-falcon-dark cursor-pointer">
                    <option value="">Оберіть клієнта...</option>
                    {clients.map(c => {
                      const displayLabel = (c.phone || c.email) ? `${c.firstName || 'Клієнт'} (${c.phone || c.email})` : c.firstName;
                      return <option key={c.id} value={c.id}>{displayLabel}</option>
                    })}
                  </select>
                </label>
                <label className="flex flex-col">
                  <span className="text-sm font-medium text-gray-700 mb-1">Канал продажу</span>
                  <input type="text" value={orderForm.salesChannel} onChange={(e) => setOrderForm({...orderForm, salesChannel: e.target.value})} placeholder="Наприклад: Instagram" className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:border-falcon-dark" />
                </label>
                <label className="flex flex-col">
                  <span className="text-sm font-medium text-gray-700 mb-1">Статус замовлення</span>
                  <select value={orderForm.statusId} onChange={(e) => setOrderForm({...orderForm, statusId: e.target.value})} className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:border-falcon-dark cursor-pointer">
                    <option value="">Без статусу</option>
                    {orderForm.statusId && !statuses.find(s => String(s.id||s.Id).toLowerCase() === orderForm.statusId) && (
                      <option value={orderForm.statusId}>{editingOrder?.currentStatusName || editingOrder?.CurrentStatusName || 'Збережений статус'}</option>
                    )}
                    {statuses.map(s => <option key={s.id} value={String(s.id || s.Id).toLowerCase().trim()}>{s.name}</option>)}
                  </select>
                </label>
                <label className="flex flex-col">
                  <span className="text-sm font-medium text-gray-700 mb-1">Відповідальний менеджер</span>
                  <select value={orderForm.managerId} onChange={(e) => setOrderForm({...orderForm, managerId: e.target.value})} className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:border-falcon-dark cursor-pointer">
                    <option value="">Не призначено</option>
                    {managers.map(m => <option key={m.id} value={String(m.id || m.Id).toLowerCase().trim()}>{m.fullName || m.login}</option>)}
                  </select>
                </label>
                <label className="flex flex-col col-span-2">
                  <span className="text-sm font-medium text-gray-700 mb-1">Адреса доставки</span>
                  <input type="text" value={orderForm.deliveryAddress} onChange={(e) => setOrderForm({...orderForm, deliveryAddress: e.target.value})} placeholder="м. Київ, НП №1" className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:border-falcon-dark" />
                </label>
              </div>
              <div className="border border-gray-200 rounded-lg p-4 bg-gray-50">
                <div className="flex justify-between items-center mb-3">
                  <h4 className="font-medium text-gray-800">Товари</h4>
                  <button type="button" onClick={handleAddItem} className="text-sm text-falcon-dark font-medium px-3 py-1 bg-white border border-falcon-dark/30 rounded hover:bg-falcon-light/20 transition-colors">+ Додати товар</button>
                </div>
                {orderForm.items.length === 0 ? (
                  <p className="text-sm text-gray-500 text-center py-4 bg-white border border-dashed border-gray-300 rounded">Не додано жодного товару</p>
                ) : (
                  <div className="flex flex-col gap-3">
                    {orderForm.items.map((item, index) => (
                      <div key={index} className="flex gap-3 items-end bg-white p-3 rounded border border-gray-200 shadow-sm">
                        <label className="flex-1 flex flex-col">
                          <span className="text-xs text-gray-500 mb-1">Товар</span>
                          <select required value={item.productId} onChange={(e) => handleItemChange(index, 'productId', e.target.value)} className="border border-gray-300 rounded text-sm px-2 py-1.5 focus:outline-none focus:border-falcon-dark cursor-pointer">
                            <option value="">Оберіть...</option>
                            {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                          </select>
                        </label>
                        <label className="w-20 flex flex-col"><span className="text-xs text-gray-500 mb-1">К-сть</span><input type="number" min="1" required value={item.quantity} onChange={(e) => handleItemChange(index, 'quantity', parseInt(e.target.value))} className="border border-gray-300 rounded text-sm px-2 py-1.5 focus:outline-none focus:border-falcon-dark" /></label>
                        <label className="w-24 flex flex-col"><span className="text-xs text-gray-500 mb-1">Ціна (₴)</span><input type="number" min="0" step="0.01" required value={item.priceAtOrder} onChange={(e) => handleItemChange(index, 'priceAtOrder', parseFloat(e.target.value))} className="border border-gray-300 rounded text-sm px-2 py-1.5 focus:outline-none focus:border-falcon-dark" /></label>
                        <button type="button" onClick={() => handleRemoveItem(index)} className="text-gray-400 hover:text-red-500 hover:bg-red-50 p-1.5 rounded mb-0.5 transition-colors">✕</button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
              <label className="flex flex-col">
                <span className="text-sm font-medium text-gray-700 mb-1">Нотатки</span>
                <textarea value={orderForm.notes} onChange={(e) => setOrderForm({...orderForm, notes: e.target.value})} rows="2" className="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:border-falcon-dark resize-none" />
              </label>
              <div className="flex gap-3 pt-4 border-t border-gray-200 justify-end mt-2 shrink-0">
                <button type="button" onClick={() => setIsOrderModalOpen(false)} className="px-5 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded transition-colors">Скасувати</button>
                <button type="submit" className="px-6 py-2 text-sm font-medium bg-falcon-dark text-white rounded shadow-sm hover:brightness-110 transition-all">Зберегти замовлення</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
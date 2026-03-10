import { useState, useEffect } from 'react';
import { apiClient } from '../api/apiClient';
import freeProduct from "../assets/products/freeProduct.png";

export default function ProductsPage() {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [sortBy, setSortBy] = useState('name');

  // --- Стейт для модального вікна створення товару ---
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [newProduct, setNewProduct] = useState({
    name: '',
    price: '',
    categoryId: ''
  });

  const fetchCategories = async () => {
    try {
      const response = await apiClient.get('/api/category');
      setCategories(response.data);
    } catch (err) {
      console.error('Помилка завантаження категорій:', err);
    }
  };

  const fetchProducts = async () => {
    try {
      const response = await apiClient.get('/api/product');
      setProducts(response.data);
    } catch (err) {
      console.error('Помилка завантаження товарів:', err);
      throw err;
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      setIsLoading(true);
      setError(null);
      try {
        await Promise.all([fetchProducts(), fetchCategories()]);
      } catch (err) {
        setError('Не вдалося завантажити дані. Спробуйте пізніше.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  // --- СТВОРЕННЯ ТОВАРУ ---
  const handleCreateProduct = async (e) => {
    e.preventDefault();
    
    // Валідація згідно з документацією (обов'язкові name та price)
    if (!newProduct.name || !newProduct.price) {
      alert('Будь ласка, заповніть назву та ціну товару');
      return;
    }

    try {
      const payload = {
        name: newProduct.name,
        price: parseFloat(newProduct.price), // Бекенд очікує число (double)
        categoryId: newProduct.categoryId || null
      };

      await apiClient.post('/api/product', payload);
      
      alert('Товар успішно додано!');
      setIsAddModalOpen(false); // Закриваємо модалку
      setNewProduct({ name: '', price: '', categoryId: '' }); // Очищаємо форму
      await fetchProducts(); // Оновлюємо список товарів
      
    } catch (err) {
      console.error('Помилка створення товару:', err);
      alert('Не вдалося створити товар. Перевірте введені дані.');
    }
  };

  const handleDeleteProduct = async (id) => {
    if (window.confirm('Ви впевнені, що хочете видалити цей товар?')) {
      try {
        await apiClient.delete(`/api/product/${id}`);
        setProducts(products.filter(p => p.id !== id));
      } catch (err) {
        console.error('Помилка видалення товару:', err);
        alert('Не вдалося видалити товар.');
      }
    }
  };

  const handleAddCategory = async () => {
    const newCategoryName = window.prompt('Введіть назву нової категорії:');
    
    if (!newCategoryName || newCategoryName.trim() === '') {
      return; 
    }

    try {
      await apiClient.post('/api/category', { name: newCategoryName.trim() });
      await fetchCategories();
      alert(`Категорію "${newCategoryName}" успішно додано!`);
    } catch (err) {
      console.error('Помилка створення категорії:', err);
      alert('Не вдалося створити категорію.');
    }
  };

  const handleDeleteCategory = async () => {
    if (!selectedCategory) return;

    const categoryToDelete = categories.find(c => c.id === selectedCategory);
    if (!categoryToDelete) return;

    if (window.confirm(`Ви впевнені, що хочете видалити категорію "${categoryToDelete.name}"?`)) {
      try {
        await apiClient.delete(`/api/category/${selectedCategory}`);
        alert('Категорію успішно видалено!');
        setSelectedCategory(''); 
        await Promise.all([fetchCategories(), fetchProducts()]);
      } catch (err) {
        console.error('Помилка видалення категорії:', err);
        alert('Не вдалося видалити категорію. Можливо, до неї ще прив\'язані товари.');
      }
    }
  };

  const getFilteredAndSortedProducts = () => {
    let result = [...products];

    if (selectedCategory) {
      result = result.filter(p => p.categoryId === selectedCategory);
    }

    if (searchQuery) {
      const lowerQuery = searchQuery.toLowerCase();
      result = result.filter(p => 
        p.name?.toLowerCase().includes(lowerQuery) || 
        (p.sku && p.sku.toLowerCase().includes(lowerQuery))
      );
    }

    if (sortBy === 'price') {
      result.sort((a, b) => (a.price || 0) - (b.price || 0));
    } else if (sortBy === 'name') {
      result.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
    }

    return result;
  };

  const displayedProducts = getFilteredAndSortedProducts();

  if (isLoading) {
    return <div className="p-8 text-center text-falcon-dark">Завантаження товарів...</div>;
  }

  if (error) {
    return <div className="p-8 text-center text-red-500">{error}</div>;
  }

  return (
    <div className="w-full relative">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl text-falcon-dark font-medium">Товари</h1>
        
        <div className="flex gap-4 items-center">
          {/* Кнопка Додати товар тепер відкриває модалку */}
          <button 
            onClick={() => setIsAddModalOpen(true)}
            className="w-8 h-8 bg-falcon-accent text-falcon-dark rounded flex items-center justify-center hover:brightness-110 transition-all shadow-sm" 
            title="Додати товар"
          >
            <span className="text-xl font-bold">+</span>
          </button>
          
          <div className="relative">
            <input 
              type="text" 
              placeholder="Пошук" 
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="bg-falcon-light text-falcon-dark placeholder-falcon-dark/50 px-4 py-1.5 rounded-md w-64 focus:outline-none focus:ring-1 focus:ring-falcon-dark text-sm"
            />
          </div>
        </div>
      </div>

      <div className="flex justify-between mb-8">
        <div className="flex items-center gap-3">
          <span className="text-falcon-dark">Категорія:</span>
          
          <div className="flex items-center gap-2">
            <select 
              value={selectedCategory} 
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="bg-falcon-light text-falcon-dark px-4 py-1.5 rounded-md min-w-[150px] focus:outline-none text-sm border-none shadow-sm cursor-pointer"
            >
              <option value="">Всі категорії</option>
              {categories.map(cat => (
                <option key={cat.id} value={cat.id}>{cat.name}</option>
              ))}
            </select>
            
            <button 
              onClick={handleAddCategory}
              className="w-7 h-7 bg-falcon-light text-falcon-dark rounded flex items-center justify-center hover:brightness-95 transition-all text-lg font-medium shadow-sm"
              title="Додати нову категорію"
            >
              +
            </button>

            {selectedCategory && (
              <button 
                onClick={handleDeleteCategory}
                className="w-7 h-7 bg-red-100 text-red-600 rounded flex items-center justify-center hover:bg-red-200 transition-all text-sm font-medium shadow-sm"
                title="Видалити обрану категорію"
              >
                ✕
              </button>
            )}
          </div>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-falcon-dark">Сортувати за:</span>
          <select 
            value={sortBy} 
            onChange={(e) => setSortBy(e.target.value)}
            className="bg-falcon-light text-falcon-dark px-4 py-1.5 rounded-md min-w-[150px] focus:outline-none text-sm border-none shadow-sm cursor-pointer"
          >
            <option value="name">Назвою</option>
            <option value="price">Ціною</option>
          </select>
        </div>
      </div>

      {displayedProducts.length === 0 ? (
        <div className="text-center py-12 text-gray-500">Товари не знайдено.</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {displayedProducts.map((product) => (
            <div key={product.id} className="relative flex rounded-xl overflow-hidden shadow-md group h-full">
              
              <div className="flex-1 bg-falcon-dark p-4 flex flex-col relative z-10 border border-transparent group-hover:border-falcon-light/30 transition-colors">
                <img 
                  src={product.img || freeProduct} 
                  alt={product.name} 
                  className="w-full h-40 object-cover rounded-lg mb-4 border border-falcon-light/20 shadow-inner bg-white/5"
                />
                
                <div className="text-white text-xs space-y-1 mt-auto flex-1 flex flex-col">
                  <h3 className="text-lg font-medium mb-2 leading-tight">{product.name}</h3>
                  <p>Ціна: <span className="text-falcon-light font-medium">{product.price} ₴</span></p>
                  <p>В наявності: <span className="text-falcon-light">{product.qty || 'Немає даних'}</span></p>
                  <p>Вага: <span className="text-falcon-light">{product.weight || 'Не вказано'}</span></p>
                  <p className="line-clamp-2" title={product.note}>Замітка: <span className="text-falcon-light/80 italic">{product.note || 'Немає'}</span></p>
                  <p className="pt-2 mt-auto">Артикул: <span className="text-falcon-light/50">{product.sku || product.id?.substring(0,8)}</span></p>
                </div>
              </div>

              <div className="w-12 bg-falcon-accent flex flex-col items-center py-3 gap-5 border-l border-falcon-dark/10 relative z-0">
                <button 
                  onClick={() => handleDeleteProduct(product.id)}
                  className="text-falcon-dark hover:scale-125 hover:text-red-600 transition-all" 
                  title="Видалити товар"
                >
                  ✕
                </button>
                <button className="text-falcon-dark hover:scale-125 transition-all" title="Перегляд">👁</button>
                <button className="text-falcon-dark hover:scale-125 transition-all" title="Додати в обране">☆</button>
                
                <div className="mt-auto mb-1">
                  <button className="text-falcon-dark hover:scale-125 transition-all" title="Більше дій">•••</button>
                </div>
              </div>

            </div>
          ))}
        </div>
      )}

      {/* --- МОДАЛЬНЕ ВІКНО ДОДАВАННЯ ТОВАРУ --- */}
      {isAddModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden flex flex-col">
            <div className="bg-falcon-dark px-6 py-4 flex justify-between items-center">
              <h3 className="text-white text-lg font-medium">Новий товар</h3>
              <button 
                onClick={() => setIsAddModalOpen(false)}
                className="text-white hover:text-falcon-light transition-colors text-xl leading-none"
              >
                ✕
              </button>
            </div>
            
            <form onSubmit={handleCreateProduct} className="p-6 flex flex-col gap-4">
              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Назва товару <span className="text-red-500">*</span></span>
                <input 
                  type="text" 
                  required
                  value={newProduct.name}
                  onChange={(e) => setNewProduct({...newProduct, name: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark text-sm"
                  placeholder="Наприклад: Джем Вишневий"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Ціна (₴) <span className="text-red-500">*</span></span>
                <input 
                  type="number" 
                  required
                  min="0"
                  step="0.01"
                  value={newProduct.price}
                  onChange={(e) => setNewProduct({...newProduct, price: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark text-sm"
                  placeholder="250"
                />
              </label>

              <label className="flex flex-col">
                <span className="text-sm text-gray-600 mb-1 font-medium">Категорія</span>
                <select 
                  value={newProduct.categoryId}
                  onChange={(e) => setNewProduct({...newProduct, categoryId: e.target.value})}
                  className="border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:border-falcon-dark focus:ring-1 focus:ring-falcon-dark text-sm cursor-pointer"
                >
                  <option value="">Без категорії</option>
                  {categories.map(cat => (
                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                  ))}
                </select>
              </label>

              <div className="flex gap-3 mt-4 pt-4 border-t border-gray-100 justify-end">
                <button 
                  type="button" 
                  onClick={() => setIsAddModalOpen(false)}
                  className="px-4 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-md transition-colors font-medium"
                >
                  Скасувати
                </button>
                <button 
                  type="submit"
                  className="px-6 py-2 text-sm bg-falcon-dark text-white rounded-md hover:brightness-110 transition-all font-medium shadow-sm"
                >
                  Зберегти товар
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {!isLoading && displayedProducts.length > 0 && (
        <div className="mt-12 flex flex-col items-center gap-4 pb-8">
          <p className="text-falcon-dark/60 text-sm">Показано: {displayedProducts.length} з {products.length}</p>
          
          <button className="group flex items-center gap-2 px-8 py-2.5 bg-white border border-falcon-dark/20 text-falcon-dark font-medium rounded-full hover:border-falcon-dark hover:bg-falcon-light/10 transition-all shadow-sm">
            <span>Завантажити ще</span>
            <svg 
              className="w-5 h-5 text-falcon-dark/70 group-hover:text-falcon-dark group-hover:translate-y-0.5 transition-transform" 
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
        </div>
      )}
    </div>
  );
}
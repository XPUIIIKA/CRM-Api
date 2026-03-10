import axios from 'axios';

// Створюємо інстанс axios з базовим URL
export const apiClient = axios.create({
  headers: {
    'Content-Type': 'application/json',
  },
});

// Додаємо інтерцептор: перед КОЖНИМ запитом перевіряємо, чи є токен
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    // Якщо токен є, додаємо його в заголовок Authorization
    config.headers.Authorization = `Bearer ${token}`; 
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

// Обробка помилок (наприклад, якщо токен протух - викидаємо на логін)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('token');
      // window.location.href = '/login'; // Розкоментуємо пізніше, щоб автоматично розлогінювати
    }
    return Promise.reject(error);
  }
);
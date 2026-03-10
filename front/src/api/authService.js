import { apiClient } from './apiClient';
import { tokenService } from '../utils/token';

export const authService = {
  login: async (email, password) => {
    // Делаем POST запрос на твой эндпоинт
    const data = await apiClient.post('/auth/login', { email, password });
    
    // Предполагаем, что бэк возвращает { token: '...' }
    if (data.token) {
      tokenService.setToken(data.token);
    }
    
    return data; 
  },
  
  logout: () => {
    tokenService.removeToken();
  }
};
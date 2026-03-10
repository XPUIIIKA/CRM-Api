import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  // Перевіряємо, чи є вже токен у браузері при завантаженні
  token: localStorage.getItem('token') || null, 
  isAuthenticated: !!localStorage.getItem('token'),
  user: null, // Тут потім будемо зберігати дані юзера
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    // Дія при успішному логіні
    loginSuccess: (state, action) => {
      state.token = action.payload.token;
      state.isAuthenticated = true;
      localStorage.setItem('token', action.payload.token);
    },
    // Дія при виході
    logout: (state) => {
      state.token = null;
      state.isAuthenticated = false;
      state.user = null;
      localStorage.removeItem('token');
    },
    // Збереження даних юзера (наприклад після GET /api/auth/me)
    setUser: (state, action) => {
      state.user = action.payload;
    }
  },
});

export const { loginSuccess, logout, setUser } = authSlice.actions;
export default authSlice.reducer;
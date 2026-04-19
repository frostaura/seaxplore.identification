import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import api from '../services/api';
import type { AdminLoginResponse } from '../types';

interface AuthState {
  token: string | null;
  username: string | null;
  loading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  token: localStorage.getItem('oceancare_token'),
  username: localStorage.getItem('oceancare_username'),
  loading: false,
  error: null,
};

export const login = createAsyncThunk(
  'auth/login',
  async (credentials: { username: string; password: string }, { rejectWithValue }) => {
    try {
      const res = await api.post<AdminLoginResponse>('/api/admin/login', credentials);
      localStorage.setItem('oceancare_token', res.data.token);
      localStorage.setItem('oceancare_username', res.data.username);
      return res.data;
    } catch {
      return rejectWithValue('Invalid credentials. Please try again.');
    }
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      state.token = null;
      state.username = null;
      localStorage.removeItem('oceancare_token');
      localStorage.removeItem('oceancare_username');
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<AdminLoginResponse>) => {
        state.loading = false;
        state.token = action.payload.token;
        state.username = action.payload.username;
      })
      .addCase(login.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;

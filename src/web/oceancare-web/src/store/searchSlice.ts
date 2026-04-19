import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import api from '../services/api';
import type { SearchResult } from '../types';

interface SearchState {
  query: string;
  results: SearchResult[];
  loading: boolean;
  error: string | null;
}

const initialState: SearchState = {
  query: '',
  results: [],
  loading: false,
  error: null,
};

export const performSearch = createAsyncThunk(
  'search/perform',
  async (query: string, { rejectWithValue }) => {
    try {
      const res = await api.get<SearchResult[]>('/api/search', { params: { q: query, topK: 12 } });
      return res.data;
    } catch (e) {
      const err = e as { response?: { data?: string } };
      return rejectWithValue(err.response?.data || 'Search failed');
    }
  }
);

const searchSlice = createSlice({
  name: 'search',
  initialState,
  reducers: {
    setQuery(state, action: PayloadAction<string>) {
      state.query = action.payload;
    },
    clearResults(state) {
      state.results = [];
      state.query = '';
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(performSearch.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(performSearch.fulfilled, (state, action) => {
        state.loading = false;
        state.results = action.payload;
      })
      .addCase(performSearch.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { setQuery, clearResults } = searchSlice.actions;
export default searchSlice.reducer;

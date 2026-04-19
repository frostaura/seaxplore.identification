import { useState, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { performSearch, setQuery, clearResults } from '../../store/searchSlice';
import type { AppDispatch, RootState } from '../../store';

export default function SearchBar() {
  const dispatch = useDispatch<AppDispatch>();
  const { query, loading } = useSelector((s: RootState) => s.search);
  const [localQuery, setLocalQuery] = useState(query);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (localQuery.trim().length < 3) {
      if (!localQuery.trim()) dispatch(clearResults());
      return;
    }
    debounceRef.current = setTimeout(() => {
      dispatch(setQuery(localQuery));
      dispatch(performSearch(localQuery));
    }, 400);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [localQuery, dispatch]);

  return (
    <div style={{ position: 'relative', maxWidth: '700px', width: '100%' }}>
      <span
        style={{
          position: 'absolute',
          left: '18px',
          top: '50%',
          transform: 'translateY(-50%)',
          fontSize: '1.3rem',
          pointerEvents: 'none',
        }}
      >
        🔍
      </span>
      <input
        type="text"
        value={localQuery}
        onChange={(e) => setLocalQuery(e.target.value)}
        placeholder="Describe what you're looking for… e.g. 'red coral with blue dots that glows in the dark'"
        style={{
          width: '100%',
          padding: '18px 20px 18px 52px',
          fontSize: '1rem',
          borderRadius: '50px',
          border: '2px solid rgba(0,180,216,0.4)',
          background: 'rgba(3,4,94,0.7)',
          color: '#caf0f8',
          outline: 'none',
          backdropFilter: 'blur(8px)',
          boxSizing: 'border-box',
          transition: 'border-color 0.2s',
        }}
        onFocus={(e) => { (e.target as HTMLInputElement).style.borderColor = '#00b4d8'; }}
        onBlur={(e) => { (e.target as HTMLInputElement).style.borderColor = 'rgba(0,180,216,0.4)'; }}
      />
      {loading && (
        <span
          style={{
            position: 'absolute',
            right: '18px',
            top: '50%',
            transform: 'translateY(-50%)',
            fontSize: '1.2rem',
            animation: 'spin 1s linear infinite',
          }}
        >
          🌊
        </span>
      )}
    </div>
  );
}

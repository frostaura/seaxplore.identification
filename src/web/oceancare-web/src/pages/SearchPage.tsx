import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import SearchBar from '../components/organisms/SearchBar';
import SpeciesCard from '../components/molecules/SpeciesCard';
import { Link } from 'react-router-dom';

export default function SearchPage() {
  const { results, loading, error, query } = useSelector((s: RootState) => s.search);

  return (
    <div
      style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #03045e 0%, #023e8a 50%, #0077b6 100%)',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
      }}
    >
      {/* Navigation */}
      <nav
        style={{
          width: '100%',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '16px 32px',
          borderBottom: '1px solid rgba(0,180,216,0.2)',
          boxSizing: 'border-box',
          backdropFilter: 'blur(8px)',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <span style={{ fontSize: '1.8rem' }}>🐠</span>
          <span style={{ fontSize: '1.4rem', fontWeight: 700, color: '#caf0f8' }}>OceanCare</span>
        </div>
        <Link
          to="/admin"
          style={{
            color: '#90e0ef',
            textDecoration: 'none',
            fontSize: '0.9rem',
            border: '1px solid rgba(0,180,216,0.3)',
            padding: '8px 16px',
            borderRadius: '20px',
            transition: 'background 0.2s',
          }}
        >
          Admin ⚙️
        </Link>
      </nav>

      {/* Hero section */}
      <div
        style={{
          textAlign: 'center',
          padding: '48px 24px 32px',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '16px',
          maxWidth: '800px',
        }}
      >
        <h1
          style={{
            fontSize: 'clamp(2rem, 5vw, 3.5rem)',
            color: '#caf0f8',
            margin: 0,
            fontWeight: 800,
            lineHeight: 1.2,
          }}
        >
          Discover{' '}
          <span style={{ color: '#48cae4' }}>Marine Life</span>
        </h1>
        <p style={{ color: '#90e0ef', fontSize: '1.1rem', margin: 0 }}>
          Describe what you're looking for in plain language — our semantic search engine finds the closest matches.
        </p>

        <SearchBar />

        {!query && (
          <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap', justifyContent: 'center', marginTop: '8px' }}>
            {[
              'red coral with white tips',
              'colorful fish that glows in the dark',
              'translucent jellyfish with long tentacles',
              'large sea turtle with patterned shell',
            ].map((hint) => (
              <button
                key={hint}
                onClick={() => {
                  const event = new CustomEvent('search-hint', { detail: hint });
                  window.dispatchEvent(event);
                }}
                style={{
                  background: 'rgba(0,180,216,0.1)',
                  border: '1px solid rgba(0,180,216,0.3)',
                  borderRadius: '20px',
                  color: '#ade8f4',
                  padding: '6px 14px',
                  cursor: 'pointer',
                  fontSize: '0.8rem',
                }}
              >
                {hint}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Results */}
      <div style={{ width: '100%', maxWidth: '1200px', padding: '0 24px 48px', boxSizing: 'border-box' }}>
        {error && (
          <div
            style={{
              background: 'rgba(239,68,68,0.1)',
              border: '1px solid rgba(239,68,68,0.3)',
              borderRadius: '12px',
              padding: '16px',
              color: '#fca5a5',
              textAlign: 'center',
              marginBottom: '24px',
            }}
          >
            ⚠️ {error} — Make sure the API server is running.
          </div>
        )}

        {query && !loading && results.length === 0 && !error && (
          <p style={{ textAlign: 'center', color: '#90e0ef' }}>
            No matches found for "{query}". Try a different description.
          </p>
        )}

        {results.length > 0 && (
          <>
            <p style={{ color: '#90e0ef', marginBottom: '16px' }}>
              Showing {results.length} result{results.length !== 1 ? 's' : ''} for "
              <em style={{ color: '#caf0f8' }}>{query}</em>"
            </p>
            <div
              style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
                gap: '20px',
              }}
            >
              {results.map((r) => (
                <SpeciesCard key={r.species.id} result={r} />
              ))}
            </div>
          </>
        )}

        {!query && !loading && (
          <div style={{ textAlign: 'center', color: '#90e0ef', marginTop: '20px' }}>
            <p style={{ fontSize: '4rem' }}>🌊</p>
            <p>Start typing above to explore the ocean's biodiversity</p>
          </div>
        )}
      </div>
    </div>
  );
}

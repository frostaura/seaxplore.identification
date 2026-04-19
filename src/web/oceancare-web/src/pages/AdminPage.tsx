import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router-dom';
import { logout } from '../store/authSlice';
import type { AppDispatch, RootState } from '../store';
import type { Species, MarineAttribute, Category } from '../types';
import api from '../services/api';
import SpeciesForm from '../components/organisms/SpeciesForm';
import AttributeManager from '../components/organisms/AttributeManager';

type Tab = 'species' | 'attributes' | 'categories';

export default function AdminPage() {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { token, username } = useSelector((s: RootState) => s.auth);

  const [activeTab, setActiveTab] = useState<Tab>('species');
  const [species, setSpecies] = useState<Species[]>([]);
  const [attributes, setAttributes] = useState<MarineAttribute[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [editingSpecies, setEditingSpecies] = useState<Species | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [loading, setLoading] = useState(false);

  const loadData = async () => {
    setLoading(true);
    try {
      const [sp, at, cat] = await Promise.all([
        api.get<Species[]>('/api/admin/species'),
        api.get<MarineAttribute[]>('/api/admin/attributes'),
        api.get<Category[]>('/api/admin/categories'),
      ]);
      setSpecies(sp.data);
      setAttributes(at.data);
      setCategories(cat.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!token) { navigate('/login'); return; }
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadData();
  // loadData is stable (no external deps that change); navigate is stable from react-router
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/');
  };

  const handleDeleteSpecies = async (id: number) => {
    if (!confirm('Delete this species?')) return;
    await api.delete(`/api/admin/species/${id}`);
    setSpecies((prev) => prev.filter((s) => s.id !== id));
  };

  const handleFormSave = () => {
    setShowForm(false);
    setEditingSpecies(null);
    loadData();
  };

  const navStyle = (tab: Tab): React.CSSProperties => ({
    padding: '10px 20px',
    background: activeTab === tab ? 'rgba(0,180,216,0.3)' : 'transparent',
    border: activeTab === tab ? '1px solid rgba(0,180,216,0.5)' : '1px solid transparent',
    borderRadius: '8px',
    color: '#caf0f8',
    cursor: 'pointer',
    fontSize: '0.9rem',
  });

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #03045e 0%, #023e8a 50%, #0077b6 100%)' }}>
      {/* Top bar */}
      <nav style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        padding: '16px 32px', borderBottom: '1px solid rgba(0,180,216,0.2)',
        backdropFilter: 'blur(8px)',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Link to="/" style={{ textDecoration: 'none' }}>
            <span style={{ fontSize: '1.8rem' }}>🐠</span>
            <span style={{ fontSize: '1.2rem', fontWeight: 700, color: '#caf0f8', marginLeft: '8px' }}>OceanCare Admin</span>
          </Link>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          <span style={{ color: '#90e0ef', fontSize: '0.9rem' }}>👤 {username}</span>
          <button onClick={handleLogout} style={{
            background: 'transparent', border: '1px solid rgba(239,68,68,0.4)',
            color: '#fca5a5', padding: '8px 16px', borderRadius: '8px', cursor: 'pointer', fontSize: '0.85rem',
          }}>
            Sign Out
          </button>
        </div>
      </nav>

      <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '24px' }}>
        {/* Tabs */}
        <div style={{ display: 'flex', gap: '8px', marginBottom: '24px' }}>
          <button style={navStyle('species')} onClick={() => setActiveTab('species')}>🐠 Species</button>
          <button style={navStyle('attributes')} onClick={() => setActiveTab('attributes')}>🏷️ Attributes</button>
          <button style={navStyle('categories')} onClick={() => setActiveTab('categories')}>📁 Categories</button>
        </div>

        {loading && <p style={{ color: '#90e0ef' }}>Loading…</p>}

        {/* Species Tab */}
        {activeTab === 'species' && !showForm && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
              <h2 style={{ color: '#caf0f8', margin: 0 }}>Marine Species ({species.length})</h2>
              <button
                onClick={() => { setEditingSpecies(null); setShowForm(true); }}
                style={{
                  background: 'rgba(0,180,216,0.7)', color: '#fff', border: 'none',
                  borderRadius: '8px', padding: '10px 20px', cursor: 'pointer', fontWeight: 600,
                }}
              >
                + Add Species
              </button>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '16px' }}>
              {species.map((s) => (
                <div key={s.id} style={{
                  background: 'rgba(3,4,94,0.6)', border: '1px solid rgba(0,180,216,0.25)',
                  borderRadius: '12px', overflow: 'hidden',
                }}>
                  <img
                    src={s.imageUrl || 'https://images.unsplash.com/photo-1518020382113-a7e8fc38eac9?w=400&q=80'}
                    alt={s.commonName}
                    onError={(e) => { (e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1518020382113-a7e8fc38eac9?w=400&q=80'; }}
                    style={{ width: '100%', height: '150px', objectFit: 'cover' }}
                  />
                  <div style={{ padding: '12px' }}>
                    <h3 style={{ color: '#caf0f8', margin: '0 0 4px', fontSize: '1rem' }}>{s.commonName}</h3>
                    <p style={{ color: '#90e0ef', margin: '0 0 8px', fontSize: '0.8rem', fontStyle: 'italic' }}>{s.scientificName}</p>
                    <p style={{ color: '#ade8f4', margin: '0 0 12px', fontSize: '0.8rem' }}>{s.categoryName}</p>
                    <div style={{ display: 'flex', gap: '8px' }}>
                      <button
                        onClick={() => { setEditingSpecies(s); setShowForm(true); }}
                        style={{
                          flex: 1, background: 'rgba(0,180,216,0.2)', border: '1px solid rgba(0,180,216,0.3)',
                          color: '#caf0f8', borderRadius: '6px', padding: '8px', cursor: 'pointer', fontSize: '0.85rem',
                        }}
                      >
                        ✏️ Edit
                      </button>
                      <button
                        onClick={() => handleDeleteSpecies(s.id)}
                        style={{
                          flex: 1, background: 'rgba(239,68,68,0.1)', border: '1px solid rgba(239,68,68,0.3)',
                          color: '#fca5a5', borderRadius: '6px', padding: '8px', cursor: 'pointer', fontSize: '0.85rem',
                        }}
                      >
                        🗑️ Delete
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Species Form */}
        {activeTab === 'species' && showForm && (
          <SpeciesForm
            species={editingSpecies}
            attributes={attributes}
            categories={categories}
            onSave={handleFormSave}
            onCancel={() => { setShowForm(false); setEditingSpecies(null); }}
          />
        )}

        {/* Attributes Tab */}
        {activeTab === 'attributes' && (
          <AttributeManager
            items={attributes}
            type="attribute"
            onRefresh={loadData}
          />
        )}

        {/* Categories Tab */}
        {activeTab === 'categories' && (
          <AttributeManager
            items={categories}
            type="category"
            onRefresh={loadData}
          />
        )}
      </div>
    </div>
  );
}

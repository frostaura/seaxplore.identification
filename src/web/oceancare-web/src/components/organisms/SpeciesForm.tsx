import { useState, useEffect } from 'react';
import type { Species, MarineAttribute, Category } from '../../types';
import api from '../../services/api';

interface Props {
  species: Species | null;
  attributes: MarineAttribute[];
  categories: Category[];
  onSave: () => void;
  onCancel: () => void;
}

interface AttrEntry {
  attributeId: number;
  value: string;
}

export default function SpeciesForm({ species, attributes, categories, onSave, onCancel }: Props) {
  const [scientificName, setScientificName] = useState(species?.scientificName ?? '');
  const [commonName, setCommonName] = useState(species?.commonName ?? '');
  const [description, setDescription] = useState(species?.description ?? '');
  const [imageUrl, setImageUrl] = useState(species?.imageUrl ?? '');
  const [categoryId, setCategoryId] = useState<number>(species?.categoryId ?? (categories[0]?.id ?? 0));
  const [attrValues, setAttrValues] = useState<AttrEntry[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (species) {
      setAttrValues(
        attributes.map((a) => ({
          attributeId: a.id,
          value: species.attributes.find((av) => av.attributeId === a.id)?.value ?? '',
        }))
      );
    } else {
      setAttrValues(attributes.map((a) => ({ attributeId: a.id, value: '' })));
    }
  }, [species, attributes]);

  const handleAttrChange = (attrId: number, value: string) => {
    setAttrValues((prev) => prev.map((e) => (e.attributeId === attrId ? { ...e, value } : e)));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      const payload = {
        scientificName,
        commonName,
        description,
        imageUrl: imageUrl || null,
        categoryId,
        attributes: attrValues.filter((a) => a.value.trim()),
      };

      if (species) {
        await api.put(`/api/admin/species/${species.id}`, payload);
      } else {
        await api.post('/api/admin/species', payload);
      }
      onSave();
    } catch (err: any) {
      setError(err.response?.data ?? 'Failed to save. Check that the API is running.');
    } finally {
      setSaving(false);
    }
  };

  const inputStyle: React.CSSProperties = {
    width: '100%',
    padding: '10px 14px',
    borderRadius: '8px',
    border: '1px solid rgba(0,180,216,0.3)',
    background: 'rgba(3,4,94,0.6)',
    color: '#caf0f8',
    fontSize: '0.95rem',
    outline: 'none',
    boxSizing: 'border-box',
  };

  const labelStyle: React.CSSProperties = {
    color: '#90e0ef',
    fontSize: '0.85rem',
    display: 'block',
    marginBottom: '6px',
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
        <h2 style={{ color: '#caf0f8', margin: 0 }}>
          {species ? `Edit: ${species.commonName}` : 'Add New Species'}
        </h2>
        <button
          onClick={onCancel}
          style={{ background: 'transparent', border: '1px solid rgba(0,180,216,0.3)', color: '#90e0ef', padding: '8px 16px', borderRadius: '8px', cursor: 'pointer' }}
        >
          ← Back
        </button>
      </div>

      <form onSubmit={handleSubmit}>
        <div style={{
          background: 'rgba(3,4,94,0.6)', border: '1px solid rgba(0,180,216,0.2)',
          borderRadius: '16px', padding: '24px', display: 'flex', flexDirection: 'column', gap: '16px',
        }}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
            <div>
              <label style={labelStyle}>Common Name *</label>
              <input style={inputStyle} value={commonName} onChange={(e) => setCommonName(e.target.value)} required placeholder="e.g. Clownfish" />
            </div>
            <div>
              <label style={labelStyle}>Scientific Name *</label>
              <input style={inputStyle} value={scientificName} onChange={(e) => setScientificName(e.target.value)} required placeholder="e.g. Amphiprion ocellaris" />
            </div>
          </div>

          <div>
            <label style={labelStyle}>Category *</label>
            <select
              style={{ ...inputStyle }}
              value={categoryId}
              onChange={(e) => setCategoryId(Number(e.target.value))}
              required
            >
              {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>

          <div>
            <label style={labelStyle}>Description *</label>
            <textarea
              style={{ ...inputStyle, minHeight: '100px', resize: 'vertical' }}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
              placeholder="Describe the species' appearance, behavior, and habitat…"
            />
          </div>

          <div>
            <label style={labelStyle}>Image URL</label>
            <input style={inputStyle} value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} type="url" placeholder="https://…" />
          </div>

          {/* Dynamic Attributes */}
          {attributes.length > 0 && (
            <div>
              <label style={{ ...labelStyle, marginBottom: '12px', fontSize: '0.95rem', fontWeight: 600 }}>Attributes</label>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '12px' }}>
                {attributes.map((attr) => (
                  <div key={attr.id}>
                    <label style={labelStyle}>{attr.name}</label>
                    {attr.dataType === 'boolean' ? (
                      <select
                        style={inputStyle}
                        value={attrValues.find((a) => a.attributeId === attr.id)?.value ?? ''}
                        onChange={(e) => handleAttrChange(attr.id, e.target.value)}
                      >
                        <option value="">Not specified</option>
                        <option value="true">Yes</option>
                        <option value="false">No</option>
                      </select>
                    ) : (
                      <input
                        style={inputStyle}
                        value={attrValues.find((a) => a.attributeId === attr.id)?.value ?? ''}
                        onChange={(e) => handleAttrChange(attr.id, e.target.value)}
                        placeholder={attr.description}
                      />
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {error && <p style={{ color: '#fca5a5', margin: 0 }}>⚠️ {error}</p>}

          <button
            type="submit"
            disabled={saving}
            style={{
              background: saving ? 'rgba(0,180,216,0.3)' : 'rgba(0,180,216,0.8)',
              color: '#fff', border: 'none', borderRadius: '10px',
              padding: '14px', fontSize: '1rem', cursor: saving ? 'not-allowed' : 'pointer', fontWeight: 600,
            }}
          >
            {saving ? 'Saving…' : species ? 'Update Species' : 'Create Species'}
          </button>
        </div>
      </form>
    </div>
  );
}

import { useState } from 'react';
import type { MarineAttribute, Category } from '../../types';
import api from '../../services/api';

type ManagedItem = MarineAttribute | Category;

interface Props {
  items: ManagedItem[];
  type: 'attribute' | 'category';
  onRefresh: () => void;
}

function isAttribute(item: ManagedItem): item is MarineAttribute {
  return 'dataType' in item;
}

export default function AttributeManager({ items, type, onRefresh }: Props) {
  const [editing, setEditing] = useState<ManagedItem | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [dataType, setDataType] = useState('string');
  const [saving, setSaving] = useState(false);
  const [showForm, setShowForm] = useState(false);

  const endpoint = type === 'attribute' ? '/api/admin/attributes' : '/api/admin/categories';
  const label = type === 'attribute' ? 'Attribute' : 'Category';

  const openCreate = () => {
    setEditing(null);
    setName('');
    setDescription('');
    setDataType('string');
    setShowForm(true);
  };

  const openEdit = (item: ManagedItem) => {
    setEditing(item);
    setName(item.name);
    setDescription(item.description);
    setDataType(isAttribute(item) ? item.dataType : 'string');
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm(`Delete this ${label.toLowerCase()}?`)) return;
    await api.delete(`${endpoint}/${id}`);
    onRefresh();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = type === 'attribute'
        ? { name, description, dataType }
        : { name, description };

      if (editing) {
        await api.put(`${endpoint}/${editing.id}`, payload);
      } else {
        await api.post(endpoint, payload);
      }
      setShowForm(false);
      onRefresh();
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

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <h2 style={{ color: '#caf0f8', margin: 0 }}>
          {type === 'attribute' ? '🏷️' : '📁'} {label}s ({items.length})
        </h2>
        <button
          onClick={openCreate}
          style={{
            background: 'rgba(0,180,216,0.7)', color: '#fff', border: 'none',
            borderRadius: '8px', padding: '10px 20px', cursor: 'pointer', fontWeight: 600,
          }}
        >
          + Add {label}
        </button>
      </div>

      {showForm && (
        <div style={{
          background: 'rgba(3,4,94,0.6)', border: '1px solid rgba(0,180,216,0.2)',
          borderRadius: '12px', padding: '20px', marginBottom: '20px',
        }}>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            <div>
              <label style={{ color: '#90e0ef', fontSize: '0.85rem', display: 'block', marginBottom: '6px' }}>
                Name *
              </label>
              <input style={inputStyle} value={name} onChange={(e) => setName(e.target.value)} required />
            </div>
            <div>
              <label style={{ color: '#90e0ef', fontSize: '0.85rem', display: 'block', marginBottom: '6px' }}>
                Description
              </label>
              <input style={inputStyle} value={description} onChange={(e) => setDescription(e.target.value)} />
            </div>
            {type === 'attribute' && (
              <div>
                <label style={{ color: '#90e0ef', fontSize: '0.85rem', display: 'block', marginBottom: '6px' }}>
                  Data Type
                </label>
                <select style={inputStyle} value={dataType} onChange={(e) => setDataType(e.target.value)}>
                  <option value="string">Text</option>
                  <option value="boolean">Yes/No</option>
                  <option value="number">Number</option>
                </select>
              </div>
            )}
            <div style={{ display: 'flex', gap: '8px' }}>
              <button
                type="submit"
                disabled={saving}
                style={{
                  background: 'rgba(0,180,216,0.7)', color: '#fff', border: 'none',
                  borderRadius: '8px', padding: '10px 20px', cursor: 'pointer',
                }}
              >
                {saving ? 'Saving…' : editing ? 'Update' : 'Create'}
              </button>
              <button
                type="button"
                onClick={() => setShowForm(false)}
                style={{
                  background: 'transparent', border: '1px solid rgba(0,180,216,0.3)',
                  color: '#90e0ef', borderRadius: '8px', padding: '10px 20px', cursor: 'pointer',
                }}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
        {items.map((item) => (
          <div
            key={item.id}
            style={{
              background: 'rgba(3,4,94,0.6)', border: '1px solid rgba(0,180,216,0.2)',
              borderRadius: '10px', padding: '16px',
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
            }}
          >
            <div>
              <h3 style={{ color: '#caf0f8', margin: '0 0 4px', fontSize: '1rem' }}>{item.name}</h3>
              <p style={{ color: '#90e0ef', margin: 0, fontSize: '0.85rem' }}>{item.description}</p>
              {isAttribute(item) && (
                <span style={{
                  display: 'inline-block', marginTop: '4px',
                  background: 'rgba(0,180,216,0.15)', borderRadius: '6px',
                  padding: '2px 8px', fontSize: '0.75rem', color: '#90e0ef',
                }}>
                  {item.dataType}
                </span>
              )}
            </div>
            <div style={{ display: 'flex', gap: '8px' }}>
              <button
                onClick={() => openEdit(item)}
                style={{
                  background: 'rgba(0,180,216,0.2)', border: '1px solid rgba(0,180,216,0.3)',
                  color: '#caf0f8', borderRadius: '6px', padding: '8px 12px', cursor: 'pointer',
                }}
              >
                ✏️
              </button>
              <button
                onClick={() => handleDelete(item.id)}
                style={{
                  background: 'rgba(239,68,68,0.1)', border: '1px solid rgba(239,68,68,0.3)',
                  color: '#fca5a5', borderRadius: '6px', padding: '8px 12px', cursor: 'pointer',
                }}
              >
                🗑️
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

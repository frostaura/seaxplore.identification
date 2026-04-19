import type { SearchResult } from '../../types';
import AttributeBadge from '../atoms/AttributeBadge';

const PLACEHOLDER = 'https://images.unsplash.com/photo-1518020382113-a7e8fc38eac9?w=400&q=80';

interface Props {
  result: SearchResult;
}

export default function SpeciesCard({ result }: Props) {
  const { species, similarityScore } = result;
  const score = Math.round(similarityScore * 100);

  return (
    <div
      style={{
        background: 'rgba(3,4,94,0.6)',
        border: '1px solid rgba(0,180,216,0.25)',
        borderRadius: '16px',
        overflow: 'hidden',
        display: 'flex',
        flexDirection: 'column',
        backdropFilter: 'blur(8px)',
        transition: 'transform 0.2s, box-shadow 0.2s',
      }}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLElement).style.transform = 'translateY(-4px)';
        (e.currentTarget as HTMLElement).style.boxShadow = '0 8px 32px rgba(0,180,216,0.3)';
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLElement).style.transform = 'translateY(0)';
        (e.currentTarget as HTMLElement).style.boxShadow = 'none';
      }}
    >
      <div style={{ position: 'relative' }}>
        <img
          src={species.imageUrl || PLACEHOLDER}
          alt={species.commonName}
          onError={(e) => { (e.target as HTMLImageElement).src = PLACEHOLDER; }}
          style={{ width: '100%', height: '200px', objectFit: 'cover' }}
        />
        <span
          style={{
            position: 'absolute',
            top: '10px',
            right: '10px',
            background: 'rgba(0,180,216,0.85)',
            color: '#fff',
            borderRadius: '12px',
            padding: '3px 10px',
            fontSize: '0.75rem',
            fontWeight: 700,
          }}
        >
          {score}% match
        </span>
        <span
          style={{
            position: 'absolute',
            top: '10px',
            left: '10px',
            background: 'rgba(3,4,94,0.8)',
            color: '#90e0ef',
            borderRadius: '12px',
            padding: '3px 10px',
            fontSize: '0.75rem',
          }}
        >
          {species.categoryName}
        </span>
      </div>

      <div style={{ padding: '16px', flex: 1, display: 'flex', flexDirection: 'column', gap: '8px' }}>
        <h3 style={{ margin: 0, color: '#caf0f8', fontSize: '1.1rem' }}>{species.commonName}</h3>
        <p style={{ margin: 0, fontSize: '0.8rem', color: '#90e0ef', fontStyle: 'italic' }}>
          {species.scientificName}
        </p>
        <p
          style={{
            margin: 0,
            fontSize: '0.85rem',
            color: '#ade8f4',
            lineHeight: 1.5,
            flex: 1,
            overflow: 'hidden',
            display: '-webkit-box',
            WebkitLineClamp: 3,
            WebkitBoxOrient: 'vertical',
          }}
        >
          {species.description}
        </p>
        <div style={{ marginTop: '8px' }}>
          {species.attributes.slice(0, 4).map((a) => (
            <AttributeBadge key={a.attributeId} label={a.attributeName} value={a.value} />
          ))}
        </div>
      </div>
    </div>
  );
}

interface BadgeProps {
  label: string;
  value: string;
}

export default function AttributeBadge({ label, value }: BadgeProps) {
  return (
    <span
      style={{
        display: 'inline-block',
        background: 'rgba(0,180,216,0.15)',
        border: '1px solid rgba(0,180,216,0.3)',
        borderRadius: '12px',
        padding: '3px 10px',
        fontSize: '0.75rem',
        color: '#caf0f8',
        margin: '2px',
      }}
    >
      <strong style={{ color: '#90e0ef' }}>{label}:</strong> {value}
    </span>
  );
}

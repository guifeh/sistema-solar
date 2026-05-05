interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export function Spinner({ size = 'md', className = '' }: SpinnerProps) {
  const sizes: Record<string, string> = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  };

  return (
    <div className={`flex items-center justify-center ${className}`}>
      <div
        className={`${sizes[size]} border-2 border-surface-700 border-t-solar-500 rounded-full`}
        style={{ animation: 'spin 0.8s linear infinite' }}
      />
    </div>
  );
}

export function PageSpinner() {
  return (
    <div className="flex items-center justify-center min-h-screen bg-surface-950">
      <div className="flex flex-col items-center gap-4 animate-fade-in">
        <Spinner size="lg" />
        <p className="text-surface-400 text-sm">Carregando...</p>
      </div>
    </div>
  );
}

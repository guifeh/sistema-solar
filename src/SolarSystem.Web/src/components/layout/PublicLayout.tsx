import { Outlet } from 'react-router-dom';
import { Sun } from 'lucide-react';

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-surface-950 flex flex-col">
      {/* Header */}
      <header className="h-16 flex items-center px-6 border-b border-surface-800/50 bg-surface-950/80 backdrop-blur-md">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl gradient-solar flex items-center justify-center shadow-lg">
            <Sun className="w-5 h-5 text-surface-900" />
          </div>
          <h1 className="text-lg font-bold text-surface-100">Sistema Solar</h1>
        </div>
      </header>

      {/* Content */}
      <main className="flex-1">
        <Outlet />
      </main>

      {/* Footer */}
      <footer className="py-6 px-6 border-t border-surface-800/50 text-center">
        <p className="text-xs text-surface-600">
          © {new Date().getFullYear()} Sistema Solar — Energia Fotovoltaica
        </p>
      </footer>
    </div>
  );
}

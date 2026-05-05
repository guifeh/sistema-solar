import { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  FileText,
  Settings,
  LogOut,
  Menu,
  X,
  Sun,
  ChevronRight,
} from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/leads', label: 'Leads', icon: Users },
  { to: '/propostas', label: 'Propostas', icon: FileText, disabled: true },
  { to: '/configuracoes', label: 'Configurações', icon: Settings, disabled: true },
];

export function AuthLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen overflow-hidden bg-surface-950">
      {/* Mobile overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/60 z-40 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          fixed inset-y-0 left-0 z-50 w-64 transform transition-transform duration-300 ease-out
          lg:relative lg:translate-x-0
          ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
          bg-surface-900/95 backdrop-blur-xl border-r border-surface-800
          flex flex-col
        `}
      >
        {/* Logo */}
        <div className="flex items-center gap-3 px-6 py-5 border-b border-surface-800">
          <div className="w-9 h-9 rounded-xl gradient-solar flex items-center justify-center shadow-lg">
            <Sun className="w-5 h-5 text-surface-900" />
          </div>
          <div>
            <h1 className="text-base font-bold text-surface-100">Sistema Solar</h1>
            <p className="text-[10px] text-surface-500 uppercase tracking-wider">Energia Fotovoltaica</p>
          </div>
          <button
            onClick={() => setSidebarOpen(false)}
            className="ml-auto lg:hidden text-surface-400 hover:text-surface-200"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.disabled ? '#' : item.to}
              onClick={(e) => {
                if (item.disabled) e.preventDefault();
                else setSidebarOpen(false);
              }}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 group
                ${item.disabled
                  ? 'text-surface-600 cursor-not-allowed'
                  : isActive
                    ? 'bg-solar-500/10 text-solar-400 border border-solar-500/20'
                    : 'text-surface-400 hover:text-surface-200 hover:bg-surface-800/60'
                }`
              }
            >
              <item.icon className="w-5 h-5 flex-shrink-0" />
              <span>{item.label}</span>
              {item.disabled && (
                <span className="ml-auto text-[10px] bg-surface-800 text-surface-500 px-1.5 py-0.5 rounded-md">
                  Em breve
                </span>
              )}
              {!item.disabled && (
                <ChevronRight className="w-4 h-4 ml-auto opacity-0 group-hover:opacity-100 transition-opacity" />
              )}
            </NavLink>
          ))}
        </nav>

        {/* User info */}
        <div className="p-3 border-t border-surface-800">
          <div className="flex items-center gap-3 px-3 py-2.5 rounded-xl bg-surface-800/40">
            <div className="w-8 h-8 rounded-lg gradient-solar flex items-center justify-center text-sm font-bold text-surface-900">
              {user?.name?.charAt(0).toUpperCase() || 'U'}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-surface-200 truncate">{user?.name || 'Usuário'}</p>
              <p className="text-xs text-surface-500 truncate">{user?.tenantName || 'Empresa'}</p>
            </div>
            <button
              onClick={handleLogout}
              title="Sair"
              className="p-1.5 rounded-lg text-surface-500 hover:text-red-400 hover:bg-red-500/10 transition-colors cursor-pointer"
            >
              <LogOut className="w-4 h-4" />
            </button>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Topbar */}
        <header className="h-16 flex items-center gap-4 px-6 border-b border-surface-800 bg-surface-950/80 backdrop-blur-md">
          <button
            onClick={() => setSidebarOpen(true)}
            className="lg:hidden p-2 rounded-lg text-surface-400 hover:text-surface-200 hover:bg-surface-800/60 transition-colors"
          >
            <Menu className="w-5 h-5" />
          </button>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto">
          <div className="p-6 max-w-7xl mx-auto animate-fade-in">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}

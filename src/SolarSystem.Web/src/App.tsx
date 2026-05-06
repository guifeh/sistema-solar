import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './routes/ProtectedRoute';
import { AuthLayout, PublicLayout } from './components/layout';
import { LoginPage } from './pages/auth/LoginPage';
import { RegisterPage } from './pages/auth/RegisterPage';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { LeadsPage } from './pages/leads/LeadsPage';
import { LeadDetailPage } from './pages/leads/LeadDetailPage';
import { ProposalPublicPage } from './pages/public/ProposalPublicPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
        <Routes>
          {/* Auth pages (no layout wrapper — full-page design) */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Protected routes with authenticated layout */}
          <Route
            element={
              <ProtectedRoute>
                <AuthLayout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/leads" element={<LeadsPage />} />
            <Route path="/leads/:id" element={<LeadDetailPage />} />
          </Route>

          {/* Public routes (proposal link) */}
          <Route element={<PublicLayout />}>
            <Route path="/proposta/:token" element={<ProposalPublicPage />} />
          </Route>

          {/* Redirect root to dashboard */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
    <ReactQueryDevtools initialIsOpen={false} />
  </QueryClientProvider>
  );
}

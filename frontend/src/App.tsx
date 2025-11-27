import React, { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext.tsx';
import { LoginForm } from './components/auth/LoginForm.tsx';
import { RegisterForm } from './components/auth/RegisterForm.tsx';
import { ProtectedRoute } from './components/auth/ProtectedRoute.tsx';
import { ProsessListe } from './components/prosess/ProsessListe.tsx';
import { ProsessDetail } from './components/prosess/ProsessDetail.tsx';
import { CreateProsessForm } from './components/prosess/CreateProsessForm.tsx';
import { ProsessEditor } from './components/prosess/ProsessEditor.tsx';
import { ApprovalQueue } from './components/approval/ApprovalQueue.tsx';
import { DeletedProcessesList } from './components/deletion/DeletedProcessesList.tsx';
import { ActorManagement } from './components/actors/ActorManagement.tsx';
import { RoleManagement } from './components/roles/RoleManagement.tsx';
import { ForsvaretLogo } from './components/common/ForsvaretLogo.tsx';
import './components/auth/auth.css';
import './components/prosess/prosess.css';
import './components/actors/actors.css';
import './components/roles/roles.css';
import './App.css';

// Navigation Component
const Navigation: React.FC = () => {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <nav className="app-nav">
      <ul>
        <li>
          <Link to="/prosesser">📋 Operasjonsprosedyrer</Link>
        </li>
        
        <ProtectedRoute requiredPermission="view_qa_queue">
          <li><Link to="/godkjenning">✅ Til kvalitetssikring</Link></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredPermission="delete_prosess">
          <li><Link to="/slettede-prosesser">🗑️ Arkiverte prosedyrer</Link></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredRole="ProsessEier">
          <li><a href="#mine-prosesser">📝 Mine prosedyrer</a></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredRole="Admin">
          <li><Link to="/aktorer">👥 Aktører</Link></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredPermission="manage_roles">
          <li><Link to="/roller">🛡️ Roller og Tilganger</Link></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredRole="Admin">
          <li><a href="#admin">⚙️ Systemadministrasjon</a></li>
        </ProtectedRoute>
      </ul>
    </nav>
  );
};

// Main App Content Component
const AppContent: React.FC = () => {
  const { user, logout } = useAuth();
  const [showRegister, setShowRegister] = useState(false);

  if (!user) {
    return (
      <div className="app">
        <header className="app-header">
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '1rem', flexDirection: 'column' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <ForsvaretLogo size="large" />
              <h1>Forsvarets Prosessportal</h1>
            </div>
            <p>Sikker prosedyrehåndtering med AI-assistanse</p>
          </div>
        </header>
        <main>
          {showRegister ? (
            <RegisterForm 
              onRegisterSuccess={() => setShowRegister(false)}
              onSwitchToLogin={() => setShowRegister(false)}
            />
          ) : (
            <LoginForm 
              onSwitchToRegister={() => setShowRegister(true)}
            />
          )}
        </main>
      </div>
    );
  }

  return (
    <BrowserRouter>
      <div className="app">
        <header className="app-header">
          <div className="header-content">
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <ForsvaretLogo size="medium" />
              <h1>Forsvarets Prosessportal</h1>
            </div>
            <div className="user-info">
              <span>Innlogget: {user.firstName} {user.lastName}</span>
              <span className="user-roles">
                {user.roles.map(role => (
                  <span key={role} className="role-badge">{role}</span>
                ))}
              </span>
              <button onClick={logout} className="logout-button">
                Logg ut
              </button>
            </div>
          </div>
        </header>

        <Navigation />

        <main className="app-main">
          <Routes>
            <Route path="/" element={<Navigate to="/prosesser" replace />} />
            <Route path="/prosesser" element={<ProsessListe />} />
            <Route path="/prosess/:id" element={<ProsessDetail />} />
            <Route path="/prosess/:id/rediger" element={<ProsessEditor />} />
            <Route path="/opprett-prosess" element={<CreateProsessForm />} />
            <Route 
              path="/godkjenning" 
              element={
                <ProtectedRoute requiredPermission="view_qa_queue">
                  <ApprovalQueue />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/slettede-prosesser" 
              element={
                <ProtectedRoute requiredPermission="delete_prosess">
                  <DeletedProcessesList />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/aktorer" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <ActorManagement />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/roller" 
              element={
                <ProtectedRoute requiredPermission="manage_roles">
                  <RoleManagement />
                </ProtectedRoute>
              } 
            />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
};

// Main App Component
const App: React.FC = () => {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
};

export default App;
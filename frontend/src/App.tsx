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
import TestComponent from './TestComponent.tsx';
import './components/auth/auth.css';
import './components/prosess/prosess.css';
import './App.css';

// Navigation Component
const Navigation: React.FC = () => {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <nav className="app-nav">
      <ul>
        <li>
          <Link to="/prosesser">📋 Prosesser</Link>
        </li>
        
        <ProtectedRoute requiredPermission="view_qa_queue">
          <li><Link to="/godkjenning">✅ Til godkjenning</Link></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredRole="ProsessEier">
          <li><a href="#mine-prosesser">📝 Mine prosesser</a></li>
        </ProtectedRoute>
        
        <ProtectedRoute requiredRole="Admin">
          <li><a href="#admin">⚙️ Administrasjon</a></li>
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
          <h1>🏢 Prosessportal</h1>
          <p>Intelligent prosesshåndtering med AI-støtte</p>
        </header>
        <main>
          <TestComponent />
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
            <h1>🏢 Prosessportal</h1>
            <div className="user-info">
              <span>Velkommen, {user.firstName} {user.lastName}</span>
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
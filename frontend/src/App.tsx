import React, { useState } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { LoginForm } from './components/auth/LoginForm';
import { RegisterForm } from './components/auth/RegisterForm';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import './components/auth/auth.css';
import './App.css';

// Main App Content Component
const AppContent: React.FC = () => {
  const { user, logout, hasRole } = useAuth();
  const [showRegister, setShowRegister] = useState(false);

  if (!user) {
    return (
      <div className="app">
        <header className="app-header">
          <h1>🏢 Prosessportal</h1>
          <p>Intelligent prosesshåndtering med AI-støtte</p>
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

      <nav className="app-nav">
        <ul>
          <li><a href="#prosesser">📋 Prosesser</a></li>
          
          <ProtectedRoute requiredRole="QA">
            <li><a href="#qa-ko">✅ QA-kø</a></li>
          </ProtectedRoute>
          
          <ProtectedRoute requiredRole="ProsessEier">
            <li><a href="#mine-prosesser">📝 Mine prosesser</a></li>
          </ProtectedRoute>
          
          <ProtectedRoute requiredRole="Admin">
            <li><a href="#admin">⚙️ Administrasjon</a></li>
          </ProtectedRoute>
        </ul>
      </nav>

      <main className="app-main">
        <div className="welcome-content">
          <h2>Velkommen til Prosessportalen</h2>
          <p>Din personlige dashboard for prosesshåndtering.</p>
          
          <div className="features-grid">
            <div className="feature-card">
              <h3>📋 Prosessoversikt</h3>
              <p>Se og søk i alle tilgjengelige prosesser</p>
            </div>
            
            {hasRole('QA') && (
              <div className="feature-card">
                <h3>✅ QA & Godkjenning</h3>
                <p>Håndter godkjenninger med AI-støtte</p>
              </div>
            )}
            
            {hasRole('ProsessEier') && (
              <div className="feature-card">
                <h3>🤖 AI-generering</h3>
                <p>Generer nye prosesser med kunstig intelligens</p>
              </div>
            )}
            
            {hasRole('Admin') && (
              <div className="feature-card">
                <h3>⚙️ Administrasjon</h3>
                <p>Administrer brukere og systemkonfigurasjon</p>
              </div>
            )}
          </div>

          <div className="status-info">
            <h3>Status</h3>
            <p>✅ Backend API: Tilkoblet</p>
            <p>✅ Autentisering: Aktiv</p>
            <p>🔧 AI-agenter: Under utvikling</p>
            <p>🔧 Git-integrasjon: Under utvikling</p>
          </div>
        </div>
      </main>
    </div>
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
import React, { useState } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext.tsx';
import { LoginForm } from './components/auth/LoginForm.tsx';
import { RegisterForm } from './components/auth/RegisterForm.tsx';
import { ProtectedRoute } from './components/auth/ProtectedRoute.tsx';
import { ProsessListe } from './components/prosess/ProsessListe.tsx';
import { ProsessDetaljer } from './components/prosess/ProsessDetaljer.tsx';
import { CreateProsessForm } from './components/prosess/CreateProsessForm.tsx';
import './components/auth/auth.css';
import './components/prosess/prosess.css';
import './App.css';

// Main App Content Component
const AppContent: React.FC = () => {
  const { user, logout, hasRole } = useAuth();
  const [showRegister, setShowRegister] = useState(false);
  const [currentView, setCurrentView] = useState<'prosesser' | 'prosess-detail' | 'create-prosess'>('prosesser');
  const [selectedProsessId, setSelectedProsessId] = useState<number | null>(null);

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
          <li>
            <button 
              onClick={() => setCurrentView('prosesser')}
              className={currentView === 'prosesser' ? 'active' : ''}
            >
              📋 Prosesser
            </button>
          </li>
          
          <ProtectedRoute requiredRole="QA">
            <li><a href="#qa-ko">✅ Til godkjenning</a></li>
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
        {currentView === 'prosesser' && (
          <ProsessListe 
            onProsessClick={(prosessId) => {
              setSelectedProsessId(prosessId);
              setCurrentView('prosess-detail');
            }}
            onCreateProsess={() => setCurrentView('create-prosess')}
          />
        )}
        
        {currentView === 'prosess-detail' && selectedProsessId && (
          <ProsessDetaljer 
            prosessId={selectedProsessId}
            onBack={() => setCurrentView('prosesser')}
          />
        )}
        
        {currentView === 'create-prosess' && (
          <CreateProsessForm 
            onSuccess={(prosessId) => {
              setSelectedProsessId(prosessId);
              setCurrentView('prosess-detail');
            }}
            onCancel={() => setCurrentView('prosesser')}
          />
        )}
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
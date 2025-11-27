import React, { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
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
import DropdownNavigation from './components/navigation/DropdownNavigation.tsx';
import { ForsvaretLogo } from './components/common/ForsvaretLogo.tsx';
import './components/auth/auth.css';
import './components/prosess/prosess.css';
import './components/actors/actors.css';
import './components/roles/roles.css';
import './components/navigation/navigation.css';
import './App.css';

// Placeholder for system administration pages
const SystemSettings: React.FC = () => (
  <div style={{ padding: '2rem', textAlign: 'center' }}>
    <h2>🔧 Systeminnstillinger</h2>
    <p>Systemkonfigurasjon og innstillinger vil implementeres her.</p>
  </div>
);

const SystemReports: React.FC = () => (
  <div style={{ padding: '2rem', textAlign: 'center' }}>
    <h2>📊 Systemrapporter</h2>
    <p>Systemrapporter og analytics vil implementeres her.</p>
  </div>
);

const AuditLogs: React.FC = () => (
  <div style={{ padding: '2rem', textAlign: 'center' }}>
    <h2>🔍 Audit Logs</h2>
    <p>Audit logg og systemovervåking vil implementeres her.</p>
  </div>
);

const MyProcesses: React.FC = () => (
  <div style={{ padding: '2rem', textAlign: 'center' }}>
    <h2>📝 Mine prosedyrer</h2>
    <p>Oversikt over prosedyrer du eier eller er ansvarlig for.</p>
  </div>
);

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
          </div>
        </header>

        <DropdownNavigation />

        <main className="app-main">
          <Routes>
            {/* Home/Default routes */}
            <Route path="/" element={<Navigate to="/prosesser" replace />} />
            <Route path="/prosesser" element={<ProsessListe />} />
            
            {/* Process detail routes */}
            <Route path="/prosess/:id" element={<ProsessDetail />} />
            <Route path="/prosess/:id/rediger" element={<ProsessEditor />} />
            
            {/* Process Administration routes */}
            <Route 
              path="/prosessadministrasjon/godkjenning" 
              element={
                <ProtectedRoute requiredPermission="view_qa_queue">
                  <ApprovalQueue />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/prosessadministrasjon/opprett" 
              element={
                <ProtectedRoute requiredPermission="create_prosess">
                  <CreateProsessForm />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/prosessadministrasjon/mine" 
              element={
                <ProtectedRoute requiredRole="ProsessEier">
                  <MyProcesses />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/prosessadministrasjon/arkiverte" 
              element={
                <ProtectedRoute requiredPermission="delete_prosess">
                  <DeletedProcessesList />
                </ProtectedRoute>
              } 
            />
            
            {/* User Administration routes */}
            <Route 
              path="/brukeradministrasjon/aktorer" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <ActorManagement />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/brukeradministrasjon/roller" 
              element={
                <ProtectedRoute requiredPermission="manage_roles">
                  <RoleManagement />
                </ProtectedRoute>
              } 
            />
            
            {/* System Administration routes */}
            <Route 
              path="/systemadministrasjon/innstillinger" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <SystemSettings />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/systemadministrasjon/rapporter" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <SystemReports />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="/systemadministrasjon/audit" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <AuditLogs />
                </ProtectedRoute>
              } 
            />
            
            {/* Legacy redirects for backward compatibility */}
            <Route path="/opprett-prosess" element={<Navigate to="/prosessadministrasjon/opprett" replace />} />
            <Route path="/godkjenning" element={<Navigate to="/prosessadministrasjon/godkjenning" replace />} />
            <Route path="/slettede-prosesser" element={<Navigate to="/prosessadministrasjon/arkiverte" replace />} />
            <Route path="/aktorer" element={<Navigate to="/brukeradministrasjon/aktorer" replace />} />
            <Route path="/roller" element={<Navigate to="/brukeradministrasjon/roller" replace />} />
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
import React, { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { LoginRequest } from '../../types/auth';

interface LoginFormProps {
  onLoginSuccess?: () => void;
  onSwitchToRegister?: () => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onLoginSuccess, onSwitchToRegister }) => {
  const { login, isLoading } = useAuth();
  const [credentials, setCredentials] = useState<LoginRequest>({
    username: '',
    password: ''
  });
  const [error, setError] = useState<string>('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!credentials.username.trim() || !credentials.password.trim()) {
      setError('Vennligst fyll ut alle felter');
      return;
    }

    const success = await login(credentials);
    if (success) {
      onLoginSuccess?.();
    } else {
      setError('Ugyldig brukernavn eller passord');
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setCredentials(prev => ({
      ...prev,
      [name]: value
    }));
    // Clear error when user starts typing
    if (error) setError('');
  };

  return (
    <div className="login-form">
      <div className="form-container">
        <h2>Logg inn</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Brukernavn:</label>
            <input
              type="text"
              id="username"
              name="username"
              value={credentials.username}
              onChange={handleInputChange}
              disabled={isLoading}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Passord:</label>
            <input
              type="password"
              id="password"
              name="password"
              value={credentials.password}
              onChange={handleInputChange}
              disabled={isLoading}
              required
            />
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <button 
            type="submit" 
            disabled={isLoading}
            className="submit-button"
          >
            {isLoading ? 'Logger inn...' : 'Logg inn'}
          </button>
        </form>

        {onSwitchToRegister && (
          <div className="switch-form">
            <p>
              Har du ikke en bruker?{' '}
              <button 
                type="button" 
                onClick={onSwitchToRegister}
                className="link-button"
              >
                Registrer deg her
              </button>
            </p>
          </div>
        )}

        <div className="demo-info">
          <h4>Demo brukere:</h4>
          <p><strong>Admin:</strong> admin / admin123</p>
          <p><em>Du kan også registrere en ny bruker</em></p>
        </div>
      </div>
    </div>
  );
};
import React, { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext.tsx';
import { RegisterRequest } from '../../types/auth.ts';

interface RegisterFormProps {
  onRegisterSuccess?: () => void;
  onSwitchToLogin?: () => void;
}

export const RegisterForm: React.FC<RegisterFormProps> = ({ onRegisterSuccess, onSwitchToLogin }) => {
  const { register, isLoading } = useAuth();
  const [userData, setUserData] = useState<RegisterRequest>({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: ''
  });
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string>('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Validation
    if (!userData.username.trim() || !userData.email.trim() || !userData.password.trim() || 
        !userData.firstName.trim() || !userData.lastName.trim()) {
      setError('Vennligst fyll ut alle felter');
      return;
    }

    if (userData.password !== confirmPassword) {
      setError('Passordene stemmer ikke overens');
      return;
    }

    if (userData.password.length < 6) {
      setError('Passord må være minst 6 tegn');
      return;
    }

    if (!isValidEmail(userData.email)) {
      setError('Ugyldig e-postadresse');
      return;
    }

    const success = await register(userData);
    if (success) {
      onRegisterSuccess?.();
    } else {
      setError('Registrering feilet. Brukernavnet eller e-postadressen kan allerede være i bruk.');
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    
    if (name === 'confirmPassword') {
      setConfirmPassword(value);
    } else {
      setUserData(prev => ({
        ...prev,
        [name]: value
      }));
    }
    
    // Clear error when user starts typing
    if (error) setError('');
  };

  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  return (
    <div className="register-form">
      <div className="form-container">
        <h2>Registrer bruker</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">Fornavn:</label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={userData.firstName}
                onChange={handleInputChange}
                disabled={isLoading}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Etternavn:</label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={userData.lastName}
                onChange={handleInputChange}
                disabled={isLoading}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="username">Brukernavn:</label>
            <input
              type="text"
              id="username"
              name="username"
              value={userData.username}
              onChange={handleInputChange}
              disabled={isLoading}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">E-post:</label>
            <input
              type="email"
              id="email"
              name="email"
              value={userData.email}
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
              value={userData.password}
              onChange={handleInputChange}
              disabled={isLoading}
              required
              minLength={6}
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Bekreft passord:</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={confirmPassword}
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
            {isLoading ? 'Registrerer...' : 'Registrer'}
          </button>
        </form>

        {onSwitchToLogin && (
          <div className="switch-form">
            <p>
              Har du allerede en bruker?{' '}
              <button 
                type="button" 
                onClick={onSwitchToLogin}
                className="link-button"
              >
                Logg inn her
              </button>
            </p>
          </div>
        )}
      </div>
    </div>
  );
};
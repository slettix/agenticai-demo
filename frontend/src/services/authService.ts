import { LoginRequest, LoginResponse, RegisterRequest, User } from '../types/auth';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

class AuthService {
  private tokenKey = 'prosessportal_token';
  private refreshTokenKey = 'prosessportal_refresh_token';
  private userKey = 'prosessportal_user';

  async login(credentials: LoginRequest): Promise<LoginResponse | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const loginResponse: LoginResponse = await response.json();
      this.setTokens(loginResponse.accessToken, loginResponse.refreshToken);
      this.setUser(loginResponse.user);
      
      return loginResponse;
    } catch (error) {
      console.error('Login error:', error);
      return null;
    }
  }

  async register(userData: RegisterRequest): Promise<User | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
      });

      if (!response.ok) {
        throw new Error('Registration failed');
      }

      const user: User = await response.json();
      return user;
    } catch (error) {
      console.error('Registration error:', error);
      return null;
    }
  }

  async getCurrentUser(): Promise<User | null> {
    try {
      const token = this.getToken();
      if (!token) return null;

      const response = await fetch(`${API_BASE_URL}/auth/me`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        if (response.status === 401) {
          this.clearAuth();
        }
        throw new Error('Failed to get current user');
      }

      const user: User = await response.json();
      this.setUser(user);
      return user;
    } catch (error) {
      console.error('Get current user error:', error);
      return null;
    }
  }

  logout(): void {
    this.clearAuth();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getUser(): User | null {
    const userStr = localStorage.getItem(this.userKey);
    return userStr ? JSON.parse(userStr) : null;
  }

  private setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.tokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }

  private setUser(user: User): void {
    localStorage.setItem(this.userKey, JSON.stringify(user));
  }

  private clearAuth(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token;
  }

  hasPermission(permission: string): boolean {
    const user = this.getUser();
    return user?.permissions.includes(permission) ?? false;
  }

  hasRole(role: string): boolean {
    const user = this.getUser();
    return user?.roles.includes(role) ?? false;
  }
}

export const authService = new AuthService();
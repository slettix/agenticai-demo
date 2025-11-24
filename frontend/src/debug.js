console.log('Environment variables:');
console.log('REACT_APP_API_URL:', process.env.REACT_APP_API_URL);
console.log('Current API URLs in services:');

// Import and log the actual values
import('./services/authService.ts').then(module => {
  console.log('AuthService loaded');
}).catch(err => {
  console.error('Error loading authService:', err);
});
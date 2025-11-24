import React from 'react';

const TestComponent: React.FC = () => {
  const apiUrl = 'http://localhost:5001/api';
  
  const testLogin = async () => {
    console.log('Using API URL:', apiUrl);
    
    try {
      const response = await fetch(`${apiUrl}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: 'admin',
          password: 'admin123'
        }),
      });

      console.log('Response:', response);
      
      if (response.ok) {
        const data = await response.json();
        console.log('Login successful:', data);
        alert('Login successful!');
      } else {
        console.log('Login failed:', response.status, response.statusText);
        const errorText = await response.text();
        alert(`Login failed: ${response.status} - ${errorText}`);
      }
    } catch (error) {
      console.error('Error:', error);
      alert(`Error: ${error}`);
    }
  };

  return (
    <div style={{ padding: '20px', border: '1px solid #ccc', margin: '20px' }}>
      <h3>Test Component</h3>
      <p>API URL: {apiUrl}</p>
      <button onClick={testLogin}>Test Login</button>
    </div>
  );
};

export default TestComponent;
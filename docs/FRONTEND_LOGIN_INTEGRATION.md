# 🔐 **Documentación de Integración de Login - Frontend**

## **📋 Tabla de Contenidos**
- [1. Endpoints de Autenticación](#1-endpoints-de-autenticación)
- [2. Flujo de Autenticación](#2-flujo-de-autenticación)
- [3. Implementación en JavaScript/TypeScript](#3-implementación-en-javascripttypescript)
- [4. Ejemplo de Uso en Componentes](#4-ejemplo-de-uso-en-componentes)
- [5. Configuración del App](#5-configuración-del-app)
- [6. Credenciales de Prueba](#6-credenciales-de-prueba)
- [7. Manejo de Errores](#7-manejo-de-errores)
- [8. Seguridad y Mejores Prácticas](#8-seguridad-y-mejores-prácticas)

---

## **1. Endpoints de Autenticación**

### **Base URL**
```bash
# Producción
https://million-real-estate-api-sh25jnp3aa-uc.a.run.app

# Local (desarrollo)
http://localhost:5209
```

### **Endpoints Disponibles**
```bash
POST /auth/owner/login      # Login principal
POST /auth/owner/refresh    # Refresh de token
POST /auth/owner/logout     # Logout
```

---

## **2. Flujo de Autenticación**

### **🔄 Flujo Completo**
```
1. Usuario ingresa credenciales → POST /auth/owner/login
2. API valida y retorna JWT + Refresh Token
3. Frontend almacena tokens
4. Usa JWT para requests protegidos
5. Cuando JWT expira → POST /auth/owner/refresh
6. Logout → POST /auth/owner/logout
```

### **📊 Diagrama de Estados**
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   No Auth  │───▶│  Authenticated │───▶│   Expired   │
└─────────────┘    └─────────────┘    └─────────────┘
       ▲                   │                   │
       │                   ▼                   ▼
       └─────────────┐    ┌─────────────┐    ┌─────────────┐
                     │    │   Refresh   │    │   Logout    │
                     └────┼─────────────┼────┼─────────────┼
                          └─────────────┘    └─────────────┘
```

---

## **3. Implementación en JavaScript/TypeScript**

### **📁 Estructura de Archivos Recomendada**
```
src/
├── services/
│   ├── authService.ts      # Lógica de autenticación
│   └── apiService.ts       # Cliente HTTP con interceptors
├── hooks/
│   └── useAuth.ts          # Hook de React para auth
├── context/
│   └── AuthContext.tsx     # Contexto de autenticación
└── utils/
    └── tokenStorage.ts     # Manejo de tokens
```

### **🔐 AuthService (TypeScript)**

```typescript
interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType: string;
}

class AuthService {
  private baseUrl = 'https://million-real-estate-api-sh25jnp3aa-uc.a.run.app';
  private accessToken: string | null = null;
  private refreshToken: string | null = null;

  // Login principal
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    try {
      const response = await fetch(`${this.baseUrl}/auth/owner/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.detail || 'Login failed');
      }

      const data: LoginResponse = await response.json();
      
      // Almacenar tokens
      this.accessToken = data.accessToken;
      this.refreshToken = data.refreshToken;
      
      // Guardar en localStorage
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('tokenExpiry', data.expiresAt);

      return data;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  // Refresh de token
  async refreshToken(): Promise<LoginResponse> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await fetch(`${this.baseUrl}/auth/owner/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) {
        throw new Error('Token refresh failed');
      }

      const data: LoginResponse = await response.json();
      
      // Actualizar tokens
      this.accessToken = data.accessToken;
      this.refreshToken = data.refreshToken;
      
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('tokenExpiry', data.expiresAt);

      return data;
    } catch (error) {
      console.error('Token refresh error:', error);
      this.logout();
      throw error;
    }
  }

  // Logout
  async logout(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        await fetch(`${this.baseUrl}/auth/owner/logout`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ refreshToken }),
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      // Limpiar tokens
      this.accessToken = null;
      this.refreshToken = null;
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('tokenExpiry');
    }
  }

  // Obtener token actual
  getAccessToken(): string | null {
    if (!this.accessToken) {
      this.accessToken = localStorage.getItem('accessToken');
    }
    return this.accessToken;
  }

  // Verificar si el token está expirado
  isTokenExpired(): boolean {
    const expiry = localStorage.getItem('tokenExpiry');
    if (!expiry) return true;
    
    const expiryDate = new Date(expiry);
    const now = new Date();
    
    // Considerar expirado 5 minutos antes
    return now >= expiryDate;
  }

  // Verificar si está autenticado
  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    return !!token && !this.isTokenExpired();
  }
}

export const authService = new AuthService();
```

### **🌐 ApiService con Interceptors**

```typescript
class ApiService {
  private baseUrl = 'https://million-real-estate-api-sh25jnp3aa-uc.a.run.app';
  private authService: AuthService;

  constructor(authService: AuthService) {
    this.authService = authService;
  }

  // Request con auto-refresh de token
  async request<T>(
    endpoint: string, 
    options: RequestInit = {}
  ): Promise<T> {
    // Agregar token de autorización
    const token = this.authService.getAccessToken();
    if (token) {
      options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${token}`,
      };
    }

    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, options);
      
      // Si es 401, intentar refresh
      if (response.status === 401) {
        await this.authService.refreshToken();
        
        // Reintentar con nuevo token
        const newToken = this.authService.getAccessToken();
        if (newToken) {
          options.headers = {
            ...options.headers,
            'Authorization': `Bearer ${newToken}`,
          };
          
          const retryResponse = await fetch(`${this.baseUrl}${endpoint}`, options);
          if (retryResponse.ok) {
            return await retryResponse.json();
          }
        }
      }

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      console.error('API request error:', error);
      throw error;
    }
  }

  // Métodos específicos
  async getProperties(page = 1, pageSize = 10) {
    return this.request(`/properties?page=${page}&pageSize=${pageSize}`);
  }

  async getOwners() {
    return this.request('/owners');
  }

  async getPropertyById(id: string) {
    return this.request(`/properties/${id}`);
  }
}

export const apiService = new ApiService(authService);
```

### **🎣 Hook de React (useAuth)**

```typescript
import { useState, useEffect, useCallback } from 'react';
import { authService } from '../services/authService';

export const useAuth = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState(null);

  useEffect(() => {
    // Verificar estado de autenticación al cargar
    const checkAuth = () => {
      const authenticated = authService.isAuthenticated();
      setIsAuthenticated(authenticated);
      setIsLoading(false);
    };

    checkAuth();
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    try {
      setIsLoading(true);
      const response = await authService.login({ email, password });
      setIsAuthenticated(true);
      return response;
    } catch (error) {
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      setIsLoading(true);
      await authService.logout();
      setIsAuthenticated(false);
      setUser(null);
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const refreshToken = useCallback(async () => {
    try {
      const response = await authService.refreshToken();
      setIsAuthenticated(true);
      return response;
    } catch (error) {
      setIsAuthenticated(false);
      throw error;
    }
  }, []);

  return {
    isAuthenticated,
    isLoading,
    user,
    login,
    logout,
    refreshToken,
  };
};
```

### **🔗 Contexto de Autenticación**

```typescript
import React, { createContext, useContext, ReactNode } from 'react';
import { useAuth } from '../hooks/useAuth';

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: any;
  login: (email: string, password: string) => Promise<any>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<any>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const auth = useAuth();

  return (
    <AuthContext.Provider value={auth}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuthContext = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuthContext must be used within an AuthProvider');
  }
  return context;
};
```

---

## **4. Ejemplo de Uso en Componentes**

### **🚪 Componente de Login**

```typescript
import React, { useState } from 'react';
import { useAuthContext } from '../context/AuthContext';

export const LoginForm: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login, isLoading } = useAuthContext();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      await login(email, password);
      // Redirigir o actualizar estado
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Login failed');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        placeholder="Email"
        required
      />
      <input
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        placeholder="Password"
        required
      />
      {error && <div className="error">{error}</div>}
      <button type="submit" disabled={isLoading}>
        {isLoading ? 'Logging in...' : 'Login'}
      </button>
    </form>
  );
};
```

### **🛡️ Componente Protegido**

```typescript
import React from 'react';
import { useAuthContext } from '../context/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuthContext();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    return <div>Please log in to access this page</div>;
  }

  return <>{children}</>;
};
```

---

## **5. Configuración del App**

### **📱 App.tsx**

```typescript
import React from 'react';
import { AuthProvider } from './context/AuthContext';
import { LoginForm } from './components/LoginForm';
import { Dashboard } from './components/Dashboard';
import { ProtectedRoute } from './components/ProtectedRoute';

function App() {
  return (
    <AuthProvider>
      <div className="App">
        <LoginForm />
        <ProtectedRoute>
          <Dashboard />
        </ProtectedRoute>
      </div>
    </AuthProvider>
  );
}

export default App;
```

---

## **6. Credenciales de Prueba**

### **👥 Usuarios Disponibles en el Seed**

```json
{
  "sarah-johnson": {
    "email": "sarah.johnson@millionrealestate.com",
    "password": "test1234",
    "role": "CEO & Founder"
  },
  "michael-chen": {
    "email": "michael.chen@millionrealestate.com", 
    "password": "test1234",
    "role": "Head of Sales"
  },
  "david-thompson": {
    "email": "david.thompson@millionrealestate.com",
    "password": "test1234", 
    "role": "Investment Advisor"
  }
}
```

### **🧪 Probar Login con cURL**

```bash
# Login
curl -X POST https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/auth/owner/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "sarah.johnson@millionrealestate.com",
    "password": "test1234"
  }'

# Response esperado:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresAt": "2024-01-16T10:30:00Z",
  "tokenType": "Bearer"
}
```

---

## **7. Manejo de Errores**

### **🚨 Códigos de Error Comunes**

```typescript
// 400 - Validation Failed
// 401 - Invalid Credentials / Unauthorized
// 429 - Account Locked (Too Many Attempts)
// 500 - Internal Server Error

const handleApiError = (error: any) => {
  if (error.status === 401) {
    // Token expirado o inválido
    authService.logout();
    // Redirigir a login
  } else if (error.status === 429) {
    // Cuenta bloqueada
    alert('Account temporarily locked. Please try again later.');
  } else {
    // Otros errores
    console.error('API Error:', error);
  }
};
```

### **📝 Ejemplos de Respuestas de Error**

```json
// 400 - Validation Failed
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The Email field is required; The Password field is required"
}

// 401 - Invalid Credentials
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Email or password is incorrect"
}

// 429 - Account Locked
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Account locked due to multiple failed login attempts"
}
```

---

## **8. Seguridad y Mejores Prácticas**

### **🔒 Recomendaciones de Seguridad**

- ✅ **HTTPS**: Siempre usar en producción
- ✅ **Token Storage**: localStorage para desarrollo, httpOnly cookies para producción
- ✅ **Auto-refresh**: Implementar refresh automático antes de expiración
- ✅ **Logout**: Limpiar tokens al cerrar sesión
- ✅ **Error Handling**: Manejar errores de autenticación gracefully
- ✅ **Loading States**: Mostrar estados de carga durante operaciones

### **⚠️ Consideraciones de Seguridad**

```typescript
// ❌ NO hacer esto:
localStorage.setItem('password', password); // Nunca guardar contraseñas
console.log('Token:', token); // No loggear tokens en producción

// ✅ Hacer esto:
localStorage.setItem('accessToken', token);
localStorage.setItem('tokenExpiry', expiry);
// Usar httpOnly cookies en producción
```

### **🔄 Auto-refresh de Token**

```typescript
// Implementar refresh automático
useEffect(() => {
  const checkTokenExpiry = () => {
    if (authService.isTokenExpired()) {
      authService.refreshToken();
    }
  };

  // Verificar cada minuto
  const interval = setInterval(checkTokenExpiry, 60000);
  
  return () => clearInterval(interval);
}, []);
```

---

## **9. Testing**

### **🧪 Tests Unitarios**

```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { LoginForm } from './LoginForm';
import { AuthProvider } from '../context/AuthContext';

describe('LoginForm', () => {
  it('should handle successful login', async () => {
    render(
      <AuthProvider>
        <LoginForm />
      </AuthProvider>
    );

    fireEvent.change(screen.getByPlaceholderText('Email'), {
      target: { value: 'test@example.com' },
    });
    fireEvent.change(screen.getByPlaceholderText('Password'), {
      target: { value: 'password123' },
    });

    fireEvent.click(screen.getByText('Login'));

    await waitFor(() => {
      expect(screen.getByText('Logging in...')).toBeInTheDocument();
    });
  });
});
```

---

## **10. Deployment y Configuración**

### **🌍 Variables de Entorno**

```bash
# .env.local
REACT_APP_API_BASE_URL=https://million-real-estate-api-sh25jnp3aa-uc.a.run.app
REACT_APP_ENVIRONMENT=production
```

### **📦 Build de Producción**

```bash
# React
npm run build

# Next.js
npm run build && npm start

# Vite
npm run build
```

---

## **📚 Recursos Adicionales**

- [JWT.io](https://jwt.io/) - Debugger de JWT
- [MDN Web Docs - Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)
- [React Context API](https://react.dev/reference/react/createContext)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)

---

## **🤝 Soporte**

Si tienes alguna pregunta o necesitas ayuda con la implementación:

1. **Revisa los logs del servidor** para errores específicos
2. **Verifica las credenciales** en el seed de MongoDB
3. **Confirma la conectividad** a la API
4. **Revisa la consola del navegador** para errores de CORS o red

---

**🎯 ¡Listo para implementar!** Esta documentación te proporciona todo lo necesario para integrar el sistema de autenticación en tu frontend.

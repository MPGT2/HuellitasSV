import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api } from '../services/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const login = useCallback(async (correo, contrasena) => {
    setError(null);
    try {
      const response = await api.login(correo, contrasena);

      const userData = {
        idCuenta: response.idCuenta,
        rol: response.rol,
        nombre: response.nombre,
        // La API no devuelve el correo, pero el perfil lo necesita y la sesion
        // debe sobrevivir al reinicio de la app.
        correo: response.correo ?? correo,
        token: response.token,
      };

      if (response.idUsuario) userData.idUsuario = response.idUsuario;
      if (response.idRefugio) userData.idRefugio = response.idRefugio;

      await api.setAuth(response.token, userData);
      setUser(userData);
      return response;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  }, []);

  const register = useCallback(async (role, data) => {
    setError(null);
    try {
      let response;
      if (role === 'usuario') {
        response = await api.registerUsuario(data.nombre, data.correo, data.contrasena);
      } else if (role === 'refugio') {
        response = await api.registerRefugio(
          data.nombreOrganizacion,
          data.correo,
          data.contrasena,
          data.departamento,
          data.municipio,
          data.contacto
        );
      } else {
        throw new Error('Rol no válido');
      }
      return response;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  }, []);

  const logout = useCallback(async () => {
    await api.clearAuth();
    setUser(null);
  }, []);

  const updateUser = useCallback(async (userData) => {
    try {
      let response;
      if (api.isUsuario()) {
        response = await api.updateUsuarioPerfil(userData);
      } else if (api.isRefugio()) {
        response = await api.updateRefugioPerfil(userData);
      }
      const newUser = { ...user, ...userData };
      await api.setAuth(api.token, newUser);
      setUser(newUser);
      return response;
    } catch (err) {
      setError(err.message);
      throw err;
    }
  }, []);

  const loadUser = useCallback(async () => {
    await api.init();
    // Un token sin su usuario guardado (instalacion interrumpida, datos
    // borrados a medias) produciria un user=null y dejaria la app sin sesion
    // pero sin explicar por que. Se limpia y se arranca de cero.
    if (api.token && !api.user) {
      await api.clearAuth();
    } else if (api.user) {
      setUser({
        idCuenta: api.user.idCuenta,
        rol: api.user.rol,
        nombre: api.user.nombre,
        correo: api.user.correo,
        idUsuario: api.user.idUsuario,
        idRefugio: api.user.idRefugio,
        token: api.token,
      });
    }
    setLoading(false);
  }, []);

  useEffect(() => {
    loadUser();
  }, [loadUser]);

  const value = {
    user,
    loading,
    error,
    login,
    register,
    logout,
    updateUser,
    isLoggedIn: !!user,
    isUsuario: user?.rol === 'Usuario',
    isRefugio: user?.rol === 'Refugio',
    isAdmin: user?.rol === 'Admin',
    userId: user?.idUsuario || user?.idRefugio,
    userName: user?.nombre,
    clearError: () => setError(null),
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
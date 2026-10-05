import { useEffect, useState } from "react";
import { api } from "../services/api";
import { CLAVE_SESION, leerSesion } from "../services/sesion";
import { AuthContext } from "./authContext";

export function AuthProvider({ children }) {
  const [sesion, setSesion] = useState(leerSesion);

  // El interceptor de axios avisa cuando el token venció.
  useEffect(() => {
    const alExpirar = () => setSesion(null);
    window.addEventListener("huellitas:sesion-expirada", alExpirar);
    return () => window.removeEventListener("huellitas:sesion-expirada", alExpirar);
  }, []);

  // El login es único: la API deduce el rol (usuario, refugio o admin).
  async function login(correo, contrasena) {
    const r = await api.login(correo, contrasena);
    if (!r?.token) throw new Error("La API no devolvió un token.");

    const nueva = {
      token: r.token,
      user: {
        idCuenta: r.idCuenta,
        rol: String(r.rol ?? "").toLowerCase(), // "usuario" | "refugio" | "admin"
        nombre: r.nombre,
        correo: r.correo ?? correo,
        idUsuario: r.idUsuario,
        idRefugio: r.idRefugio,
      },
    };
    localStorage.setItem(CLAVE_SESION, JSON.stringify(nueva));
    setSesion(nueva);
    return nueva.user;
  }

  function logout() {
    localStorage.removeItem(CLAVE_SESION);
    setSesion(null);
  }

  const user = sesion?.user ?? null;

  return (
    <AuthContext.Provider
      value={{
        user,
        autenticado: !!sesion?.token,
        esUsuario: user?.rol === "usuario",
        esRefugio: user?.rol === "refugio",
        esAdmin: user?.rol === "admin",
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

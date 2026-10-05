import { useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";
import { mensajeDeError } from "../services/errores";
import { ErrorMensaje } from "../components/Estado";

function destinoPorRol(rol) {
  if (rol === "admin") return "/admin";
  if (rol === "refugio") return "/refugio";
  return "/catalogo";
}

export default function Login() {
  const { login, autenticado, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [correo, setCorreo] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  if (autenticado) return <Navigate to={destinoPorRol(user.rol)} replace />;

  async function enviar(e) {
    e.preventDefault();
    setError("");
    setCargando(true);
    try {
      const u = await login(correo.trim(), contrasena);
      navigate(location.state?.desde ?? destinoPorRol(u.rol), { replace: true });
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo iniciar sesión. Revisa tu correo y contraseña."));
    } finally {
      setCargando(false);
    }
  }

  return (
    <section className="formulario">
      <h1>Iniciar sesión</h1>
      <p className="muted">
        Entra con tu cuenta de usuario, refugio o administrador.
      </p>
      {location.state?.registrado === "usuario" && (
        <p className="ok">Cuenta creada. Ya puedes iniciar sesión.</p>
      )}
      {location.state?.registrado === "refugio" && (
        <p className="ok">
          Solicitud enviada. Tu cuenta de refugio debe ser verificada antes de poder entrar
          (1 a 2 días hábiles).
        </p>
      )}
      <form onSubmit={enviar}>
        <label>
          Correo
          <input type="email" value={correo} onChange={(e) => setCorreo(e.target.value)} required autoComplete="email" />
        </label>
        <label>
          Contraseña
          <input type="password" value={contrasena} onChange={(e) => setContrasena(e.target.value)} required autoComplete="current-password" />
        </label>
        <ErrorMensaje mensaje={error} />
        <button className="btn" type="submit" disabled={cargando}>
          {cargando ? "Entrando..." : "Iniciar sesión"}
        </button>
      </form>
      <p>
        ¿No tienes cuenta? <Link to="/registro">Crear cuenta</Link>
      </p>
    </section>
  );
}

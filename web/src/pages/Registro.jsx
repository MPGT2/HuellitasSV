import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../services/api";
import { mensajeDeError } from "../services/errores";
import { DEPARTAMENTOS } from "../config/opciones";
import { ErrorMensaje } from "../components/Estado";

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const VACIO = {
  nombre: "",
  nombreOrganizacion: "",
  departamento: "",
  municipio: "",
  contacto: "",
  correo: "",
  contrasena: "",
  confirmar: "",
};

export default function Registro() {
  const navigate = useNavigate();
  const [rol, setRol] = useState("usuario");
  const [form, setForm] = useState(VACIO);
  const [documento, setDocumento] = useState(null);
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value });

  function validar() {
    if (rol === "usuario" && !form.nombre.trim()) return "Ingresa tu nombre.";
    if (rol === "refugio") {
      if (!form.nombreOrganizacion.trim()) return "Ingresa el nombre del refugio.";
      if (!form.departamento) return "Selecciona el departamento.";
      if (!form.municipio.trim()) return "Ingresa el municipio.";
      if (!form.contacto.trim()) return "Ingresa información de contacto.";
    }
    if (!EMAIL.test(form.correo.trim())) return "Ingresa un correo válido.";
    if (form.contrasena.length < 8) return "La contraseña debe tener al menos 8 caracteres.";
    if (form.contrasena !== form.confirmar) return "Las contraseñas no coinciden.";
    return "";
  }

  async function enviar(e) {
    e.preventDefault();
    const problema = validar();
    if (problema) {
      setError(problema);
      return;
    }
    setError("");
    setCargando(true);
    try {
      if (rol === "usuario") {
        await api.registrarUsuario({
          nombre: form.nombre.trim(),
          correo: form.correo.trim(),
          contrasena: form.contrasena,
        });
      } else {
        await api.registrarRefugio(
          {
            nombreOrganizacion: form.nombreOrganizacion.trim(),
            correo: form.correo.trim(),
            contrasena: form.contrasena,
            departamento: form.departamento,
            municipio: form.municipio.trim(),
            contacto: form.contacto.trim(),
          },
          documento
        );
      }
      navigate("/login", { state: { registrado: rol } });
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo crear la cuenta."));
    } finally {
      setCargando(false);
    }
  }

  return (
    <section className="formulario">
      <h1>Crear cuenta</h1>

      <div className="pestanas" role="tablist">
        <button type="button" role="tab" aria-selected={rol === "usuario"} className={rol === "usuario" ? "activa" : ""} onClick={() => setRol("usuario")}>
          Usuario
        </button>
        <button type="button" role="tab" aria-selected={rol === "refugio"} className={rol === "refugio" ? "activa" : ""} onClick={() => setRol("refugio")}>
          Refugio
        </button>
      </div>

      {rol === "refugio" && (
        <p className="info">
          Las cuentas de refugio requieren verificación de documentos. El proceso toma de 1 a 2 días hábiles.
        </p>
      )}

      <form onSubmit={enviar}>
        {rol === "usuario" ? (
          <label>
            Nombre completo
            <input value={form.nombre} onChange={cambiar("nombre")} required />
          </label>
        ) : (
          <>
            <label>
              Nombre del refugio
              <input value={form.nombreOrganizacion} onChange={cambiar("nombreOrganizacion")} required />
            </label>
            <label>
              Departamento
              <select value={form.departamento} onChange={cambiar("departamento")} required>
                <option value="">Selecciona...</option>
                {DEPARTAMENTOS.map((d) => (
                  <option key={d} value={d}>{d}</option>
                ))}
              </select>
            </label>
            <label>
              Municipio
              <input value={form.municipio} onChange={cambiar("municipio")} required />
            </label>
            <label>
              Contacto
              <input value={form.contacto} onChange={cambiar("contacto")} placeholder="2222-0000 / contacto@refugio.org" required />
            </label>
            <label>
              Acta o permiso (PDF o imagen)
              <input type="file" accept=".pdf,image/*" onChange={(e) => setDocumento(e.target.files[0] ?? null)} />
            </label>
          </>
        )}

        <label>
          Correo
          <input type="email" value={form.correo} onChange={cambiar("correo")} required autoComplete="email" />
        </label>
        <label>
          Contraseña
          <input type="password" value={form.contrasena} onChange={cambiar("contrasena")} placeholder="Mínimo 8 caracteres" required autoComplete="new-password" />
        </label>
        <label>
          Confirmar contraseña
          <input type="password" value={form.confirmar} onChange={cambiar("confirmar")} required autoComplete="new-password" />
        </label>

        <ErrorMensaje mensaje={error} />
        <button className={rol === "refugio" ? "btn btn-acento" : "btn"} type="submit" disabled={cargando}>
          {cargando ? "Enviando..." : rol === "refugio" ? "Enviar solicitud de registro" : "Crear mi cuenta"}
        </button>
      </form>
      <p>
        ¿Ya tienes cuenta? <Link to="/login">Iniciar sesión</Link>
      </p>
    </section>
  );
}

import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { mensajeDeError } from "../services/errores";
import { Cargando, ErrorMensaje } from "../components/Estado";

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const TELEFONO = /^[0-9+()\-\s]{8,20}$/;

function Formulario({ mascota, perfil }) {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    nombreContacto: perfil?.nombre ?? "",
    telefonoContacto: "",
    correoContacto: perfil?.correo ?? "",
  });
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value });

  async function enviar(e) {
    e.preventDefault();
    if (!form.nombreContacto.trim()) return setError("Ingresa tu nombre de contacto.");
    if (!TELEFONO.test(form.telefonoContacto.trim())) return setError("Ingresa un teléfono válido.");
    if (!EMAIL.test(form.correoContacto.trim())) return setError("Ingresa un correo válido.");

    setError("");
    setCargando(true);
    try {
      await api.crearSolicitud({
        idMascota: mascota.idMascota ?? mascota.id,
        nombreContacto: form.nombreContacto.trim(),
        telefonoContacto: form.telefonoContacto.trim(),
        correoContacto: form.correoContacto.trim().toLowerCase(),
      });
      navigate("/mis-solicitudes");
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo enviar la solicitud."));
    } finally {
      setCargando(false);
    }
  }

  return (
    <section className="formulario">
      <h1>Solicitar adopción</h1>
      <p className="muted">Mascota: <strong>{mascota.nombre}</strong>. El refugio usará estos datos para contactarte.</p>
      <form onSubmit={enviar}>
        <label>
          Nombre de contacto
          <input value={form.nombreContacto} onChange={cambiar("nombreContacto")} required />
        </label>
        <label>
          Teléfono
          <input value={form.telefonoContacto} onChange={cambiar("telefonoContacto")} placeholder="7777-0000" required />
        </label>
        <label>
          Correo
          <input type="email" value={form.correoContacto} onChange={cambiar("correoContacto")} required />
        </label>
        <ErrorMensaje mensaje={error} />
        <button className="btn" type="submit" disabled={cargando}>
          {cargando ? "Enviando..." : "Enviar solicitud"}
        </button>
      </form>
      <p><Link to={`/mascotas/${mascota.idMascota ?? mascota.id}`}>Cancelar</Link></p>
    </section>
  );
}

export default function SolicitarAdopcion() {
  const { id } = useParams();
  const { datos, cargando, error } = useDatos(async () => {
    const mascota = await api.mascota(id);
    // El perfil solo sirve para autocompletar; si falla, el formulario sigue.
    const perfil = await api.perfilUsuario().catch(() => null);
    return { mascota, perfil };
  }, id);

  if (cargando) return <Cargando />;
  if (error || !datos) return <ErrorMensaje mensaje={error || "No se encontró la mascota."} />;
  if (String(datos.mascota.estado).toLowerCase() !== "disponible") {
    return <p className="info">Esta mascota ya no está disponible. <Link to="/catalogo">Volver al catálogo</Link></p>;
  }
  return <Formulario mascota={datos.mascota} perfil={datos.perfil} />;
}

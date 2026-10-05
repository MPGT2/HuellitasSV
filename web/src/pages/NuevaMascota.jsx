import { useState } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import api from "../services/api";
import { ENDPOINTS } from "../services/endpoints";
import { useAuth } from "../context/useAuth";
import { mensajeDeError } from "../services/errores";

export default function NuevaMascota() {
  const { tipo, idRefugio } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    nombre: "",
    especie: "",
    tamano: "",
    edadMeses: "",
    estadoSalud: "",
  });
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  if (tipo !== "refugio") return <Navigate to="/mascotas" replace />;

  function cambiar(campo) {
    return (e) => setForm({ ...form, [campo]: e.target.value });
  }

  async function enviar(e) {
    e.preventDefault();
    setError("");
    setCargando(true);
    try {
      await api.post(ENDPOINTS.mascotas, {
        idRefugio: Number(idRefugio),
        nombre: form.nombre,
        especie: form.especie,
        tamano: form.tamano,
        edadMeses: Number(form.edadMeses),
        estadoSalud: form.estadoSalud,
      });
      navigate("/mascotas");
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo registrar la mascota."));
    } finally {
      setCargando(false);
    }
  }

  return (
    <main className="login">
      <h1>Registrar mascota</h1>
      <form onSubmit={enviar}>
        <label>
          Nombre
          <input value={form.nombre} onChange={cambiar("nombre")} required />
        </label>
        <label>
          Especie
          <input
            value={form.especie}
            onChange={cambiar("especie")}
            placeholder="perro"
            required
          />
        </label>
        <label>
          Tamaño
          <input
            value={form.tamano}
            onChange={cambiar("tamano")}
            placeholder="mediano"
            required
          />
        </label>
        <label>
          Edad (meses)
          <input
            type="number"
            min="0"
            value={form.edadMeses}
            onChange={cambiar("edadMeses")}
            required
          />
        </label>
        <label>
          Estado de salud
          <input
            value={form.estadoSalud}
            onChange={cambiar("estadoSalud")}
            placeholder="sano"
            required
          />
        </label>

        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={cargando}>
          {cargando ? "Guardando..." : "Guardar mascota"}
        </button>
      </form>
      <p>
        <Link to="/mascotas">Volver</Link>
      </p>
    </main>
  );
}

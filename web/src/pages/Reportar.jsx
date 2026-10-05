import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api } from "../services/api";
import { mensajeDeError } from "../services/errores";
import { ErrorMensaje } from "../components/Estado";

export default function Reportar() {
  const navigate = useNavigate();
  const [descripcion, setDescripcion] = useState("");
  const [latitud, setLatitud] = useState("");
  const [longitud, setLongitud] = useState("");
  const [foto, setFoto] = useState(null);
  const [error, setError] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [ubicando, setUbicando] = useState(false);
  const vistaPrevia = useMemo(() => (foto ? URL.createObjectURL(foto) : null), [foto]);

  function usarMiUbicacion() {
    if (!navigator.geolocation) return setError("Tu navegador no permite obtener la ubicación.");
    setError("");
    setUbicando(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setLatitud(pos.coords.latitude.toFixed(6));
        setLongitud(pos.coords.longitude.toFixed(6));
        setUbicando(false);
      },
      () => {
        setError("No se pudo obtener tu ubicación. Escribe las coordenadas a mano.");
        setUbicando(false);
      }
    );
  }

  async function enviar(e) {
    e.preventDefault();
    const lat = Number(latitud);
    const lng = Number(longitud);
    if (!latitud || !longitud || Number.isNaN(lat) || Number.isNaN(lng)) {
      return setError("Indica la ubicación del animal (latitud y longitud).");
    }
    if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
      return setError("Las coordenadas están fuera de rango.");
    }
    setError("");
    setEnviando(true);
    try {
      await api.crearReporte(
        { Descripcion: descripcion.trim(), Latitud: lat, Longitud: lng },
        foto
      );
      navigate("/mis-reportes");
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo enviar el reporte."));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <section className="formulario">
      <h1>Reportar un animal</h1>
      <p className="muted">¿Viste un animal callejero que necesita ayuda? Avisa a los refugios cercanos.</p>
      <form onSubmit={enviar}>
        <label>
          Descripción
          <textarea rows="4" maxLength={1000} value={descripcion} onChange={(e) => setDescripcion(e.target.value)} placeholder="Qué animal es, cómo está, referencias del lugar..." required />
        </label>
        <div className="coordenadas">
          <label>
            Latitud
            <input value={latitud} onChange={(e) => setLatitud(e.target.value)} inputMode="decimal" placeholder="13.6929" required />
          </label>
          <label>
            Longitud
            <input value={longitud} onChange={(e) => setLongitud(e.target.value)} inputMode="decimal" placeholder="-89.2182" required />
          </label>
        </div>
        <button type="button" className="btn btn-sec" onClick={usarMiUbicacion} disabled={ubicando}>
          {ubicando ? "Buscando..." : "Usar mi ubicación actual"}
        </button>
        <label>
          Foto (opcional)
          <input type="file" accept="image/*" onChange={(e) => setFoto(e.target.files[0] ?? null)} />
        </label>
        {vistaPrevia && <img className="vista-previa" src={vistaPrevia} alt="Vista previa" />}
        <ErrorMensaje mensaje={error} />
        <button className="btn" type="submit" disabled={enviando}>{enviando ? "Enviando..." : "Enviar reporte"}</button>
      </form>
      <p><Link to="/mis-reportes">Ver mis reportes</Link></p>
    </section>
  );
}

import { useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { useAuth } from "../context/useAuth";
import { mensajeDeError } from "../services/errores";
import { resolveImageUrl } from "../config/env";
import { ESPECIES, ESTADOS_MASCOTA, SALUD, TAMANOS } from "../config/opciones";
import { Cargando, ErrorMensaje } from "../components/Estado";

function Formulario({ mascota }) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const editando = Boolean(mascota);

  const [form, setForm] = useState({
    nombre: mascota?.nombre ?? "",
    especie: mascota?.especie ?? "",
    tamano: mascota?.tamano ?? "",
    edadMeses: mascota?.edadMeses ?? "",
    estadoSalud: mascota?.estadoSalud ?? "",
  });
  const [estado, setEstado] = useState(mascota?.estado ?? "disponible");
  const [justificacion, setJustificacion] = useState("");
  const [imagen, setImagen] = useState(null);
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value });
  const vistaPrevia = useMemo(() => (imagen ? URL.createObjectURL(imagen) : null), [imagen]);

  async function enviar(e) {
    e.preventDefault();
    if (!form.especie || !form.tamano || !form.estadoSalud) {
      return setError("Completa especie, tamaño y estado de salud.");
    }
    if (!editando && !user.idRefugio) {
      return setError("No se pudo identificar tu refugio. Cierra sesión y vuelve a entrar.");
    }

    setError("");
    setGuardando(true);
    const base = {
      Nombre: form.nombre.trim(),
      Especie: form.especie,
      Tamano: form.tamano,
      EdadMeses: parseInt(form.edadMeses, 10),
      EstadoSalud: form.estadoSalud,
    };

    try {
      if (editando) {
        const cuerpo = { ...base };
        if (estado !== mascota.estado) {
          cuerpo.Estado = estado;
          if (justificacion.trim()) cuerpo.JustificacionCambioEstado = justificacion.trim();
        }
        await api.actualizarMascota(mascota.idMascota, cuerpo, imagen);
      } else {
        await api.registrarMascota({ IdRefugio: user.idRefugio, ...base }, imagen);
      }
      navigate("/refugio");
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo guardar la mascota."));
    } finally {
      setGuardando(false);
    }
  }

  const opciones = (lista) =>
    lista.map((o) => <option key={o.valor} value={o.valor}>{o.etiqueta}</option>);

  return (
    <section className="formulario">
      <h1>{editando ? `Editar a ${mascota.nombre}` : "Registrar mascota"}</h1>
      <form onSubmit={enviar}>
        <label>
          Nombre
          <input value={form.nombre} onChange={cambiar("nombre")} required />
        </label>
        <label>
          Especie
          <select value={form.especie} onChange={cambiar("especie")} required>
            <option value="">Selecciona...</option>
            {opciones(ESPECIES)}
          </select>
        </label>
        <label>
          Tamaño
          <select value={form.tamano} onChange={cambiar("tamano")} required>
            <option value="">Selecciona...</option>
            {opciones(TAMANOS)}
          </select>
        </label>
        <label>
          Edad (meses)
          <input type="number" min="1" max="300" value={form.edadMeses} onChange={cambiar("edadMeses")} required />
        </label>
        <label>
          Estado de salud
          <select value={form.estadoSalud} onChange={cambiar("estadoSalud")} required>
            <option value="">Selecciona...</option>
            {opciones(SALUD)}
          </select>
        </label>

        {editando && (
          <>
            <label>
              Estado de la mascota
              <select value={estado} onChange={(e) => setEstado(e.target.value)}>
                {opciones(ESTADOS_MASCOTA)}
              </select>
            </label>
            {estado !== mascota.estado && (
              <label>
                Justificación del cambio de estado
                <textarea rows="3" value={justificacion} onChange={(e) => setJustificacion(e.target.value)} />
              </label>
            )}
          </>
        )}

        <label>
          Foto {editando && "(opcional: reemplaza la actual)"}
          <input type="file" accept="image/*" onChange={(e) => setImagen(e.target.files[0] ?? null)} />
        </label>
        {editando && mascota.imagenUrl && !imagen && (
          <img className="vista-previa" src={resolveImageUrl(mascota.imagenUrl)} alt="Foto actual" />
        )}
        {vistaPrevia && <img className="vista-previa" src={vistaPrevia} alt="Vista previa" />}

        <ErrorMensaje mensaje={error} />
        <button className="btn" type="submit" disabled={guardando}>
          {guardando ? "Guardando..." : editando ? "Guardar cambios" : "Registrar mascota"}
        </button>
      </form>
      <p><Link to="/refugio">Cancelar</Link></p>
    </section>
  );
}

export default function MascotaForm() {
  const { id } = useParams();
  const { datos, cargando, error } = useDatos(
    () => (id ? api.mascota(id) : Promise.resolve(null)),
    id ?? "nueva"
  );

  if (cargando) return <Cargando />;
  if (error) return <ErrorMensaje mensaje={error} />;
  return <Formulario mascota={datos} />;
}

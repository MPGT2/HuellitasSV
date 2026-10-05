import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { useAuth } from "../context/useAuth";
import { mensajeDeError } from "../services/errores";
import { comoLista } from "../utils/formato";
import Estrellas from "../components/Estrellas";
import TarjetaMascota from "../components/TarjetaMascota";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

function Calificar({ idRefugio, alGuardar }) {
  const { datos: mia } = useDatos(() => api.miCalificacion(idRefugio), String(idRefugio));
  const [estrellas, setEstrellas] = useState(null);
  const [comentario, setComentario] = useState(null);
  const [mensaje, setMensaje] = useState("");
  const [error, setError] = useState("");

  const valor = estrellas ?? mia?.estrellas ?? 0;
  const texto = comentario ?? mia?.comentario ?? "";

  async function enviar(e) {
    e.preventDefault();
    if (valor < 1) return setError("Elige de 1 a 5 estrellas.");
    setError("");
    setMensaje("");
    try {
      await api.calificarRefugio(idRefugio, valor, texto.trim() || undefined);
      setMensaje("¡Gracias por tu calificación!");
      alGuardar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo guardar la calificación."));
    }
  }

  return (
    <form onSubmit={enviar} className="calificar">
      <h2>{mia?.estrellas ? "Actualiza tu calificación" : "Califica a este refugio"}</h2>
      <div className="selector-estrellas" role="radiogroup" aria-label="Estrellas">
        {[1, 2, 3, 4, 5].map((n) => (
          <button
            type="button"
            key={n}
            role="radio"
            aria-checked={valor === n}
            aria-label={`${n} ${n === 1 ? "estrella" : "estrellas"}`}
            className={n <= valor ? "estrella-activa" : ""}
            onClick={() => setEstrellas(n)}
          >
            ★
          </button>
        ))}
      </div>
      <label>
        Comentario (opcional)
        <textarea rows="2" maxLength={500} value={texto} onChange={(e) => setComentario(e.target.value)} />
      </label>
      <ErrorMensaje mensaje={error} />
      {mensaje && <p className="ok">{mensaje}</p>}
      <button className="btn" type="submit">Guardar calificación</button>
    </form>
  );
}

export default function RefugioPerfil() {
  const { id } = useParams();
  const { esUsuario } = useAuth();
  const { datos: r, cargando, error, recargar } = useDatos(() => api.refugio(id), id);
  const { datos: mascotas } = useDatos(
    async () => comoLista(await api.mascotasDeRefugio(id, "disponible")),
    `m${id}`
  );
  const { datos: necesidades } = useDatos(
    async () => comoLista(await api.necesidadesDeRefugio(id, "activa")),
    `n${id}`
  );

  if (cargando) return <Cargando />;
  if (error || !r) return <ErrorMensaje mensaje={error || "No se encontró el refugio."} />;

  return (
    <>
      <p><Link to="/catalogo">← Volver al catálogo</Link></p>
      <h1>{r.nombreOrganizacion}</h1>
      <p className="muted">
        {r.municipio}, {r.departamento} · {r.contacto}
      </p>
      <p>
        {r.totalCalificaciones > 0 ? (
          <Estrellas valor={r.promedioEstrellas} total={r.totalCalificaciones} />
        ) : (
          <span className="muted">Aún sin calificaciones</span>
        )}
      </p>

      {esUsuario && <Calificar idRefugio={id} alGuardar={recargar} />}

      {necesidades && necesidades.length > 0 && (
        <>
          <h2 className="seccion">Necesidades de donación</h2>
          <div className="lista">
            {necesidades.map((n) => (
              <article className="item" key={n.idNecesidad}>
                <h2>{n.tipoInsumo}</h2>
                {n.descripcion && <p>{n.descripcion}</p>}
                <p className="muted">
                  Cubierto {n.cantidadCubierta} de {n.cantidadRequerida}
                </p>
              </article>
            ))}
          </div>
          <p><Link to="/necesidades">Ver todas las necesidades</Link></p>
        </>
      )}

      <h2 className="seccion">Mascotas disponibles</h2>
      {mascotas && mascotas.length === 0 && <Vacio>Este refugio no tiene mascotas disponibles ahora.</Vacio>}
      <div className="rejilla">
        {mascotas?.map((m) => (
          <TarjetaMascota key={m.idMascota} mascota={{ ...m, refugio: m.refugio ?? r }} />
        ))}
      </div>
    </>
  );
}

import { useState } from "react";
import { api } from "../services/api";
import { useDatos } from "../hooks/useDatos";
import { mensajeDeError } from "../services/errores";
import { comoLista, fecha } from "../utils/formato";
import { Cargando, ErrorMensaje, Vacio } from "../components/Estado";

export default function Notificaciones() {
  const [errorAccion, setErrorAccion] = useState("");
  const { datos, cargando, error, recargar } = useDatos(
    async () => comoLista(await api.notificaciones()),
    "todas"
  );

  async function marcar(id) {
    setErrorAccion("");
    try {
      await api.marcarLeida(id);
      recargar();
    } catch (err) {
      setErrorAccion(mensajeDeError(err, "No se pudo marcar como leída."));
    }
  }

  return (
    <>
      <h1>Notificaciones</h1>
      {cargando && <Cargando />}
      <ErrorMensaje mensaje={error || errorAccion} />
      {datos && datos.length === 0 && <Vacio>No tienes notificaciones.</Vacio>}
      <div className="lista">
        {datos?.map((n) => (
          <article className={n.leida ? "item" : "item no-leida"} key={n.idNotificacion}>
            <p>{n.mensaje}</p>
            <div className="item-cabecera">
              <span className="muted">{fecha(n.fechaCreacion)}</span>
              {!n.leida && (
                <button className="btn btn-sec" onClick={() => marcar(n.idNotificacion)}>Marcar como leída</button>
              )}
            </div>
          </article>
        ))}
      </div>
    </>
  );
}

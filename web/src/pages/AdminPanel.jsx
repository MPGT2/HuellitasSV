import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../services/api";
import { ENDPOINTS } from "../services/endpoints";
import { mensajeDeError } from "../services/errores";
import Encabezado from "../components/Encabezado";

export default function AdminPanel() {
  const [refugios, setRefugios] = useState([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .get(ENDPOINTS.refugios)
      .then(({ data }) => setRefugios(Array.isArray(data) ? data : []))
      .catch(() => setError("No se pudieron cargar los refugios."))
      .finally(() => setCargando(false));
  }, []);

  async function cambiarEstado(refugio, nuevoEstado) {
    const id = refugio.id ?? refugio.idRefugio;
    setError("");
    try {
      await api.request({
        method: ENDPOINTS.refugioEstado.metodo,
        url: ENDPOINTS.refugioEstado.ruta(id),
        data: { estado: nuevoEstado },
      });
      setRefugios((lista) =>
        lista.map((r) =>
          (r.id ?? r.idRefugio) === id ? { ...r, estado: nuevoEstado } : r
        )
      );
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo cambiar el estado."));
    }
  }

  return (
    <main className="mascotas">
      <Encabezado titulo="Panel de administración" />

      <section>
        <h2>Refugios</h2>
        {cargando && <p>Cargando...</p>}
        {error && <p className="error">{error}</p>}
        {!cargando && refugios.length === 0 && !error && (
          <p>No hay refugios registrados.</p>
        )}

        {refugios.length > 0 && (
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th>Organización</th>
                  <th>Correo</th>
                  <th>Ubicación</th>
                  <th>Contacto</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {refugios.map((r) => (
                  <tr key={r.id ?? r.idRefugio}>
                    <td>{r.nombreOrganizacion}</td>
                    <td>{r.correo}</td>
                    <td>
                      {r.municipio}, {r.departamento}
                    </td>
                    <td>{r.contacto}</td>
                    <td>{r.estado}</td>
                    <td className="acciones">
                      <button onClick={() => cambiarEstado(r, "aprobado")}>
                        Aprobar
                      </button>
                      <button
                        className="secundario"
                        onClick={() => cambiarEstado(r, "rechazado")}
                      >
                        Rechazar
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <p>
        Para revisar lo que ven los usuarios, entra a{" "}
        <Link to="/mascotas">Mascotas</Link>.
      </p>
    </main>
  );
}

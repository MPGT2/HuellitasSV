import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../services/api";
import { ENDPOINTS } from "../services/endpoints";
import Encabezado from "../components/Encabezado";

export default function Mascotas() {
  const [mascotas, setMascotas] = useState([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .get(ENDPOINTS.mascotas)
      .then(({ data }) => setMascotas(Array.isArray(data) ? data : []))
      .catch(() => setError("No se pudieron cargar las mascotas."))
      .finally(() => setCargando(false));
  }, []);

  return (
    <main className="mascotas">
      <Encabezado titulo="Mascotas" />

      {cargando && <p>Cargando...</p>}
      {error && <p className="error">{error}</p>}
      {!cargando && !error && mascotas.length === 0 && (
        <p>Todavía no hay mascotas registradas.</p>
      )}

      <ul className="lista">
        {mascotas.map((m) => {
          const id = m.id ?? m.idMascota;
          return (
            <li key={id}>
              <Link to={`/mascotas/${id}`} state={{ mascota: m }}>
                <h2>{m.nombre}</h2>
                <p>
                  {m.especie} · {m.tamano}
                </p>
                <p>{m.edadMeses} meses</p>
                {m.estado && <p>Estado: {m.estado}</p>}
                <span className="ver">Ver detalle</span>
              </Link>
            </li>
          );
        })}
      </ul>
    </main>
  );
}

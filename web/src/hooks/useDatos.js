import { useEffect, useRef, useState } from "react";
import { mensajeDeError } from "../services/errores";

// Carga datos de la API.
// - fetcher: función que devuelve una promesa.
// - clave: texto que cambia cuando hay que volver a consultar (filtros, ids...).
// Devuelve { datos, cargando, error, recargar }.
export function useDatos(fetcher, clave = "") {
  const [version, setVersion] = useState(0);
  const [resultado, setResultado] = useState({ clave: null, datos: null, error: "" });
  const fetcherRef = useRef(fetcher);
  const claveCompleta = `${clave}|${version}`;

  useEffect(() => {
    fetcherRef.current = fetcher;
  });

  useEffect(() => {
    let activo = true;
    fetcherRef
      .current()
      .then((datos) => {
        if (activo) setResultado({ clave: claveCompleta, datos, error: "" });
      })
      .catch((err) => {
        if (activo) {
          setResultado({
            clave: claveCompleta,
            datos: null,
            error: mensajeDeError(err, "No se pudo cargar la información."),
          });
        }
      });
    return () => {
      activo = false;
    };
  }, [claveCompleta]);

  const vigente = resultado.clave === claveCompleta;
  return {
    datos: vigente ? resultado.datos : null,
    error: vigente ? resultado.error : "",
    cargando: !vigente,
    recargar: () => setVersion((v) => v + 1),
  };
}

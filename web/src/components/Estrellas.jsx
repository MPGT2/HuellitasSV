// Muestra una calificación de 0 a 5 estrellas (solo lectura).
export default function Estrellas({ valor = 0, total }) {
  const redondo = Math.round(Number(valor) || 0);
  return (
    <span className="estrellas" aria-label={`${valor} de 5 estrellas`}>
      {"★".repeat(redondo)}
      <span className="estrellas-vacias">{"★".repeat(5 - redondo)}</span>
      {total != null && <span className="muted"> ({total})</span>}
    </span>
  );
}

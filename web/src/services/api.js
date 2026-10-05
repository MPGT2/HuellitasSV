import axios from "axios";
import { API_URL } from "../config/env";
import { CLAVE_SESION, leerSesion } from "./sesion";

const http = axios.create({ baseURL: API_URL });

// Envía el token JWT en cada petición.
http.interceptors.request.use((config) => {
  const token = leerSesion()?.token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Si el token venció, se limpia la sesión y se avisa a la app.
http.interceptors.response.use(
  (r) => r,
  (error) => {
    if (error.response?.status === 401 && leerSesion()?.token) {
      localStorage.removeItem(CLAVE_SESION);
      window.dispatchEvent(new Event("huellitas:sesion-expirada"));
    }
    return Promise.reject(error);
  }
);

const datos = (promesa) => promesa.then((r) => r.data);

// Arma un FormData con valores simples y un archivo opcional.
function formData(campos, nombreArchivo, archivo) {
  const fd = new FormData();
  Object.entries(campos).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== "") fd.append(k, String(v));
  });
  if (archivo) fd.append(nombreArchivo, archivo);
  return fd;
}

const conEstado = (estado) => (estado ? { params: { estado } } : undefined);

export const api = {
  // --- Autenticación (login único: la API devuelve el rol) ---
  login: (correo, contrasena) =>
    datos(http.post("/Auth/login", { correo, contrasena })),
  registrarUsuario: (cuerpo) => datos(http.post("/Usuarios/registro", cuerpo)),
  registrarRefugio: (c, documento) =>
    datos(
      http.post(
        "/Refugios/registro",
        formData(
          {
            NombreOrganizacion: c.nombreOrganizacion,
            Correo: c.correo,
            Contrasena: c.contrasena,
            Departamento: c.departamento,
            Municipio: c.municipio,
            Contacto: c.contacto,
          },
          "Documentacion",
          documento
        )
      )
    ),
  perfilUsuario: () => datos(http.get("/Usuarios/perfil")),

  // --- Refugios (público) y calificación ---
  refugio: (id) => datos(http.get(`/Refugios/${id}`)),
  miCalificacion: (id) => datos(http.get(`/Refugios/${id}/mi-calificacion`)),
  calificarRefugio: (id, estrellas, comentario) =>
    datos(http.post(`/Refugios/${id}/calificar`, { estrellas, comentario })),

  // --- Mascotas ---
  catalogo: () => datos(http.get("/Mascotas")),
  mascota: (id) => datos(http.get(`/Mascotas/${id}`)),
  mascotasPorEspecie: (especie) =>
    datos(http.get(`/Mascotas/especie/${encodeURIComponent(especie)}`)),
  mascotasPorAtributos: (params) =>
    datos(http.get("/Mascotas/atributos", { params })),
  mascotasPorUbicacion: (params) =>
    datos(http.get("/Mascotas/ubicacion", { params })),
  mascotasDeRefugio: (idRefugio, estado) =>
    datos(http.get(`/Mascotas/refugio/${idRefugio}`, conEstado(estado))),
  registrarMascota: (campos, imagen) =>
    datos(http.post("/Mascotas", formData(campos, "imagen", imagen))),
  actualizarMascota: (id, campos, imagen) =>
    datos(http.put(`/Mascotas/${id}`, formData(campos, "imagen", imagen))),
  eliminarMascota: (id) => datos(http.delete(`/Mascotas/${id}`)),

  // --- Solicitudes de adopción ---
  misSolicitudes: (estado) =>
    datos(http.get("/SolicitudesAdopcion", conEstado(estado))),
  crearSolicitud: (cuerpo) => datos(http.post("/SolicitudesAdopcion", cuerpo)),
  solicitudesRefugio: (estado) =>
    datos(http.get("/GestionSolicitudes", conEstado(estado))),
  decidirSolicitud: (id, decision, comentarioDecision) =>
    datos(
      http.put(`/GestionSolicitudes/${id}/${decision}`, { comentarioDecision })
    ),

  // --- Rescates (refugio) ---
  reportesRescate: (estado) =>
    datos(http.get("/ReportesRescate", conEstado(estado))),
  marcarAtendido: (id) => datos(http.put(`/ReportesRescate/${id}/atendido`)),

  // --- Necesidades de donación ---
  necesidades: (estado = "activa") =>
    datos(http.get("/NecesidadesDonacion", { params: { estado } })),
  necesidadesDeRefugio: (idRefugio, estado = "todas") =>
    datos(http.get(`/NecesidadesDonacion/refugio/${idRefugio}`, { params: { estado } })),
  publicarNecesidad: (cuerpo) => datos(http.post("/NecesidadesDonacion", cuerpo)),
  aportarNecesidad: (id, cantidad) =>
    datos(http.post(`/NecesidadesDonacion/${id}/aportar`, { cantidad })),
  eliminarNecesidad: (id) => datos(http.delete(`/NecesidadesDonacion/${id}`)),

  // --- Reportes de animales (usuario) ---
  misReportes: (estado) => datos(http.get("/ReportesAnimales", conEstado(estado))),
  crearReporte: (campos, foto) =>
    datos(http.post("/ReportesAnimales", formData(campos, "Foto", foto))),
  eliminarReporte: (id) => datos(http.delete(`/ReportesAnimales/${id}`)),

  // --- Notificaciones (usuario o refugio) ---
  notificaciones: (leidas) =>
    datos(http.get("/Notificaciones", leidas === undefined ? undefined : { params: { leidas } })),
  marcarLeida: (id) => datos(http.put(`/Notificaciones/${id}/leida`)),

  // --- Administración ---
  refugiosPendientes: () => datos(http.get("/Refugios/pendientes")),
  todosLosRefugios: () => datos(http.get("/Refugios")),
  cambiarEstadoRefugio: (id, estado) =>
    datos(http.put(`/Refugios/${id}/estado`, { estado })),
  anuncios: (estado) => datos(http.get("/Anuncios", conEstado(estado))),
  aprobarAnuncio: (id) => datos(http.post(`/Anuncios/${id}/aprobar`)),
  confirmarPagoAnuncio: (id) =>
    datos(http.post(`/Anuncios/${id}/confirmar-pago`)),
  rechazarAnuncio: (id) => datos(http.post(`/Anuncios/${id}/rechazar`)),
  eliminarAnuncio: (id) => datos(http.delete(`/Anuncios/${id}`)),
  crearAnuncio: (cuerpo) => datos(http.post("/Anuncios", cuerpo)),
  monetizacion: () => datos(http.get("/Anuncios/panel/monetizacion")),
};

export default http;

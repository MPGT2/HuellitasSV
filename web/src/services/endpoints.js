// Todas las rutas de la API en un solo lugar.
// TODO: confirma cada ruta (y el método: put / patch) en
// https://huellitas-api.runasp.net/swagger y corrígelas aquí.
// No hace falta tocar nada más.
export const ENDPOINTS = {
  login: {
    refugio: "/refugios/login",
    usuario: "/usuarios/login",
    administrador: "/admin/login",
  },
  registro: {
    refugio: "/refugios/registro",
    usuario: "/usuarios/registro",
  },
  mascotas: "/mascotas",
  mascota: (id) => `/mascotas/${id}`,
  mascotaActualizar: {
    metodo: "put",
    ruta: (id) => `/mascotas/${id}`,
  },
  refugios: "/refugios",
  refugioEstado: {
    metodo: "put",
    ruta: (id) => `/refugios/${id}/estado`,
  },
};

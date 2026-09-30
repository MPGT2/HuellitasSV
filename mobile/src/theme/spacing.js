// Escala de medidas extraida de RegisterScreen, que es la pantalla de
// referencia. Los valores coinciden con los que ya usaba a mano, asi que
// aplicar la escala no cambia ni un pixel: solo deja de estar repetido.
import { colors } from './colors';

export const spacing = {
  xs: 6,
  sm: 8,
  md: 12,
  lg: 14,
  xl: 16,
  xxl: 20,
  xxxl: 24,

  // Margenes de la pagina (RegisterScreen.scroll)
  page: 24,
  pageTop: 40,
  pageBottom: 64,
};

// Radios. Antes convivian 8, 10, 12, 16, 20, 22, 24 y 50 en distintas pantallas.
export const radius = {
  sm: 8,
  md: 12,
  lg: 16,
  circle: 20,
  pill: 999,
};

// Tamanos de texto, con el peso y el lineHeight que ya usaba cada pantalla.
export const typography = {
  title: { fontSize: 20, fontWeight: '800', color: colors.text },
  subtitle: { fontSize: 13, color: colors.muted, marginTop: 2 },
  label: { fontSize: 12, fontWeight: '600', letterSpacing: 1 },
  body: { fontSize: 13, lineHeight: 20 },
  sectionTitle: { fontSize: 16, fontWeight: '700' },
};

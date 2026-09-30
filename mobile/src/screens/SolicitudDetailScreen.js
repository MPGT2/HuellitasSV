import { useLayoutEffect } from 'react';
import {
  Image,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { resolveImageUrl } from '../config/env';
import { useNavigation, useRoute } from '@react-navigation/native';

const estadoColor = (estado) => {
  switch (estado) {
    case 'Pendiente':
      return colors.warning;
    case 'Aprobada':
      return colors.success;
    case 'Rechazada':
      return colors.danger;
    default:
      return colors.muted;
  }
};

const estadoBg = (estado) => {
  switch (estado) {
    case 'Pendiente':
      return colors.warningBg;
    case 'Aprobada':
      return colors.successBg;
    case 'Rechazada':
      return colors.dangerBg;
    default:
      return colors.surfaceMuted;
  }
};

const formatearFecha = (fecha) => {
  if (!fecha) return '—';
  const d = new Date(fecha);
  return Number.isNaN(d.getTime())
    ? '—'
    : d.toLocaleDateString('es-ES', { day: '2-digit', month: 'long', year: 'numeric' });
};

export default function SolicitudDetailScreen() {
  const nav = useNavigation();
  const route = useRoute();
  const solicitud = route.params?.solicitud;

  useLayoutEffect(() => {
    nav.setOptions({ title: 'Detalle de la solicitud' });
  }, [nav]);

  if (!solicitud) {
    return (
      <SafeAreaView style={styles.container} edges={['bottom']}>
        <View style={styles.emptyContainer}>
          <Ionicons name="alert-circle" size={64} color={colors.muted} />
          <Text style={styles.emptyText}>Solicitud no encontrada</Text>
        </View>
      </SafeAreaView>
    );
  }

  const color = estadoColor(solicitud.estado);
  const bg = estadoBg(solicitud.estado);
  const mascota = solicitud.mascota;

  // Linea de tiempo del proceso de adopcion.
  const pasos = [
    { titulo: 'Solicitud enviada', hecho: true, detalle: formatearFecha(solicitud.fechaSolicitud) },
    {
      titulo: 'Revisión del refugio',
      hecho: solicitud.estado !== 'Pendiente',
      detalle: solicitud.estado === 'Pendiente' ? 'En espera' : 'Revisada',
    },
    {
      titulo:
        solicitud.estado === 'Aprobada'
          ? 'Adopción aprobada'
          : solicitud.estado === 'Rechazada'
            ? 'Solicitud rechazada'
            : 'Resultado',
      hecho: solicitud.estado !== 'Pendiente',
      detalle:
        solicitud.estado === 'Pendiente' ? 'Pendiente de decisión' : solicitud.estado,
    },
  ];

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <ScrollView contentContainerStyle={styles.scroll}>
        <TouchableOpacity
          style={styles.petCard}
          activeOpacity={0.8}
          onPress={() => (mascota ? nav.navigate('PetDetail', { pet: mascota }) : null)}
        >
          {resolveImageUrl(mascota?.imagenUrl) ? (
            <Image
              source={{ uri: resolveImageUrl(mascota?.imagenUrl) }}
              style={styles.petThumb}
            />
          ) : (
            <View style={[styles.petThumb, styles.petThumbEmpty]}>
              <Ionicons name="paw" size={22} color={colors.muted} />
            </View>
          )}
          <View style={styles.petInfo}>
            <Text style={styles.petName}>{mascota?.nombre || 'Mascota'}</Text>
            <Text style={styles.petMeta}>
              {mascota?.especie} · {mascota?.tamano}
              {mascota ? ` · ${mascota.edadMeses} meses` : ''}
            </Text>
          </View>
          <Ionicons name="chevron-forward" size={20} color={colors.muted} />
        </TouchableOpacity>

        <View style={[styles.statusBox, { backgroundColor: bg }]}>
          <View style={[styles.statusDot, { backgroundColor: color }]} />
          <Text style={[styles.statusText, { color }]}>
            {solicitud.estado}
          </Text>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Proceso de adopción</Text>
          <View style={styles.timeline}>
            {pasos.map((paso, i) => (
              <View key={paso.titulo} style={styles.paso}>
                <View style={styles.pasoLeft}>
                  <View
                    style={[
                      styles.pasoDot,
                      paso.hecho ? { backgroundColor: colors.primary } : styles.pasoDotPending,
                    ]}
                  >
                    {paso.hecho ? (
                      <Ionicons name="checkmark" size={14} color={colors.onPrimary} />
                    ) : null}
                  </View>
                  {i < pasos.length - 1 ? <View style={styles.pasoLine} /> : null}
                </View>
                <View style={styles.pasoBody}>
                  <Text style={styles.pasoTitulo}>{paso.titulo}</Text>
                  <Text style={styles.pasoDetalle}>{paso.detalle}</Text>
                </View>
              </View>
            ))}
          </View>
        </View>

        {solicitud.comentarioDecision ? (
          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Respuesta del refugio</Text>
            <View style={styles.comment}>
              <Ionicons name="chatbox-ellipses-outline" size={16} color={colors.muted} />
              <Text style={styles.commentText}>{solicitud.comentarioDecision}</Text>
            </View>
          </View>
        ) : null}

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Tus datos de contacto</Text>
          <View style={styles.infoCard}>
            <InfoRow icon="person-outline" label="Nombre" value={solicitud.nombreContacto} />
            <InfoRow icon="call-outline" label="Teléfono" value={solicitud.telefonoContacto} />
            <InfoRow icon="mail-outline" label="Correo" value={solicitud.correoContacto} />
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function InfoRow({ icon, label, value }) {
  return (
    <View style={styles.infoRow}>
      <Ionicons name={icon} size={18} color={colors.muted} />
      <View style={styles.infoRowText}>
        <Text style={styles.infoRowLabel}>{label}</Text>
        <Text style={styles.infoRowValue}>{value || '—'}</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    padding: spacing.page,
    gap: spacing.xl,
  },
  emptyContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.md,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.text,
  },
  petCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    padding: spacing.md,
    borderRadius: radius.md,
    backgroundColor: colors.surfaceAlt,
    borderWidth: 1,
    borderColor: colors.border,
  },
  petThumb: {
    width: 56,
    height: 56,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceMuted,
  },
  petThumbEmpty: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  petInfo: {
    flex: 1,
  },
  petName: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  petMeta: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
    textTransform: 'capitalize',
  },
  statusBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    alignSelf: 'flex-start',
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.sm,
    borderRadius: radius.pill,
  },
  statusDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
  },
  statusText: {
    fontSize: 14,
    fontWeight: '700',
  },
  section: {
    gap: spacing.md,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  timeline: {
    gap: 0,
  },
  paso: {
    flexDirection: 'row',
    gap: spacing.md,
  },
  pasoLeft: {
    alignItems: 'center',
    width: 24,
  },
  pasoDot: {
    width: 24,
    height: 24,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  pasoDotPending: {
    backgroundColor: colors.surfaceMuted,
    borderWidth: 1,
    borderColor: colors.border,
  },
  pasoLine: {
    flex: 1,
    width: 2,
    backgroundColor: colors.border,
    marginVertical: 2,
  },
  pasoBody: {
    flex: 1,
    paddingBottom: spacing.lg,
  },
  pasoTitulo: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
  },
  pasoDetalle: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
    textTransform: 'capitalize',
  },
  comment: {
    flexDirection: 'row',
    gap: spacing.sm,
    backgroundColor: colors.surfaceAlt,
    borderRadius: radius.md,
    padding: spacing.md,
  },
  commentText: {
    flex: 1,
    fontSize: 14,
    color: colors.text,
    lineHeight: 20,
  },
  infoCard: {
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.card,
    padding: spacing.md,
    gap: spacing.md,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  infoRowText: {
    flex: 1,
  },
  infoRowLabel: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.muted,
    letterSpacing: 0.5,
  },
  infoRowValue: {
    fontSize: 14,
    color: colors.text,
    marginTop: 1,
  },
});

import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Image,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';

const estadoColor = (estado) =>
  estado === 'Atendido' ? colors.success : colors.warning;
const estadoBg = (estado) =>
  estado === 'Atendido' ? colors.successBg : colors.warningBg;

const formatearFecha = (fecha) => {
  if (!fecha) return '—';
  const d = new Date(fecha);
  return Number.isNaN(d.getTime())
    ? '—'
    : d.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

export default function MisReportesScreen() {
  const nav = useNavigation();
  const [reportes, setReportes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);

  const cargar = useCallback(async () => {
    try {
      setError(null);
      const data = await api.getMisReportes();
      setReportes(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || 'No se pudieron cargar tus reportes.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      cargar();
    }, [cargar]),
  );

  const renderReporte = ({ item }) => (
    <View style={styles.card}>
      <View style={styles.row}>
        {resolveImageUrl(item.fotoUrl) ? (
          <Image source={{ uri: resolveImageUrl(item.fotoUrl) }} style={styles.thumb} />
        ) : (
          <View style={[styles.thumb, styles.thumbEmpty]}>
            <Ionicons name="image-outline" size={22} color={colors.muted} />
          </View>
        )}
        <View style={styles.info}>
          <Text style={styles.descripcion} numberOfLines={2}>
            {item.descripcion}
          </Text>
          <Text style={styles.fecha}>{formatearFecha(item.fechaRegistro)}</Text>
        </View>
        <View style={[styles.badge, { backgroundColor: estadoBg(item.estado) }]}>
          <Text style={[styles.badgeText, { color: estadoColor(item.estado) }]}>
            {item.estado}
          </Text>
        </View>
      </View>
    </View>
  );

  if (loading && reportes.length === 0) {
    return (
      <SafeAreaView style={styles.container} edges={['top']}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando reportes...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <View style={styles.topBar}>
        <View style={styles.topBarText}>
          <Text style={styles.topTitle}>Mis reportes</Text>
          <Text style={styles.topSubtitle}>
            {reportes.length} {reportes.length === 1 ? 'reporte' : 'reportes'}
          </Text>
        </View>
        <TouchableOpacity
          style={styles.addButton}
          onPress={() => nav.navigate('ReporteForm')}
        >
          <Ionicons name="add" size={18} color={colors.onPrimary} />
          <Text style={styles.addButtonText}>Reportar</Text>
        </TouchableOpacity>
      </View>

      {error ? (
        <View style={styles.errorBanner}>
          <Ionicons name="alert-circle" size={18} color={colors.danger} />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity onPress={cargar}>
            <Text style={styles.errorRetry}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      ) : null}

      <FlatList
        data={reportes}
        keyExtractor={(item) => item.idReporte.toString()}
        renderItem={renderReporte}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => {
              setRefreshing(true);
              cargar();
            }}
          />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Ionicons name="megaphone-outline" size={56} color={colors.muted} />
            <Text style={styles.emptyText}>No has enviado reportes</Text>
            <Text style={styles.emptySubtext}>
              Si ves un animal en situación de calle, repórtalo.
            </Text>
          </View>
        }
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: spacing.md,
  },
  loadingText: {
    fontSize: 14,
    color: colors.muted,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: spacing.page,
    paddingVertical: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  topBarText: {
    flex: 1,
  },
  topTitle: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
  },
  topSubtitle: {
    fontSize: 12,
    color: colors.muted,
    marginTop: 2,
  },
  addButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: colors.primary,
    borderRadius: radius.pill,
    paddingVertical: 8,
    paddingHorizontal: spacing.lg,
  },
  addButtonText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  errorBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    backgroundColor: colors.dangerBg,
    marginHorizontal: spacing.page,
    marginTop: spacing.md,
    padding: spacing.md,
    borderRadius: radius.md,
  },
  errorText: {
    flex: 1,
    fontSize: 13,
    color: colors.danger,
  },
  errorRetry: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.danger,
  },
  list: {
    padding: spacing.page,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  thumb: {
    width: 56,
    height: 56,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceMuted,
  },
  thumbEmpty: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  info: {
    flex: 1,
  },
  descripcion: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
  },
  fecha: {
    fontSize: 12,
    color: colors.muted,
    marginTop: 4,
  },
  badge: {
    paddingHorizontal: spacing.md,
    paddingVertical: 5,
    borderRadius: radius.pill,
  },
  badgeText: {
    fontSize: 12,
    fontWeight: '700',
  },
  emptyContainer: {
    alignItems: 'center',
    gap: spacing.sm,
    padding: 40,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.text,
  },
  emptySubtext: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
  },
});

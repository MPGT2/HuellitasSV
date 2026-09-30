import { useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Image,
  Modal,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';

const FILTROS_VACIOS = {
  especie: '',
  tamano: '',
  edadMaxMeses: '',
  estadoSalud: '',
  departamento: '',
  municipio: '',
};

const OPCIONES_ESPECIE = [
  { valor: 'perro', label: 'Perro' },
  { valor: 'gato', label: 'Gato' },
  { valor: 'otro', label: 'Otro' },
];

const OPCIONES_TAMANO = [
  { valor: 'pequeño', label: 'Pequeño' },
  { valor: 'mediano', label: 'Mediano' },
  { valor: 'grande', label: 'Grande' },
];

const OPCIONES_SALUD = [
  { valor: 'sano', label: 'Sano' },
  { valor: 'en_tratamiento', label: 'En tratamiento' },
];

// Los 14 departamentos de El Salvador, para el filtro por ubicacion (HU-06).
const DEPARTAMENTOS = [
  'Ahuachapán', 'Cabañas', 'Chalatenango', 'Cuscatlán', 'La Libertad',
  'La Paz', 'La Unión', 'Morazán', 'San Miguel', 'San Salvador',
  'San Vicente', 'Santa Ana', 'Sonsonate', 'Usulután',
];

const contarActivos = (filtros) =>
  Object.values(filtros).filter((v) => v !== '').length;

export default function CatalogScreen() {
  // Lo que se elige en el panel y lo que realmente se consulta van separados a
  // proposito. Antes no habia boton de aplicar: los chips solo cambiaban el
  // estado local y la lista no se actualizaba hasta hacer pull-to-refresh.
  const [draft, setDraft] = useState(FILTROS_VACIOS);
  const [aplicados, setAplicados] = useState(FILTROS_VACIOS);
  const [pets, setPets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const [showFilters, setShowFilters] = useState(false);
  const nav = useNavigation();

  const consultar = useCallback(async (filtros) => {
    const { especie, tamano, edadMaxMeses, estadoSalud, departamento, municipio } = filtros;
    const hayUbicacion = Boolean(departamento || municipio);

    // HU-06: el endpoint de ubicacion no combina con atributos/especie, asi que
    // si hay ubicacion se consulta por ella y el resto se acota en el cliente.
    if (hayUbicacion) {
      const data = await api.getMascotasByUbicacion({
        departamento: departamento || undefined,
        municipio: municipio || undefined,
      });
      const edad = edadMaxMeses ? parseInt(edadMaxMeses, 10) : null;
      return data.filter((m) => {
        if (especie && m.especie?.toLowerCase() !== especie.toLowerCase()) return false;
        if (tamano && m.tamano?.toLowerCase() !== tamano.toLowerCase()) return false;
        if (edad != null && !Number.isNaN(edad) && (m.edadMeses ?? 0) > edad) return false;
        if (estadoSalud && m.estadoSalud?.toLowerCase() !== estadoSalud.toLowerCase()) return false;
        return true;
      });
    }

    const hayAtributos = Boolean(tamano || edadMaxMeses || estadoSalud);

    if (!especie && !hayAtributos) {
      const data = await api.getCatalogo();
      return data.mascotas || [];
    }

    if (especie && !hayAtributos) {
      return await api.getMascotasByEspecie(especie);
    }

    const data = await api.getMascotasByAtributos({
      tamano: tamano || undefined,
      edadMaxMeses: edadMaxMeses ? parseInt(edadMaxMeses, 10) : undefined,
      estadoSalud: estadoSalud || undefined,
    });

    // El endpoint de atributos no recibe especie, asi que se acota en el
    // cliente. Se filtra sobre la respuesta, sin mutar lo que devuelve la api.
    if (especie) {
      return data.filter(
        (m) => m.especie?.toLowerCase() === especie.toLowerCase(),
      );
    }
    return data;
  }, []);

  const cargar = useCallback(
    async (filtros) => {
      setError(null);
      try {
        setPets(await consultar(filtros));
      } catch (err) {
        setError(err.message || 'No se pudo cargar el catalogo.');
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [consultar],
  );

  useEffect(() => {
    cargar(FILTROS_VACIOS);
  }, [cargar]);

  const onRefresh = () => {
    setRefreshing(true);
    cargar(aplicados);
  };

  const onAplicar = () => {
    setAplicados(draft);
    setLoading(true);
    cargar(draft);
  };

  const onLimpiar = () => {
    setDraft(FILTROS_VACIOS);
    setAplicados(FILTROS_VACIOS);
    setLoading(true);
    cargar(FILTROS_VACIOS);
  };

  const alternar = (campo, valor) => {
    setDraft((prev) => ({
      ...prev,
      // Antes comparaba con truthiness: con "gato" puesto, tocar "perro"
      // borraba el filtro en vez de cambiarlo.
      [campo]: prev[campo] === valor ? '' : valor,
      // El municipio solo tiene sentido dentro del departamento elegido.
      ...(campo === 'departamento' ? { municipio: '' } : {}),
    }));
  };

  const hayCambiosPendientes =
    contarActivos(draft) !== contarActivos(aplicados) ||
    Object.keys(FILTROS_VACIOS).some((k) => draft[k] !== aplicados[k]);

  const renderPet = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      onPress={() => nav.navigate('PetDetail', { pet: item })}
      activeOpacity={0.8}
    >
      <View style={styles.imageContainer}>
        <Image
          source={{ uri: resolveImageUrl(item.imagenUrl) || 'https://via.placeholder.com/300' }}
          style={styles.image}
        />
        <View style={styles.statusBadge}>
          <Text style={styles.statusText}>{item.estado}</Text>
        </View>
      </View>
      <View style={styles.info}>
        <View style={styles.header}>
          <Text style={styles.name}>{item.nombre}</Text>
          <View style={styles.speciesBadge}>
            <Ionicons
              name={
                item.especie === 'perro'
                  ? 'paw'
                  : item.especie === 'gato'
                    ? 'cat'
                    : 'ellipse-outline'
              }
              size={14}
              color={colors.onPrimary}
            />
            <Text style={styles.speciesText}>{item.especie}</Text>
          </View>
        </View>
        <View style={styles.details}>
          <Text style={styles.detail}>
            <Ionicons name="resize" size={14} color={colors.muted} />{' '}
            {item.tamano}
          </Text>
          <Text style={styles.detail}>
            <Ionicons name="time" size={14} color={colors.muted} /> {item.edadMeses}{' '}
            meses
          </Text>
          <Text style={styles.detail}>
            <Ionicons name="medical" size={14} color={colors.muted} />{' '}
            {item.estadoSalud}
          </Text>
        </View>
        <View style={styles.refugio}>
          <Ionicons name="home" size={14} color={colors.muted} />
          <Text style={styles.refugioText} numberOfLines={1}>
            {item.refugio?.nombreOrganizacion || 'Refugio'}
          </Text>
          {item.refugio?.calificacionTexto ? (
            <Text style={styles.rating}>{item.refugio.calificacionTexto}</Text>
          ) : null}
        </View>
      </View>
    </TouchableOpacity>
  );

  const activos = contarActivos(aplicados);

  const renderEmpty = () => (
    <View style={styles.emptyContainer}>
      <Ionicons name="paw" size={64} color={colors.muted} />
      <Text style={styles.emptyText}>
        {activos ? 'Sin resultados' : 'No hay mascotas disponibles'}
      </Text>
      <Text style={styles.emptySubtext}>
        {activos
          ? 'Proba cambiando o quitando algun filtro'
          : 'Volve mas tarde, se publica a diario'}
      </Text>
      {activos ? (
        <TouchableOpacity style={styles.emptyButton} onPress={onLimpiar}>
          <Text style={styles.emptyButtonText}>Limpiar filtros</Text>
        </TouchableOpacity>
      ) : null}
    </View>
  );

  if (loading && pets.length === 0) {
    return (
      <SafeAreaView style={styles.container} edges={['top']}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando mascotas...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <View style={styles.topBar}>
        <View style={styles.topBarText}>
          <Text style={styles.topTitle}>HuellitasSV</Text>
          <Text style={styles.topSubtitle}>
            {pets.length} {pets.length === 1 ? 'mascota' : 'mascotas'}
            {activos ? ' filtradas' : ' disponibles'}
          </Text>
        </View>
        <TouchableOpacity
          style={[styles.filterButton, activos > 0 && styles.filterButtonActive]}
          onPress={() => setShowFilters(true)}
        >
          <Ionicons
            name="funnel-outline"
            size={20}
            color={activos > 0 ? colors.primary : colors.muted}
          />
          {activos > 0 ? (
            <View style={styles.filterBadge}>
              <Text style={styles.filterBadgeText}>{activos}</Text>
            </View>
          ) : null}
        </TouchableOpacity>
      </View>

      {error ? (
        <View style={styles.errorBanner}>
          <Ionicons name="alert-circle" size={18} color={colors.danger} />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity onPress={() => cargar(aplicados)}>
            <Text style={styles.errorRetry}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      ) : null}

      <Modal
        visible={showFilters}
        animationType="slide"
        transparent
        onRequestClose={() => setShowFilters(false)}
      >
        <View style={styles.modalBackdrop}>
          <View style={styles.modalSheet}>
            <View style={styles.modalHeader}>
              <Text style={styles.modalTitle}>Filtros</Text>
              <TouchableOpacity onPress={() => setShowFilters(false)}>
                <Ionicons name="close" size={24} color={colors.text} />
              </TouchableOpacity>
            </View>
            {/* El panel crecio con ubicacion; ahora vive en un modal con scroll
                propio para que nunca se corte el boton de aplicar. */}
            <ScrollView
              contentContainerStyle={styles.filtersPanel}
              showsVerticalScrollIndicator
              keyboardShouldPersistTaps="handled"
            >
          <View style={styles.filterItem}>
            <Text style={styles.filterLabel}>ESPECIE</Text>
            <View style={styles.chipRow}>
              {OPCIONES_ESPECIE.map((op) => (
                <Chip
                  key={op.valor}
                  label={op.label}
                  activo={draft.especie === op.valor}
                  onPress={() => alternar('especie', op.valor)}
                />
              ))}
            </View>
          </View>

          <View style={styles.filterItem}>
            <Text style={styles.filterLabel}>TAMANO</Text>
            <View style={styles.chipRow}>
              {OPCIONES_TAMANO.map((op) => (
                <Chip
                  key={op.valor}
                  label={op.label}
                  activo={draft.tamano === op.valor}
                  onPress={() => alternar('tamano', op.valor)}
                />
              ))}
            </View>
          </View>

          <View style={styles.filterItem}>
            <Text style={styles.filterLabel}>ESTADO DE SALUD</Text>
            <View style={styles.chipRow}>
              {OPCIONES_SALUD.map((op) => (
                <Chip
                  key={op.valor}
                  label={op.label}
                  activo={draft.estadoSalud === op.valor}
                  onPress={() => alternar('estadoSalud', op.valor)}
                />
              ))}
            </View>
          </View>

          <View style={styles.filterItem}>
            <Text style={styles.filterLabel}>EDAD MAXIMA (MESES)</Text>
            <TextInput
              style={styles.ageInput}
              placeholder="Ej. 24"
              placeholderTextColor={colors.placeholder}
              keyboardType="number-pad"
              inputMode="numeric"
              // Solo digitos: antes parseInt("abc") mandaba NaN en la query.
              value={draft.edadMaxMeses}
              onChangeText={(v) =>
                setDraft((prev) => ({
                  ...prev,
                  edadMaxMeses: v.replace(/\D/g, '').slice(0, 3),
                }))
              }
            />
          </View>

          <View style={styles.filterItem}>
            <Text style={styles.filterLabel}>DEPARTAMENTO</Text>
            <View style={styles.chipRow}>
              {DEPARTAMENTOS.map((dep) => (
                <Chip
                  key={dep}
                  label={dep}
                  activo={draft.departamento === dep}
                  onPress={() => alternar('departamento', dep)}
                />
              ))}
            </View>
          </View>

          <View style={styles.filterItem}>
            <Text style={styles.filterLabel}>MUNICIPIO</Text>
            <TextInput
              style={[styles.ageInput, !draft.departamento && styles.inputDisabled]}
              placeholder={
                draft.departamento
                  ? 'Ej. Soyapango'
                  : 'Selecciona primero un departamento'
              }
              placeholderTextColor={colors.placeholder}
              editable={Boolean(draft.departamento)}
              value={draft.municipio}
              onChangeText={(v) =>
                setDraft((prev) => ({
                  ...prev,
                  // El backend rechaza municipio sin departamento; al quitar el
                  // departamento tambien se limpia el municipio.
                  municipio: v,
                }))
              }
            />
          </View>

          <View style={styles.filterActions}>
            <TouchableOpacity
              style={styles.clearButton}
              onPress={() => setDraft(FILTROS_VACIOS)}
              disabled={contarActivos(draft) === 0}
            >
              <Text
                style={[
                  styles.clearButtonText,
                  contarActivos(draft) === 0 && styles.buttonDisabledText,
                ]}
              >
                Limpiar
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[
                styles.applyButton,
                !hayCambiosPendientes && styles.applyButtonDisabled,
              ]}
              onPress={onAplicar}
              disabled={!hayCambiosPendientes}
            >
              <Text
                style={[
                  styles.applyButtonText,
                  !hayCambiosPendientes && styles.applyButtonTextDisabled,
                ]}
              >
                Aplicar filtros
              </Text>
            </TouchableOpacity>
          </View>
            </ScrollView>
          </View>
        </View>
      </Modal>

      <FlatList
        data={pets}
        keyExtractor={(item) => item.idMascota.toString()}
        renderItem={renderPet}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={renderEmpty}
      />
    </SafeAreaView>
  );
}

function Chip({ label, activo, onPress }) {
  return (
    <TouchableOpacity
      style={[styles.chip, activo && styles.chipActive]}
      onPress={onPress}
      activeOpacity={0.7}
    >
      <Text style={[styles.chipText, activo && styles.chipTextActive]}>
        {label}
      </Text>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  modalBackdrop: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.4)',
    justifyContent: 'flex-end',
  },
  modalSheet: {
    backgroundColor: colors.background,
    borderTopLeftRadius: radius.lg,
    borderTopRightRadius: radius.lg,
    maxHeight: '85%',
    paddingBottom: spacing.xl,
  },
  modalHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.page,
    paddingVertical: spacing.lg,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
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
  filterButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    backgroundColor: colors.surfaceMuted,
    borderRadius: radius.pill,
    paddingVertical: 10,
    paddingHorizontal: spacing.xl,
  },
  filterButtonActive: {
    backgroundColor: colors.chipActive,
    borderWidth: 1,
    borderColor: colors.primary,
  },
  filterBadge: {
    backgroundColor: colors.primary,
    borderRadius: 10,
    minWidth: 20,
    height: 20,
    alignItems: 'center',
    justifyContent: 'center',
  },
  filterBadgeText: {
    fontSize: 11,
    fontWeight: '700',
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
  filtersPanel: {
    paddingHorizontal: spacing.page,
    paddingVertical: spacing.xl,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: spacing.lg,
  },
  filterItem: {
    gap: spacing.sm,
  },
  filterLabel: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
  },
  chipRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
  },
  chip: {
    backgroundColor: colors.surfaceMuted,
    borderRadius: radius.sm,
    paddingVertical: 10,
    paddingHorizontal: spacing.lg,
    borderWidth: 1,
    borderColor: 'transparent',
  },
  chipActive: {
    backgroundColor: colors.chipActive,
    borderColor: colors.primary,
  },
  chipText: {
    fontSize: 13,
    color: colors.text,
  },
  chipTextActive: {
    fontWeight: '600',
  },
  ageInput: {
    backgroundColor: colors.surfaceMuted,
    borderRadius: radius.sm,
    borderWidth: 1,
    borderColor: colors.border,
    padding: 10,
    fontSize: 14,
    color: colors.text,
  },
  inputDisabled: {
    opacity: 0.5,
  },
  filterActions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    marginTop: spacing.xs,
  },
  clearButton: {
    paddingVertical: 12,
    paddingHorizontal: spacing.xl,
  },
  clearButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.primary,
  },
  buttonDisabledText: {
    color: colors.muted,
  },
  applyButton: {
    flex: 1,
    backgroundColor: colors.primary,
    borderRadius: radius.md,
    paddingVertical: 13,
    alignItems: 'center',
  },
  applyButtonDisabled: {
    backgroundColor: colors.surfaceMuted,
  },
  applyButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  applyButtonTextDisabled: {
    color: colors.muted,
  },
  list: {
    padding: spacing.page,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: radius.md,
    marginBottom: spacing.xl,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: colors.border,
  },
  imageContainer: {
    position: 'relative',
    height: 180,
  },
  image: {
    width: '100%',
    height: '100%',
  },
  statusBadge: {
    position: 'absolute',
    top: 12,
    right: 12,
    backgroundColor: 'rgba(0,0,0,0.6)',
    borderRadius: 12,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  statusText: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.onPrimary,
    textTransform: 'capitalize',
  },
  info: {
    padding: spacing.md,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: spacing.sm,
  },
  name: {
    fontSize: 18,
    fontWeight: '700',
    color: colors.text,
  },
  speciesBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: colors.primary,
    borderRadius: 12,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  speciesText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.onPrimary,
    textTransform: 'capitalize',
  },
  details: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.xl,
    marginBottom: spacing.sm,
  },
  detail: {
    fontSize: 12,
    color: colors.muted,
  },
  refugio: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    paddingTop: spacing.sm,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  refugioText: {
    flex: 1,
    fontSize: 12,
    color: colors.muted,
  },
  rating: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.accent,
  },
  emptyContainer: {
    alignItems: 'center',
    gap: spacing.md,
    padding: 40,
  },
  emptyText: {
    fontSize: 18,
    fontWeight: '600',
    color: colors.text,
    textAlign: 'center',
  },
  emptySubtext: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
  },
  emptyButton: {
    marginTop: spacing.sm,
    paddingVertical: 10,
    paddingHorizontal: spacing.xl,
    borderRadius: radius.md,
    backgroundColor: colors.surfaceMuted,
  },
  emptyButtonText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.primary,
  },
});

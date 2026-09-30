import { useEffect, useState } from 'react';
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
import { colors } from '../theme/colors';
import { api } from '../services/api';
import { useNavigation } from '@react-navigation/native';

export default function CatalogScreen() {
  const [pets, setPets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    especie: '',
    tamano: '',
    edadMaxMeses: '',
    estadoSalud: '',
  });
  const [showFilters, setShowFilters] = useState(false);
  const nav = useNavigation();

  const fetchCatalogo = async () => {
    try {
      setError(null);
      const data = await api.getCatalogo();
      setPets(data.mascotas || []);
    } catch (err) {
      setError(err.message);
      console.error('Error fetching catalogo:', err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const fetchFiltered = async () => {
    try {
      setError(null);
      const { especie, tamano, edadMaxMeses, estadoSalud } = filters;
      let data;
      
      if (especie && !tamano && !edadMaxMeses && !estadoSalud) {
        data = await api.getMascotasByEspecie(especie);
      } else if (!especie && (tamano || edadMaxMeses || estadoSalud)) {
        data = await api.getMascotasByAtributos({
          tamano: tamano || undefined,
          edadMaxMeses: edadMaxMeses ? parseInt(edadMaxMeses) : undefined,
          estadoSalud: estadoSalud || undefined,
        });
      } else if (especie && (tamano || edadMaxMeses || estadoSalud)) {
        // For combined filters, we'll use the atributos endpoint and filter by especie client-side
        data = await api.getMascotasByAtributos({
          tamano: tamano || undefined,
          edadMaxMeses: edadMaxMeses ? parseInt(edadMaxMeses) : undefined,
          estadoSalud: estadoSalud || undefined,
        });
        data = data.filter(m => m.especie.toLowerCase() === especie.toLowerCase());
      } else {
        data = await api.getCatalogo();
        data = data.mascotas || [];
      }
      
      setPets(data);
    } catch (err) {
      setError(err.message);
      console.error('Error fetching filtered:', err);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    if (Object.values(filters).some(v => v)) {
      fetchFiltered();
    } else {
      fetchCatalogo();
    }
  };

  useEffect(() => {
    fetchCatalogo();
  }, []);

  const renderPet = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      onPress={() => nav.navigate('PetDetail', { pet: item })}
      activeOpacity={0.8}
    >
      <View style={styles.imageContainer}>
        <Image source={{ uri: item.imagenUrl || 'https://via.placeholder.com/300' }} style={styles.image} />
        <View style={styles.statusBadge}>
          <Text style={styles.statusText}>{item.estado}</Text>
        </View>
      </View>
      <View style={styles.info}>
        <View style={styles.header}>
          <Text style={styles.name}>{item.nombre}</Text>
          <View style={styles.speciesBadge}>
            <Ionicons name={item.especie === 'perro' ? 'paw' : item.especie === 'gato' ? 'cat' : 'ellipse'} size={14} color="#FFFFFF" />
            <Text style={styles.speciesText}>{item.especie}</Text>
          </View>
        </View>
        <View style={styles.details}>
          <Text style={styles.detail}>
            <Ionicons name="resize" size={14} color={colors.muted} />
            {item.tamano}
          </Text>
          <Text style={styles.detail}>
            <Ionicons name="time" size={14} color={colors.muted} />
            {item.edadMeses} meses
          </Text>
          <Text style={styles.detail}>
            <Ionicons name="medical" size={14} color={colors.muted} />
            {item.estadoSalud}
          </Text>
        </View>
        <View style={styles.refugio}>
          <Ionicons name="home" size={14} color={colors.muted} />
          <Text style={styles.refugioText}>{item.refugio?.nombreOrganizacion || 'Refugio'}</Text>
        </View>
      </View>
    </TouchableOpacity>
  );

  const renderEmpty = () => (
    <View style={styles.emptyContainer}>
      <Ionicons name="paw" size={64} color={colors.muted} />
      <Text style={styles.emptyText}>No hay mascotas disponibles</Text>
      <Text style={styles.emptySubtext}>Intenta cambiar los filtros o vuelve más tarde</Text>
    </View>
  );

  if (loading && pets.length === 0) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Cargando mascotas...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.filterBar}>
        <TouchableOpacity
          style={[styles.filterButton, showFilters && styles.filterButtonActive]}
          onPress={() => setShowFilters(!showFilters)}
        >
          <Ionicons name="funnel" size={20} color={showFilters ? colors.primary : colors.muted} />
          <Text style={[styles.filterButtonText, showFilters && { color: colors.primary }]}>
            Filtros
          </Text>
          {(filters.especie || filters.tamano || filters.edadMaxMeses || filters.estadoSalud) && (
            <View style={styles.filterBadge}>
              <Text style={styles.filterBadgeText}>
                {Object.values(filters).filter(v => v).length}
              </Text>
            </View>
          )}
        </TouchableOpacity>
      </View>

      {showFilters && (
        <View style={styles.filtersPanel}>
          <View style={styles.filterRow}>
            <View style={styles.filterItem}>
              <Text style={styles.filterLabel}>Especie</Text>
              <TouchableOpacity
                style={[styles.filterSelect, filters.especie && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, especie: filters.especie ? '' : 'perro' })}
              >
                <Text style={styles.filterSelectText}>Perro</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.filterSelect, filters.especie === 'gato' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, especie: filters.especie === 'gato' ? '' : 'gato' })}
              >
                <Text style={styles.filterSelectText}>Gato</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.filterSelect, filters.especie === 'otro' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, especie: filters.especie === 'otro' ? '' : 'otro' })}
              >
                <Text style={styles.filterSelectText}>Otro</Text>
              </TouchableOpacity>
            </View>

            <View style={styles.filterItem}>
              <Text style={styles.filterLabel}>Tamaño</Text>
              <TouchableOpacity
                style={[styles.filterSelect, filters.tamano === 'pequeño' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, tamano: filters.tamano === 'pequeño' ? '' : 'pequeño' })}
              >
                <Text style={styles.filterSelectText}>Pequeño</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.filterSelect, filters.tamano === 'mediano' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, tamano: filters.tamano === 'mediano' ? '' : 'mediano' })}
              >
                <Text style={styles.filterSelectText}>Mediano</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.filterSelect, filters.tamano === 'grande' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, tamano: filters.tamano === 'grande' ? '' : 'grande' })}
              >
                <Text style={styles.filterSelectText}>Grande</Text>
              </TouchableOpacity>
            </View>
          </View>

          <View style={styles.filterRow}>
            <View style={styles.filterItem}>
              <Text style={styles.filterLabel}>Edad máx (meses)</Text>
              <View style={styles.ageInputWrapper}>
                <TextInput
                  style={styles.ageInput}
                  placeholder="Ej. 24"
                  keyboardType="numeric"
                  value={filters.edadMaxMeses}
                  onChangeText={(v) => setFilters({ ...filters, edadMaxMeses: v })}
                />
              </View>
            </View>

            <View style={styles.filterItem}>
              <Text style={styles.filterLabel}>Estado salud</Text>
              <TouchableOpacity
                style={[styles.filterSelect, filters.estadoSalud === 'sano' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, estadoSalud: filters.estadoSalud === 'sano' ? '' : 'sano' })}
              >
                <Text style={styles.filterSelectText}>Sano</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.filterSelect, filters.estadoSalud === 'en_tratamiento' && styles.filterSelectActive]}
                onPress={() => setFilters({ ...filters, estadoSalud: filters.estadoSalud === 'en_tratamiento' ? '' : 'en_tratamiento' })}
              >
                <Text style={styles.filterSelectText}>En tratamiento</Text>
              </TouchableOpacity>
            </View>
          </View>

          <TouchableOpacity
            style={styles.clearFiltersButton}
            onPress={() => setFilters({ especie: '', tamano: '', edadMaxMeses: '', estadoSalud: '' })}
          >
            <Text style={styles.clearFiltersText}>Limpiar filtros</Text>
          </TouchableOpacity>
        </View>
      )}

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

// Need to import TextInput
import { TextInput } from 'react-native';

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 12,
  },
  loadingText: {
    fontSize: 14,
    color: colors.muted,
  },
  filterBar: {
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  filterButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: '#F1F5F9',
    borderRadius: 24,
    paddingVertical: 10,
    paddingHorizontal: 16,
  },
  filterButtonActive: {
    backgroundColor: '#FEF3C7',
    borderWidth: 1,
    borderColor: colors.primary,
  },
  filterButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.text,
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
    color: '#FFFFFF',
  },
  filtersPanel: {
    padding: 16,
    backgroundColor: colors.card,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 16,
  },
  filterRow: {
    flexDirection: 'row',
    gap: 12,
    flexWrap: 'wrap',
  },
  filterItem: {
    flex: 1,
    minWidth: 140,
    gap: 8,
  },
  filterLabel: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 0.5,
  },
  filterSelect: {
    backgroundColor: '#F1F5F9',
    borderRadius: 8,
    paddingVertical: 10,
    paddingHorizontal: 12,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'transparent',
  },
  filterSelectActive: {
    backgroundColor: '#FEF3C7',
    borderColor: colors.primary,
  },
  filterSelectText: {
    fontSize: 13,
    color: colors.text,
  },
  ageInputWrapper: {
    backgroundColor: '#F1F5F9',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#E2E8F0',
  },
  ageInput: {
    padding: 10,
    fontSize: 14,
    color: colors.text,
  },
  clearFiltersButton: {
    alignSelf: 'flex-start',
    paddingVertical: 8,
    paddingHorizontal: 16,
  },
  clearFiltersText: {
    fontSize: 13,
    color: colors.primary,
    fontWeight: '600',
  },
  list: {
    padding: 16,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: 12,
    marginBottom: 16,
    overflow: 'hidden',
    elevation: 3,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
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
    color: '#FFFFFF',
    textTransform: 'capitalize',
  },
  info: {
    padding: 12,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
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
    color: '#FFFFFF',
    textTransform: 'capitalize',
  },
  details: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 16,
    marginBottom: 8,
  },
  detail: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    fontSize: 12,
    color: colors.muted,
  },
  refugio: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  refugioText: {
    fontSize: 12,
    color: colors.muted,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 12,
    padding: 32,
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
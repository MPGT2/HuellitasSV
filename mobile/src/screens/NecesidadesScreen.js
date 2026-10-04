import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';

// Estos valores son los que acepta el backend: PublicarNecesidadDto valida
// TipoInsumo con la expresion regular ^(alimento|medicina|manta|accesorio)$.
// Antes se ofrecian "comida", "cobijas", "juguetes" y "otro", que el servidor
// rechazaba, y el usuario no podia publicar nada.
const TIPOS = ['alimento', 'medicina', 'manta', 'accesorio'];

export default function NecesidadesScreen() {
  const { user } = useAuth();
  const [necesidades, setNecesidades] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tipoInsumo, setTipoInsumo] = useState('alimento');
  const [descripcion, setDescripcion] = useState('');
  const [cantidad, setCantidad] = useState('');
  const [publicando, setPublicando] = useState(false);

  const cargar = useCallback(async () => {
    try {
      const data = await api.getNecesidadesPorRefugio(user.idRefugio, 'todas');
      setNecesidades(Array.isArray(data) ? data : []);
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setLoading(false);
    }
  }, [user.idRefugio]);

  useFocusEffect(
    useCallback(() => {
      cargar();
    }, [cargar]),
  );

  const publicar = async () => {
    const cant = parseInt(cantidad, 10);
    if (!cant || cant <= 0) {
      Alert.alert('Cantidad inválida', 'Ingresa una cantidad mayor a 0.');
      return;
    }
    // El backend exige el IdRefugio en el cuerpo: sin el, la validacion del DTO
    // lo rechaza antes de usar el id del token.
    if (!user?.idRefugio) {
      Alert.alert(
        'Sesión incompleta',
        'No se pudo identificar tu refugio. Cierra sesión y vuelve a entrar.',
      );
      return;
    }
    setPublicando(true);
    try {
      await api.publicarNecesidad(user.idRefugio, tipoInsumo, descripcion.trim(), cant);
      setDescripcion('');
      setCantidad('');
      await cargar();
      Alert.alert('Publicada', 'Tu necesidad ya es visible para la comunidad.');
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setPublicando(false);
    }
  };

  const eliminar = (item) => {
    Alert.alert('Eliminar necesidad', '¿Seguro que quieres eliminarla?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: async () => {
          try {
            await api.eliminarNecesidad(item.idNecesidad);
            await cargar();
          } catch (err) {
            Alert.alert('Error', err.message);
          }
        },
      },
    ]);
  };

  const renderItem = ({ item }) => {
    const pct =
      item.cantidadRequerida > 0
        ? Math.min(1, item.cantidadCubierta / item.cantidadRequerida)
        : 0;

    return (
      <View style={styles.card}>
        <View style={styles.cardTop}>
          <Text style={styles.tipo}>{item.tipoInsumo}</Text>
          {item.estado === 'activa' && item.cantidadCubierta === 0 ? (
            <TouchableOpacity onPress={() => eliminar(item)}>
              <Ionicons name="trash-outline" size={18} color={colors.danger} />
            </TouchableOpacity>
          ) : null}
        </View>
        {item.descripcion ? <Text style={styles.desc}>{item.descripcion}</Text> : null}
        <View style={styles.progressTrack}>
          <View style={[styles.progressFill, { width: `${pct * 100}%` }]} />
        </View>
        <Text style={styles.meta}>
          {item.cantidadCubierta}/{item.cantidadRequerida} · {item.estado}
        </Text>
      </View>
    );
  };

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <FlatList
        data={necesidades}
        keyExtractor={(item) => item.idNecesidad.toString()}
        renderItem={renderItem}
        contentContainerStyle={styles.list}
        ListHeaderComponent={
          <View>
            <View style={styles.formBox}>
              <Text style={styles.formTitle}>Publicar una necesidad</Text>
              <Text style={styles.label}>TIPO DE INSUMO</Text>
              <View style={styles.chips}>
                {TIPOS.map((t) => (
                  <TouchableOpacity
                    key={t}
                    style={[styles.chip, tipoInsumo === t && styles.chipActive]}
                    onPress={() => setTipoInsumo(t)}
                  >
                    <Text
                      style={[styles.chipText, tipoInsumo === t && styles.chipTextActive]}
                    >
                      {t}
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>
              <FormField
                label="DESCRIPCIÓN"
                placeholder="Ej. Alimento para cachorros"
                value={descripcion}
                onChangeText={setDescripcion}
                multiline
              />
              <FormField
                label="CANTIDAD REQUERIDA"
                placeholder="Ej. 20"
                value={cantidad}
                onChangeText={setCantidad}
                keyboardType="numeric"
              />
              <PrimaryButton
                title={publicando ? 'Publicando...' : 'Publicar necesidad'}
                icon="heart-outline"
                onPress={publicar}
                disabled={publicando}
              />
            </View>
            <Text style={styles.sectionTitle}>Mis necesidades ({necesidades.length})</Text>
          </View>
        }
        ListEmptyComponent={
          loading ? (
            <ActivityIndicator style={styles.loader} color={colors.primary} />
          ) : (
            <Text style={styles.empty}>Aún no has publicado necesidades.</Text>
          )
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
  list: {
    padding: spacing.page,
  },
  formBox: {
    marginBottom: spacing.lg,
  },
  formTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginBottom: spacing.md,
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
    marginBottom: spacing.sm,
  },
  chips: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
    marginBottom: spacing.lg,
  },
  chip: {
    paddingHorizontal: spacing.md,
    paddingVertical: 8,
    borderRadius: radius.pill,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.surfaceAlt,
  },
  chipActive: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  chipText: {
    fontSize: 13,
    color: colors.text,
    textTransform: 'capitalize',
  },
  chipTextActive: {
    color: colors.onPrimary,
    fontWeight: '600',
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginBottom: spacing.md,
  },
  card: {
    backgroundColor: colors.card,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  cardTop: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  tipo: {
    fontSize: 15,
    fontWeight: '700',
    color: colors.text,
    textTransform: 'capitalize',
  },
  desc: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
  },
  progressTrack: {
    height: 8,
    borderRadius: 4,
    backgroundColor: colors.surfaceMuted,
    marginTop: spacing.sm,
    overflow: 'hidden',
  },
  progressFill: {
    height: '100%',
    backgroundColor: colors.primary,
  },
  meta: {
    fontSize: 12,
    color: colors.muted,
    marginTop: spacing.xs,
  },
  loader: {
    marginTop: spacing.lg,
  },
  empty: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
    paddingVertical: spacing.xl,
  },
});

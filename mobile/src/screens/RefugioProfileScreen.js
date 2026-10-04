import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Image,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';
import { useAuth } from '../context/AuthContext';
import { useNavigation, useRoute } from '@react-navigation/native';

export default function RefugioProfileScreen() {
  const nav = useNavigation();
  const route = useRoute();
  const refugioParam = route.params?.refugio;
  const idRefugio = refugioParam?.idRefugio ?? route.params?.idRefugio;
  const { isUsuario } = useAuth();

  const [refugio, setRefugio] = useState(refugioParam ?? null);
  const [mascotas, setMascotas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [miEstrellas, setMiEstrellas] = useState(0);
  const [miComentario, setMiComentario] = useState('');
  const [enviandoCalif, setEnviandoCalif] = useState(false);
  const [necesidades, setNecesidades] = useState([]);
  const [aportandoId, setAportandoId] = useState(null);
  const [aporteCantidad, setAporteCantidad] = useState('');
  const [enviandoAporte, setEnviandoAporte] = useState(false);

  useEffect(() => {
    let activo = true;

    const cargar = async () => {
      try {
        const [perfil, pets, miCalif, needs] = await Promise.all([
          idRefugio
            ? api.getRefugioById(idRefugio).catch(() => null)
            : Promise.resolve(null),
          idRefugio
            ? api.getMascotasByRefugio(idRefugio, 'disponible').catch(() => [])
            : Promise.resolve([]),
          idRefugio && isUsuario
            ? api.getMiCalificacion(idRefugio).catch(() => null)
            : Promise.resolve(null),
          idRefugio
            ? api.getNecesidadesPorRefugio(idRefugio).catch(() => [])
            : Promise.resolve([]),
        ]);
        if (!activo) return;
        if (perfil) setRefugio((prev) => ({ ...prev, ...perfil }));
        setMascotas(Array.isArray(pets) ? pets : []);
        setNecesidades(Array.isArray(needs) ? needs : []);
        if (miCalif) {
          setMiEstrellas(miCalif.estrellas || 0);
          setMiComentario(miCalif.comentario || '');
        }
      } finally {
        if (activo) setLoading(false);
      }
    };

    cargar();
    return () => {
      activo = false;
    };
  }, [idRefugio, isUsuario]);

  const enviarCalificacion = async () => {
    if (!miEstrellas) {
      Alert.alert('Falta la calificación', 'Elige de 1 a 5 estrellas.');
      return;
    }
    setEnviandoCalif(true);
    try {
      const r = await api.calificarRefugio(idRefugio, miEstrellas, miComentario);
      setRefugio((prev) => ({
        ...prev,
        promedioEstrellas: r.promedioEstrellas,
        totalCalificaciones: r.totalCalificaciones,
      }));
      Alert.alert('Gracias', 'Tu calificación fue registrada.');
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setEnviandoCalif(false);
    }
  };

  const enviarAporte = async (idNecesidad) => {
    const cantidad = parseInt(aporteCantidad, 10);
    if (!cantidad || cantidad <= 0) {
      Alert.alert('Cantidad inválida', 'Ingresa una cantidad mayor a 0.');
      return;
    }
    setEnviandoAporte(true);
    try {
      const r = await api.aportarNecesidad(idNecesidad, cantidad);
      Alert.alert('Gracias', r?.mensaje || 'Aporte registrado.');
      setAportandoId(null);
      setAporteCantidad('');
      const actualizadas = await api
        .getNecesidadesPorRefugio(idRefugio)
        .catch(() => necesidades);
      if (Array.isArray(actualizadas)) setNecesidades(actualizadas);
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setEnviandoAporte(false);
    }
  };

  const renderMascota = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      activeOpacity={0.8}
      onPress={() => nav.navigate('PetDetail', { pet: item })}
    >
      {resolveImageUrl(item.imagenUrl) ? (
        <Image
          source={{ uri: resolveImageUrl(item.imagenUrl) }}
          style={styles.cardImage}
        />
      ) : (
        <View style={[styles.cardImage, styles.cardImageEmpty]}>
          <Ionicons name="paw" size={24} color={colors.muted} />
        </View>
      )}
      <View style={styles.cardInfo}>
        <Text style={styles.cardName}>{item.nombre}</Text>
        <Text style={styles.cardMeta}>
          {item.especie} · {item.tamano} · {item.edadMeses} meses
        </Text>
        <Text style={styles.cardMeta}>{item.estadoSalud}</Text>
      </View>
    </TouchableOpacity>
  );

  const promedio = refugio?.promedioEstrellas;

  const header = (
    <View style={styles.headerBlock}>
      <View style={styles.avatar}>
        <Ionicons name="home" size={28} color={colors.onPrimary} />
      </View>
      <Text style={styles.nombre}>
        {refugio?.nombreOrganizacion || 'Refugio'}
      </Text>

      {(refugio?.municipio || refugio?.departamento) && (
        <View style={styles.locationRow}>
          <Ionicons name="location-outline" size={16} color={colors.muted} />
          <Text style={styles.locationText}>
            {[refugio?.municipio, refugio?.departamento].filter(Boolean).join(', ')}
          </Text>
        </View>
      )}

      <View style={styles.ratingRow}>
        {promedio ? (
          <>
            {[...Array(5)].map((_, i) => (
              <Ionicons
                key={i}
                name={i < Math.round(promedio) ? 'star' : 'star-outline'}
                size={18}
                color={colors.warningStrong}
              />
            ))}
            <Text style={styles.ratingText}>
              {promedio.toFixed(1)}
              {refugio?.totalCalificaciones
                ? ` · ${refugio.totalCalificaciones} calificaciones`
                : ''}
            </Text>
          </>
        ) : (
          <Text style={styles.ratingMuted}>Sin calificaciones</Text>
        )}
      </View>

      {refugio?.contacto ? (
        <View style={styles.contactRow}>
          <Ionicons name="call-outline" size={16} color={colors.primary} />
          <Text style={styles.contactText}>{refugio.contacto}</Text>
        </View>
      ) : null}

      {isUsuario && idRefugio ? (
        <View style={styles.calificarBox}>
          <Text style={styles.calificarTitle}>
            {miEstrellas ? 'Tu calificación' : 'Califica este refugio'}
          </Text>
          <View style={styles.starsPick}>
            {[1, 2, 3, 4, 5].map((n) => (
              <TouchableOpacity key={n} onPress={() => setMiEstrellas(n)}>
                <Ionicons
                  name={n <= miEstrellas ? 'star' : 'star-outline'}
                  size={30}
                  color={colors.warningStrong}
                />
              </TouchableOpacity>
            ))}
          </View>
          <TextInput
            style={styles.calificarInput}
            placeholder="Comentario (opcional)"
            placeholderTextColor={colors.placeholder}
            value={miComentario}
            onChangeText={setMiComentario}
            multiline
          />
          <TouchableOpacity
            style={[styles.calificarBtn, enviandoCalif && styles.calificarBtnDisabled]}
            onPress={enviarCalificacion}
            disabled={enviandoCalif}
          >
            <Text style={styles.calificarBtnText}>
              {enviandoCalif ? 'Enviando…' : 'Enviar calificación'}
            </Text>
          </TouchableOpacity>
        </View>
      ) : null}

      {necesidades.length > 0 ? (
        <View style={styles.necesidadesBox}>
          <Text style={styles.sectionTitle}>Necesidades ({necesidades.length})</Text>
          {necesidades.map((n) => {
            const pct =
              n.cantidadRequerida > 0
                ? Math.min(1, n.cantidadCubierta / n.cantidadRequerida)
                : 0;
            return (
              <View key={n.idNecesidad} style={styles.necesidadCard}>
                <Text style={styles.necesidadTipo}>{n.tipoInsumo}</Text>
                {n.descripcion ? (
                  <Text style={styles.necesidadDesc}>{n.descripcion}</Text>
                ) : null}
                <View style={styles.progressTrack}>
                  <View style={[styles.progressFill, { width: `${pct * 100}%` }]} />
                </View>
                <Text style={styles.necesidadMeta}>
                  {n.cantidadCubierta}/{n.cantidadRequerida} · {n.estado}
                </Text>
                {isUsuario ? (
                  <>
                    <TouchableOpacity
                      style={styles.aportarBtn}
                      onPress={() =>
                        setAportandoId(
                          aportandoId === n.idNecesidad ? null : n.idNecesidad,
                        )
                      }
                    >
                      <Text style={styles.aportarBtnText}>
                        {aportandoId === n.idNecesidad ? 'Cancelar' : 'Aportar'}
                      </Text>
                    </TouchableOpacity>
                    {aportandoId === n.idNecesidad ? (
                      <View style={styles.aporteRow}>
                        <TextInput
                          style={styles.aporteInput}
                          keyboardType="numeric"
                          placeholder="Cantidad"
                          placeholderTextColor={colors.placeholder}
                          value={aporteCantidad}
                          onChangeText={setAporteCantidad}
                        />
                        <TouchableOpacity
                          style={styles.aporteConfirm}
                          onPress={() => enviarAporte(n.idNecesidad)}
                          disabled={enviandoAporte}
                        >
                          <Text style={styles.aporteConfirmText}>
                            {enviandoAporte ? '...' : 'Donar'}
                          </Text>
                        </TouchableOpacity>
                      </View>
                    ) : null}
                  </>
                ) : null}
              </View>
            );
          })}
        </View>
      ) : null}

      <Text style={styles.sectionTitle}>
        Mascotas disponibles ({mascotas.length})
      </Text>
    </View>
  );

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <FlatList
        data={mascotas}
        keyExtractor={(item) => item.idMascota.toString()}
        renderItem={renderMascota}
        ListHeaderComponent={header}
        contentContainerStyle={styles.list}
        ListEmptyComponent={
          loading ? (
            <ActivityIndicator
              style={styles.loader}
              size="small"
              color={colors.primary}
            />
          ) : (
            <View style={styles.emptyContainer}>
              <Ionicons name="paw-outline" size={48} color={colors.muted} />
              <Text style={styles.emptyText}>
                Este refugio no tiene mascotas disponibles
              </Text>
            </View>
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
  headerBlock: {
    alignItems: 'center',
    gap: spacing.sm,
    marginBottom: spacing.xl,
  },
  avatar: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.xs,
  },
  nombre: {
    fontSize: 22,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
  },
  locationRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  locationText: {
    fontSize: 14,
    color: colors.muted,
  },
  ratingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 3,
    marginTop: spacing.xs,
  },
  ratingText: {
    fontSize: 13,
    color: colors.muted,
    marginLeft: spacing.xs,
  },
  ratingMuted: {
    fontSize: 13,
    color: colors.muted,
    fontStyle: 'italic',
  },
  contactRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.xs,
    marginTop: spacing.xs,
  },
  contactText: {
    fontSize: 14,
    color: colors.primary,
    fontWeight: '600',
  },
  calificarBox: {
    alignSelf: 'stretch',
    marginTop: spacing.xl,
    padding: spacing.md,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.surfaceAlt,
    gap: spacing.sm,
  },
  calificarTitle: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.text,
  },
  starsPick: {
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
  },
  calificarInput: {
    backgroundColor: colors.background,
    borderRadius: radius.sm,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    fontSize: 14,
    color: colors.text,
    minHeight: 44,
  },
  calificarBtn: {
    backgroundColor: colors.primary,
    borderRadius: radius.md,
    paddingVertical: 12,
    alignItems: 'center',
  },
  calificarBtnDisabled: {
    opacity: 0.6,
  },
  calificarBtnText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  sectionTitle: {
    alignSelf: 'flex-start',
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
    marginTop: spacing.xl,
  },
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    backgroundColor: colors.card,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  cardImage: {
    width: 60,
    height: 60,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceMuted,
  },
  cardImageEmpty: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  cardInfo: {
    flex: 1,
  },
  cardName: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.text,
  },
  cardMeta: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
    textTransform: 'capitalize',
  },
  loader: {
    marginTop: spacing.lg,
  },
  emptyContainer: {
    alignItems: 'center',
    gap: spacing.sm,
    paddingVertical: spacing.xxxl,
  },
  emptyText: {
    fontSize: 14,
    color: colors.muted,
    textAlign: 'center',
  },
  necesidadesBox: {
    alignSelf: 'stretch',
    marginTop: spacing.xl,
  },
  necesidadCard: {
    backgroundColor: colors.card,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  necesidadTipo: {
    fontSize: 15,
    fontWeight: '700',
    color: colors.text,
    textTransform: 'capitalize',
  },
  necesidadDesc: {
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
  necesidadMeta: {
    fontSize: 12,
    color: colors.muted,
    marginTop: spacing.xs,
  },
  aportarBtn: {
    marginTop: spacing.md,
    alignSelf: 'flex-start',
    paddingHorizontal: spacing.md,
    paddingVertical: 8,
    borderRadius: radius.pill,
    borderWidth: 1,
    borderColor: colors.primary,
  },
  aportarBtnText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.primary,
  },
  aporteRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    marginTop: spacing.sm,
  },
  aporteInput: {
    flex: 1,
    backgroundColor: colors.background,
    borderRadius: radius.sm,
    borderWidth: 1,
    borderColor: colors.border,
    paddingHorizontal: spacing.md,
    paddingVertical: 10,
    fontSize: 14,
    color: colors.text,
  },
  aporteConfirm: {
    backgroundColor: colors.primary,
    borderRadius: radius.md,
    paddingHorizontal: spacing.lg,
    paddingVertical: 10,
  },
  aporteConfirmText: {
    fontSize: 14,
    fontWeight: '600',
    color: colors.onPrimary,
  },
});

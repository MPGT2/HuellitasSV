import { useState } from 'react';
import {
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';
import { api } from '../services/api';
import { resolveImageUrl } from '../config/env';
import { useAuth } from '../context/AuthContext';

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
  { valor: 'discapacidad', label: 'Discapacidad' },
  { valor: 'crónico', label: 'Crónico' },
];

const OPCIONES_ESTADO = [
  { valor: 'disponible', label: 'Disponible' },
  { valor: 'reservada', label: 'Reservada' },
  { valor: 'adoptada', label: 'Adoptada' },
  { valor: 'en_tratamiento', label: 'En tratamiento' },
  { valor: 'fallecida', label: 'Fallecida' },
];

export default function MascotaFormScreen({ navigation, route }) {
  const mascota = route.params?.mascota;
  const editando = Boolean(mascota?.idMascota);
  const { user } = useAuth();

  const [form, setForm] = useState({
    nombre: mascota?.nombre ?? '',
    especie: mascota?.especie ?? '',
    tamano: mascota?.tamano ?? '',
    edadMeses: mascota?.edadMeses != null ? String(mascota.edadMeses) : '',
    estadoSalud: mascota?.estadoSalud ?? '',
  });
  const [estado, setEstado] = useState(mascota?.estado ?? 'disponible');
  const [justificacion, setJustificacion] = useState('');
  const [foto, setFoto] = useState(null);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  const handleChange = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validate = () => {
    const next = {};
    if (!form.especie) next.especie = 'Selecciona la especie';
    if (!form.tamano) next.tamano = 'Selecciona el tamaño';
    if (!form.estadoSalud) next.estadoSalud = 'Selecciona el estado de salud';
    const edad = parseInt(form.edadMeses, 10);
    if (!form.edadMeses) next.edadMeses = 'Ingresa la edad en meses';
    else if (Number.isNaN(edad) || edad < 1 || edad > 300)
      next.edadMeses = 'La edad debe estar entre 1 y 300 meses';
    if (form.nombre && form.nombre.trim().length > 100)
      next.nombre = 'El nombre no puede exceder 100 caracteres';
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const elegirFoto = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert(
        'Permiso requerido',
        'Necesitamos acceso a tus fotos para subir la imagen de la mascota.',
      );
      return;
    }
    const res = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      allowsEditing: true,
      quality: 0.7,
    });
    if (!res.canceled) setFoto(res.assets[0]);
  };

  const handleSubmit = async () => {
    if (!validate()) return;
    setLoading(true);
    try {
      if (editando) {
        // El PUT del backend es JSON y no admite imagen: la foto se muestra de solo lectura.
        const body = {
          Nombre: form.nombre.trim(),
          Especie: form.especie,
          Tamano: form.tamano,
          EdadMeses: parseInt(form.edadMeses, 10),
          EstadoSalud: form.estadoSalud,
        };
        if (estado && estado !== mascota.estado) {
          body.Estado = estado;
          if (justificacion.trim()) body.JustificacionCambioEstado = justificacion.trim();
        }
        await api.actualizarMascota(mascota.idMascota, body);
      } else {
        // Alta multipart. IdRefugio es obligatorio en el DTO aunque el backend
        // use el token: si la sesion no lo trae, no tiene sentido intentarlo.
        if (!user?.idRefugio) {
          Alert.alert(
            'Sesión incompleta',
            'No se pudo identificar tu refugio. Cierra sesión y vuelve a entrar.',
          );
          return;
        }
        const data = {
          IdRefugio: String(user.idRefugio),
          Nombre: form.nombre.trim(),
          Especie: form.especie,
          Tamano: form.tamano,
          EdadMeses: String(parseInt(form.edadMeses, 10)),
          EstadoSalud: form.estadoSalud,
        };
        const imagen = foto
          ? {
              uri: foto.uri,
              name: foto.fileName || 'mascota.jpg',
              type: foto.mimeType || 'image/jpeg',
            }
          : null;
        await api.registrarMascota(data, imagen);
      }
      navigation.goBack();
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setLoading(false);
    }
  };

  const previewUri = foto?.uri || resolveImageUrl(mascota?.imagenUrl);

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
          indicatorStyle="black"
        >
          <View style={styles.header}>
            <TouchableOpacity
              style={styles.backButton}
              onPress={() => navigation.goBack()}
            >
              <Ionicons name="chevron-back" size={20} color={colors.text} />
            </TouchableOpacity>
            <View style={styles.headerText}>
              <Text style={styles.title}>
                {editando ? 'Editar mascota' : 'Nueva mascota'}
              </Text>
              <Text style={styles.subtitle}>
                {editando
                  ? 'Actualiza los datos de la mascota.'
                  : 'Completa los datos y sube una foto.'}
              </Text>
            </View>
          </View>

          {/* Foto: seleccionable en el alta; de solo lectura en la edicion. */}
          <Text style={styles.label}>FOTO</Text>
          {previewUri ? (
            <View style={styles.previewWrap}>
              <Image source={{ uri: previewUri }} style={styles.preview} />
              {!editando ? (
                <TouchableOpacity
                  style={styles.previewAction}
                  onPress={elegirFoto}
                  activeOpacity={0.7}
                >
                  <Ionicons name="camera" size={16} color={colors.onPrimary} />
                  <Text style={styles.previewActionText}>Cambiar</Text>
                </TouchableOpacity>
              ) : null}
            </View>
          ) : (
            <TouchableOpacity
              style={styles.uploadBox}
              onPress={elegirFoto}
              activeOpacity={0.7}
            >
              <Ionicons name="camera-outline" size={26} color={colors.placeholder} />
              <Text style={styles.uploadText}>Toca para elegir una foto</Text>
            </TouchableOpacity>
          )}

          <FormField
            label="NOMBRE"
            placeholder="Ej. Firulais"
            value={form.nombre}
            onChangeText={(v) => handleChange('nombre', v)}
            error={errors.nombre}
          />

          <View style={styles.group}>
            <Text style={styles.label}>ESPECIE</Text>
            <View style={styles.chipRow}>
              {OPCIONES_ESPECIE.map((op) => (
                <Chip
                  key={op.valor}
                  label={op.label}
                  activo={form.especie === op.valor}
                  onPress={() => handleChange('especie', op.valor)}
                />
              ))}
            </View>
            {errors.especie ? <Text style={styles.error}>{errors.especie}</Text> : null}
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>TAMAÑO</Text>
            <View style={styles.chipRow}>
              {OPCIONES_TAMANO.map((op) => (
                <Chip
                  key={op.valor}
                  label={op.label}
                  activo={form.tamano === op.valor}
                  onPress={() => handleChange('tamano', op.valor)}
                />
              ))}
            </View>
            {errors.tamano ? <Text style={styles.error}>{errors.tamano}</Text> : null}
          </View>

          <FormField
            label="EDAD (MESES)"
            placeholder="Ej. 24"
            keyboardType="number-pad"
            inputMode="numeric"
            value={form.edadMeses}
            onChangeText={(v) =>
              handleChange('edadMeses', v.replace(/\D/g, '').slice(0, 3))
            }
            error={errors.edadMeses}
          />

          <View style={styles.group}>
            <Text style={styles.label}>ESTADO DE SALUD</Text>
            <View style={styles.chipRow}>
              {OPCIONES_SALUD.map((op) => (
                <Chip
                  key={op.valor}
                  label={op.label}
                  activo={form.estadoSalud === op.valor}
                  onPress={() => handleChange('estadoSalud', op.valor)}
                />
              ))}
            </View>
            {errors.estadoSalud ? (
              <Text style={styles.error}>{errors.estadoSalud}</Text>
            ) : null}
          </View>

          {editando ? (
            <>
              <View style={styles.group}>
                <Text style={styles.label}>ESTADO DE ADOPCIÓN</Text>
                <View style={styles.chipRow}>
                  {OPCIONES_ESTADO.map((op) => (
                    <Chip
                      key={op.valor}
                      label={op.label}
                      activo={estado === op.valor}
                      onPress={() => setEstado(op.valor)}
                    />
                  ))}
                </View>
              </View>

              {estado !== mascota.estado ? (
                <FormField
                  label="JUSTIFICACIÓN DEL CAMBIO (SI APLICA)"
                  placeholder="Obligatoria para volver de 'adoptada' a 'disponible'"
                  value={justificacion}
                  onChangeText={setJustificacion}
                />
              ) : null}
            </>
          ) : null}

          <PrimaryButton
            title={loading ? 'Guardando...' : editando ? 'Guardar cambios' : 'Registrar mascota'}
            icon={loading ? undefined : editando ? 'save-outline' : 'paw'}
            onPress={handleSubmit}
            disabled={loading}
          />
        </ScrollView>
      </KeyboardAvoidingView>
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
      <Text style={[styles.chipText, activo && styles.chipTextActive]}>{label}</Text>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  flex: {
    flex: 1,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: spacing.page,
    paddingTop: spacing.pageTop,
    paddingBottom: spacing.pageBottom,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.lg,
    marginBottom: 28,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: radius.circle,
    backgroundColor: colors.surfaceMuted,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerText: {
    flex: 1,
  },
  title: {
    fontSize: 20,
    fontWeight: '800',
    color: colors.text,
  },
  subtitle: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
    marginBottom: spacing.sm,
  },
  uploadBox: {
    backgroundColor: colors.surfaceAlt,
    borderRadius: radius.md,
    borderWidth: 1.5,
    borderStyle: 'dashed',
    borderColor: colors.borderStrong,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 34,
    marginBottom: spacing.xxl,
    gap: spacing.sm,
  },
  uploadText: {
    fontSize: 13,
    color: colors.muted,
  },
  previewWrap: {
    marginBottom: spacing.xxl,
    borderRadius: radius.md,
    overflow: 'hidden',
  },
  preview: {
    width: '100%',
    height: 200,
    backgroundColor: colors.surfaceMuted,
  },
  previewAction: {
    position: 'absolute',
    bottom: spacing.md,
    right: spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(0,0,0,0.6)',
    borderRadius: radius.pill,
    paddingHorizontal: spacing.md,
    paddingVertical: 6,
  },
  previewActionText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.onPrimary,
  },
  group: {
    marginBottom: spacing.md,
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
  error: {
    fontSize: 12,
    color: colors.danger,
    marginTop: spacing.xs,
  },
});

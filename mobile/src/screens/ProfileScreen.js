import { useState } from 'react';
import {
  Alert,
  ActivityIndicator,
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
import { colors } from '../theme/colors';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';

export default function ProfileScreen() {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    nombre: '',
    correo: '',
    contrasena: '',
    // Refugio specific
    nombreOrganizacion: '',
    departamento: '',
    municipio: '',
    contacto: '',
    latitud: '',
    longitud: '',
    documentacionUrl: '',
  });
  const [errors, setErrors] = useState({});
  const { user, isUsuario, isRefugio, updateUser, logout } = useAuth();

  const fetchProfile = async () => {
    try {
      setLoading(true);
      let data;
      if (isUsuario) {
        data = await api.getUsuarioPerfil();
        setForm(prev => ({
          ...prev,
          nombre: data.nombre,
          correo: data.correo,
        }));
      } else if (isRefugio) {
        data = await api.getRefugioPerfil();
        setForm(prev => ({
          ...prev,
          nombreOrganizacion: data.nombreOrganizacion,
          departamento: data.departamento,
          municipio: data.municipio,
          contacto: data.contacto,
          latitud: data.latitud || '',
          longitud: data.longitud || '',
          documentacionUrl: data.documentacionUrl || '',
          correo: data.correo,
        }));
      }
    } catch (err) {
      console.error('Error fetching profile:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field, value) => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const validate = () => {
    const next = {};
    if (!form.nombre.trim()) next.nombre = 'Ingresa tu nombre';
    if (!form.correo.trim()) next.correo = 'Ingresa tu correo';
    if (isRefugio) {
      if (!form.nombreOrganizacion.trim()) next.nombreOrganizacion = 'Ingresa el nombre del refugio';
      if (!form.departamento.trim()) next.departamento = 'Ingresa el departamento';
      if (!form.municipio.trim()) next.municipio = 'Ingresa el municipio';
      if (!form.contacto.trim()) next.contacto = 'Ingresa información de contacto';
    }
    if (form.contrasena && form.contrasena.length < 8) {
      next.contrasena = 'La contraseña debe tener al menos 8 caracteres';
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSave = async () => {
    if (!validate()) return;
    setSaving(true);
    try {
      const data = { ...form };
      if (!data.contrasena) {
        delete data.contrasena;
      }
      if (isUsuario) {
        // Only send user fields
        await updateUser({
          nombre: data.nombre,
          correo: data.correo,
          contrasena: data.contrasena,
        });
      } else if (isRefugio) {
        await updateUser({
          nombreOrganizacion: data.nombreOrganizacion,
          departamento: data.departamento,
          municipio: data.municipio,
          contacto: data.contacto,
          latitud: data.latitud ? parseFloat(data.latitud) : undefined,
          longitud: data.longitud ? parseFloat(data.longitud) : undefined,
          documentacionUrl: data.documentacionUrl,
          correo: data.correo,
          contrasena: data.contrasena,
        });
      }
      Alert.alert('Éxito', 'Perfil actualizado correctamente');
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setSaving(false);
    }
  };

  const handleLogout = () => {
    Alert.alert(
      'Cerrar sesión',
      '¿Estás seguro de que quieres cerrar sesión?',
      [
        { text: 'Cancelar', style: 'cancel' },
        // Sin esto el boton de confirmar no hacia nada y el usuario se
        // quedaba atrapado en la sesion, sin forma de salir.
        { text: 'Sí, cerrar sesión', onPress: logout },
      ]
    );
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.keyboardView}
      >
        <ScrollView
          contentContainerStyle={styles.scroll}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.avatarSection}>
            <View style={styles.avatar}>
              <Text style={styles.avatarText}>
                {user.nombre?.charAt(0).toUpperCase() || '?'}
              </Text>
            </View>
            <Text style={styles.userName}>{user.nombre}</Text>
            <View style={styles.roleBadge}>
              <Text style={styles.roleText}>{isUsuario ? 'Usuario' : 'Refugio'}</Text>
            </View>
          </View>

          <FormField
            label={isUsuario ? 'NOMBRE COMPLETO' : 'NOMBRE DE CONTACTO'}
            placeholder={isUsuario ? 'Ej. María González' : 'Ej. Juan Pérez'}
            value={form.nombre || form.nombreOrganizacion}
            onChangeText={(v) => handleChange(isUsuario ? 'nombre' : 'nombreOrganizacion', v)}
            error={errors.nombre || errors.nombreOrganizacion}
          />

          {isRefugio && (
            <FormField
              label="NOMBRE DEL REFUGIO"
              placeholder="Ej. Refugio Patitas Felices"
              value={form.nombreOrganizacion}
              onChangeText={(v) => handleChange('nombreOrganizacion', v)}
              error={errors.nombreOrganizacion}
            />
          )}

          {isRefugio && (
            <FormField
              label="DEPARTAMENTO"
              placeholder="Ej. San Salvador"
              value={form.departamento}
              onChangeText={(v) => handleChange('departamento', v)}
              error={errors.departamento}
            />
          )}

          {isRefugio && (
            <FormField
              label="MUNICIPIO"
              placeholder="Ej. San Salvador"
              value={form.municipio}
              onChangeText={(v) => handleChange('municipio', v)}
              error={errors.municipio}
            />
          )}

          {isRefugio && (
            <FormField
              label="CONTACTO (TEL/ EMAIL)"
              placeholder="Ej. 2222-0000 / contacto@refugio.org"
              value={form.contacto}
              onChangeText={(v) => handleChange('contacto', v)}
              error={errors.contacto}
            />
          )}

          {isRefugio && (
            <View style={styles.coordsRow}>
                <FormField
                  style={styles.coordField}
                  label="LATITUD"
                  placeholder="Ej. 13.6929"
                  keyboardType="decimal-pad"
                  value={form.latitud}
                  onChangeText={(v) => handleChange('latitud', v)}
                />
                <FormField
                  style={styles.coordField}
                  label="LONGITUD"
                  placeholder="Ej. -89.2182"
                  keyboardType="decimal-pad"
                  value={form.longitud}
                  onChangeText={(v) => handleChange('longitud', v)}
                />
            </View>
          )}

          {isRefugio && (
            <FormField
              label="URL DOCUMENTACIÓN"
              placeholder="https://refugio.org/docs"
              keyboardType="url"
              value={form.documentacionUrl}
              onChangeText={(v) => handleChange('documentacionUrl', v)}
            />
          )}

          <FormField
            label="CORREO ELECTRÓNICO"
            placeholder="correo@ejemplo.com"
            keyboardType="email-address"
            autoCapitalize="none"
            autoComplete="email"
            value={form.correo}
            onChangeText={(v) => handleChange('correo', v)}
            error={errors.correo}
          />

          <FormField
            label="NUEVA CONTRASEÑA (OPCIONAL)"
            placeholder="Mínimo 8 caracteres"
            secureTextEntry
            value={form.contrasena}
            onChangeText={(v) => handleChange('contrasena', v)}
            error={errors.contrasena}
          />

          <PrimaryButton
            title={saving ? 'Guardando...' : 'Guardar cambios'}
            icon={saving ? undefined : 'save'}
            onPress={handleSave}
            disabled={saving}
          />

          <TouchableOpacity
            style={styles.logoutButton}
            onPress={handleLogout}
          >
            <Ionicons name="log-out" size={18} color="#EF4444" />
            <Text style={styles.logoutText}>Cerrar sesión</Text>
          </TouchableOpacity>

          <Text style={styles.version}>HuellitasSV v1.0.0</Text>
        </ScrollView>
      </KeyboardAvoidingView>
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
  },
  keyboardView: {
    flex: 1,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: 24,
    paddingTop: 20,
    paddingBottom: 40,
  },
  avatarSection: {
    alignItems: 'center',
    marginBottom: 24,
  },
  avatar: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 12,
  },
  avatarText: {
    fontSize: 40,
    fontWeight: '700',
    color: '#FFFFFF',
  },
  userName: {
    fontSize: 22,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 4,
  },
  roleBadge: {
    backgroundColor: '#F1F5F9',
    borderRadius: 16,
    paddingHorizontal: 16,
    paddingVertical: 6,
  },
  roleText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.text,
  },
  coordsRow: {
    flexDirection: 'row',
    gap: 12,
  },
  // Antes decia 'coordsRow > *:nth-child(1)': un selector CSS, no un estilo de
  // React Native. Rompia el parseo del archivo y hacia que la pantalla no
  // cargara. El reparto equitativo de los dos campos se resuelve con flex: 1.
  coordField: {
    flex: 1,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    paddingVertical: 14,
    marginTop: 8,
  },
  logoutText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#EF4444',
  },
  version: {
    textAlign: 'center',
    fontSize: 12,
    color: colors.muted,
    marginTop: 20,
  },
});
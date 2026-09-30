import { useState } from 'react';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import * as DocumentPicker from 'expo-document-picker';
import RoleTabs from '../components/RoleTabs';
import FormField from '../components/FormField';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { useAuth } from '../context/AuthContext';

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const initialForm = {
  name: '',
  shelterName: '',
  location: '',
  foundationYear: '',
  email: '',
  password: '',
  confirmPassword: '',
  // Refugio specific
  departamento: '',
  municipio: '',
  contacto: '',
};

export default function RegisterScreen({ navigation, route }) {
  const [role, setRole] = useState(route.params?.role ?? 'usuario');
  const [form, setForm] = useState(initialForm);
  const [errors, setErrors] = useState({});
  const [documento, setDocumento] = useState(null);
  const [loading, setLoading] = useState(false);
  const { register, clearError } = useAuth();

  const handleChange = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const validateCommon = () => {
    const next = {};
    if (!form.email.trim()) next.email = 'Ingresa tu correo electrónico';
    else if (!EMAIL_REGEX.test(form.email.trim()))
      next.email = 'Ingresa un correo válido';
    if (!form.password) next.password = 'Ingresa una contraseña';
    else if (form.password.length < 8) next.password = 'Mínimo 8 caracteres';
    if (!form.confirmPassword) next.confirmPassword = 'Confirma tu contraseña';
    else if (form.confirmPassword !== form.password)
      next.confirmPassword = 'Las contraseñas no coinciden';
    return next;
  };

  const validateUsuario = () => {
    const next = validateCommon();
    if (!form.name.trim()) next.name = 'Ingresa tu nombre completo';
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const validateRefugio = () => {
    const next = validateCommon();
    if (!form.name.trim()) next.name = 'Ingresa tu nombre completo';
    if (!form.shelterName.trim()) next.shelterName = 'Ingresa el nombre del refugio';
    if (!form.departamento.trim()) next.departamento = 'Ingresa el departamento';
    if (!form.municipio.trim()) next.municipio = 'Ingresa el municipio';
    if (!form.contacto.trim()) next.contacto = 'Ingresa información de contacto';
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = async () => {
    const valid = role === 'usuario' ? validateUsuario() : validateRefugio();
    if (!valid) return;
    setLoading(true);
    clearError();
    try {
      if (role === 'usuario') {
        await register('usuario', {
          nombre: form.name.trim(),
          correo: form.email.trim().toLowerCase(),
          contrasena: form.password,
        });
      } else {
        await register('refugio', {
          nombreOrganizacion: form.shelterName.trim(),
          correo: form.email.trim().toLowerCase(),
          contrasena: form.password,
          departamento: form.departamento.trim(),
          municipio: form.municipio.trim(),
          contacto: form.contacto.trim(),
        }, documento);
      }
      Alert.alert('Éxito', role === 'usuario'
        ? 'Cuenta creada correctamente. Ya puedes iniciar sesión.'
        : 'Solicitud de registro enviada. El equipo admin la revisará en 1-2 días hábiles.'
      );
      // Sin parametro de rol: el login es unico y la API deduce el rol desde
      // la cuenta. RoleSelectionScreen ya no lo manda y LoginScreen no lo lee.
      navigation.navigate('Login');
    } catch (err) {
      Alert.alert('Error', err.message);
    } finally {
      setLoading(false);
    }
  };

  const pickDocumento = async () => {
    const res = await DocumentPicker.getDocumentAsync({
      type: ['application/pdf', 'image/*'],
      copyToCacheDirectory: true,
    });
    if (!res.canceled) {
      setDocumento(res.assets[0]);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
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
            <Text style={styles.title}>Crear cuenta</Text>
            <Text style={styles.subtitle}>
              Es gratis y toma menos de un minuto.
            </Text>
          </View>
        </View>

        <RoleTabs value={role} onChange={setRole} />

        {role === 'refugio' ? (
          <View style={styles.banner}>
            <Text style={styles.bannerText}>
              Las cuentas de Refugio requieren verificación de documentos. El
              equipo admin revisará tu solicitud en 1-2 días hábiles.
            </Text>
          </View>
        ) : null}

        <FormField
          label="NOMBRE COMPLETO"
          placeholder="Ej. María González"
          value={form.name}
          onChangeText={(v) => handleChange('name', v)}
          error={errors.name}
        />

        {role === 'refugio' ? (
          <View>
            <FormField
              label="NOMBRE DEL REFUGIO"
              placeholder="Ej. Refugio Patitas Felices"
              value={form.shelterName}
              onChangeText={(v) => handleChange('shelterName', v)}
              error={errors.shelterName}
            />
            <FormField
              label="DEPARTAMENTO"
              placeholder="Ej. San Salvador"
              value={form.departamento}
              onChangeText={(v) => handleChange('departamento', v)}
              error={errors.departamento}
            />
            <FormField
              label="MUNICIPIO"
              placeholder="Ej. San Salvador"
              value={form.municipio}
              onChangeText={(v) => handleChange('municipio', v)}
              error={errors.municipio}
            />
            <FormField
              label="CONTACTO (TELÉFONO/EMAIL)"
              placeholder="Ej. 2222-0000 / contacto@refugio.org"
              value={form.contacto}
              onChangeText={(v) => handleChange('contacto', v)}
              error={errors.contacto}
            />

            <Text style={styles.label}>
              DOCUMENTOS DE VERIFICACIÓN (PDF, JPG)
            </Text>
            <TouchableOpacity
              style={styles.uploadBox}
              onPress={pickDocumento}
              activeOpacity={0.7}
            >
              <Ionicons name="document-outline" size={26} color={colors.placeholder} />
              <Text style={styles.uploadText}>
                {documento?.name || 'Toca para subir acta o permiso'}
              </Text>
              {documento ? (
                <TouchableOpacity onPress={() => setDocumento(null)}>
                  <Text style={styles.uploadClear}>Quitar archivo</Text>
                </TouchableOpacity>
              ) : null}
            </TouchableOpacity>

            <FormField
              label="CORREO ELECTRÓNICO"
              placeholder="correo@ejemplo.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              value={form.email}
              onChangeText={(v) => handleChange('email', v)}
              error={errors.email}
            />
            <FormField
              label="CONTRASEÑA"
              placeholder="Mínimo 8 caracteres"
              secureTextEntry
              value={form.password}
              onChangeText={(v) => handleChange('password', v)}
              error={errors.password}
            />
            <FormField
              label="CONFIRMAR CONTRASEÑA"
              placeholder="Repite tu contraseña"
              secureTextEntry
              value={form.confirmPassword}
              onChangeText={(v) => handleChange('confirmPassword', v)}
              error={errors.confirmPassword}
            />

            <PrimaryButton
              title={loading ? 'Enviando...' : 'Enviar solicitud de registro'}
              icon={loading ? undefined : 'paw'}
              color={colors.accent}
              onPress={handleSubmit}
              disabled={loading}
            />
          </View>
        ) : (
          <View>
            <FormField
              label="CORREO ELECTRÓNICO"
              placeholder="correo@ejemplo.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              value={form.email}
              onChangeText={(v) => handleChange('email', v)}
              error={errors.email}
            />
            <FormField
              label="CONTRASEÑA"
              placeholder="Mínimo 8 caracteres"
              secureTextEntry
              value={form.password}
              onChangeText={(v) => handleChange('password', v)}
              error={errors.password}
            />
            <FormField
              label="CONFIRMAR CONTRASEÑA"
              placeholder="Repite tu contraseña"
              secureTextEntry
              value={form.confirmPassword}
              onChangeText={(v) => handleChange('confirmPassword', v)}
              error={errors.confirmPassword}
            />

            <PrimaryButton
              title={loading ? 'Creando cuenta...' : 'Crear mi cuenta'}
              icon={loading ? undefined : 'paw'}
              onPress={handleSubmit}
              disabled={loading}
            />
          </View>
        )}

        <Text style={styles.terms}>
          Al registrarte aceptas los{' '}
          <Text style={styles.termsLink}>Términos de Uso</Text> y la{' '}
          <Text style={styles.termsLink}>Política de Privacidad</Text> de
          HuellitasSV.
        </Text>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: 24,
    paddingTop: 40,
    paddingBottom: 64,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    marginBottom: 28,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
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
  banner: {
    backgroundColor: colors.warningBg,
    borderRadius: 12,
    padding: 14,
    marginBottom: 20,
  },
  bannerText: {
    fontSize: 13,
    color: colors.warningText,
    lineHeight: 19,
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
    marginBottom: 8,
  },
  uploadBox: {
    backgroundColor: colors.surfaceAlt,
    borderRadius: 12,
    borderWidth: 1.5,
    borderStyle: 'dashed',
    borderColor: colors.borderStrong,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 28,
    marginBottom: 20,
    gap: 10,
  },
  uploadText: {
    fontSize: 13,
    color: colors.muted,
  },
  uploadClear: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.primary,
    marginTop: 2,
  },
  terms: {
    fontSize: 13,
    color: colors.muted,
    textAlign: 'center',
    lineHeight: 20,
    marginTop: 20,
  },
  termsLink: {
    color: colors.primary,
    fontWeight: '600',
  },
});

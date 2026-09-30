import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import PrimaryButton from '../components/PrimaryButton';
import { colors } from '../theme/colors';
import { radius, spacing } from '../theme/spacing';

export default function RoleSelectionScreen({ navigation }) {
  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <ScrollView
        contentContainerStyle={styles.scroll}
        showsVerticalScrollIndicator={false}
      >
        {/* Encabezado con la misma gramática que RegisterScreen: marca circular,
            título en 800 y subtítulo chico en muted. */}
        <View style={styles.header}>
          <View style={styles.logoRow}>
            <View style={styles.logoCircle}>
              <Ionicons name="paw" size={22} color={colors.onPrimary} />
            </View>
            <View style={styles.logoText}>
              <Text style={styles.title}>HuellitasSV</Text>
              <Text style={styles.subtitle}>
                Conectando mascotas con hogares y refugios
              </Text>
            </View>
          </View>
        </View>

        {/* Las dos rutas de entrada con el mismo peso visual. Antes "Registrarse"
            era un link suelto al pie y la pantalla parecia a medio hacer. */}
        <View style={styles.actions}>
          <PrimaryButton
            title="Iniciar sesión"
            icon="log-in"
            onPress={() => navigation.navigate('Login')}
          />
          <PrimaryButton
            title="Crear cuenta"
            icon="paw"
            color={colors.accent}
            onPress={() => navigation.navigate('Register')}
          />
        </View>

        {/* Aviso para refugios: el registro de refugio no es inmediato, y sin
            este texto el usuario no lo sabe hasta ver la pantalla de registro. */}
        <View style={styles.banner}>
          <Ionicons name="business-outline" size={18} color={colors.warningText} />
          <Text style={styles.bannerText}>
            ¿Tenés un refugio? Tu cuenta se revisa en 1-2 días hábiles.
          </Text>
        </View>

        <Text style={styles.terms}>
          Al crear una cuenta aceptas los{' '}
          <Text style={styles.termsLink}>Términos de Uso</Text> y la{' '}
          <Text style={styles.termsLink}>Política de Privacidad</Text> de
          HuellitasSV.
        </Text>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scroll: {
    flexGrow: 1,
    paddingHorizontal: spacing.page,
    paddingTop: spacing.pageTop,
    paddingBottom: spacing.pageBottom,
    justifyContent: 'center',
  },
  header: {
    marginBottom: spacing.xxxl,
  },
  logoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  logoText: {
    flex: 1,
  },
  logoCircle: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  title: {
    fontSize: 24,
    fontWeight: '800',
    color: colors.text,
  },
  subtitle: {
    fontSize: 13,
    color: colors.muted,
    marginTop: 2,
  },
  actions: {
    gap: spacing.md,
  },
  banner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
    backgroundColor: colors.warningBg,
    borderRadius: radius.md,
    padding: spacing.lg,
    marginTop: spacing.xxl,
  },
  bannerText: {
    flex: 1,
    fontSize: 13,
    color: colors.warningText,
    lineHeight: 19,
  },
  terms: {
    fontSize: 13,
    color: colors.muted,
    textAlign: 'center',
    lineHeight: 20,
    marginTop: spacing.xxl,
  },
  termsLink: {
    color: colors.primary,
    fontWeight: '600',
  },
});

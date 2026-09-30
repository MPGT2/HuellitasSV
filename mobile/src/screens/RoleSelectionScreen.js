import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import RoleCard from '../components/RoleCard';
import { colors } from '../theme/colors';

export default function RoleSelectionScreen({ navigation }) {
  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={styles.logoRow}>
          <View style={styles.logoCircle}>
            <Ionicons name="paw" size={22} color="#FFFFFF" />
          </View>
          <Text style={styles.title}>HuellitasSV</Text>
        </View>
        <Text style={styles.subtitle}>
          Conectando mascotas con hogares y refugios.
        </Text>
      </View>

      <Text style={styles.sectionLabel}>INICIAR SESIÓN COMO</Text>

      <View style={styles.cards}>
        <RoleCard
          icon="person"
          color={colors.primary}
          title="Usuario"
          description="Adopta mascotas, reporta animales y dona insumos."
          onPress={() => navigation.navigate('Catalog')}
        />
        <RoleCard
          icon="home"
          color={colors.accent}
          title="Refugio"
          description="Gestiona rescates, mascotas y solicitudes de adopción."
          onPress={() => navigation.navigate('Catalog')}
        />
      </View>

      <View style={styles.footer}>
        <Text style={styles.footerText}>¿No tienes cuenta? </Text>
        <TouchableOpacity onPress={() => navigation.navigate('Register')}>
          <Text style={styles.footerLink}>Regístrate</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    paddingHorizontal: 24,
    paddingTop: 60,
  },
  header: {
    marginBottom: 32,
  },
  logoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    marginBottom: 12,
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
    fontSize: 28,
    fontWeight: '800',
    color: colors.text,
  },
  subtitle: {
    fontSize: 15,
    color: colors.muted,
  },
  sectionLabel: {
    fontSize: 12,
    fontWeight: '600',
    color: '#94A3B8',
    letterSpacing: 1.5,
    marginBottom: 16,
  },
  cards: {
    gap: 16,
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'center',
    marginTop: 32,
  },
  footerText: {
    fontSize: 14,
    color: colors.muted,
  },
  footerLink: {
    fontSize: 14,
    color: colors.primary,
    fontWeight: '600',
  },
});

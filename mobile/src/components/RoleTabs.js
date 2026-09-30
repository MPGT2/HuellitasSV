import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';

const ROLES = [
  { id: 'usuario', label: 'Usuario', icon: 'person', color: colors.primary },
  { id: 'refugio', label: 'Refugio', icon: 'home', color: colors.accent },
];

export default function RoleTabs({ value, onChange }) {
  return (
    <View style={styles.container}>
      <Text style={styles.sectionLabel}>TIPO DE CUENTA</Text>
      <View style={styles.tabs}>
        {ROLES.map((role) => {
          const selected = value === role.id;
          return (
            <TouchableOpacity
              key={role.id}
              style={[styles.tab, selected ? styles.tabSelected : null]}
              onPress={() => onChange(role.id)}
              activeOpacity={0.7}
            >
              <View
                style={[
                  styles.iconContainer,
                  selected ? { backgroundColor: role.color } : styles.iconMuted,
                ]}
              >
                <Ionicons
                  name={role.icon}
                  size={22}
                  color={selected ? colors.onPrimary : colors.placeholder}
                />
              </View>
              <Text style={[styles.label, selected ? styles.labelSelected : null]}>
                {role.label}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    marginBottom: 24,
  },
  sectionLabel: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
    marginBottom: 12,
  },
  tabs: {
    flexDirection: 'row',
    gap: 12,
  },
  tab: {
    flex: 1,
    backgroundColor: colors.card,
    borderRadius: 14,
    borderWidth: 1.5,
    borderColor: colors.border,
    alignItems: 'center',
    paddingVertical: 16,
  },
  tabSelected: {
    borderColor: colors.text,
    backgroundColor: colors.primarySoft,
  },
  iconContainer: {
    width: 44,
    height: 44,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 8,
  },
  iconMuted: {
    backgroundColor: colors.surfaceMuted,
  },
  label: {
    fontSize: 13,
    fontWeight: '500',
    color: colors.muted,
  },
  labelSelected: {
    fontWeight: '700',
    color: colors.text,
  },
});

import { useState } from 'react';
import { StyleSheet, Text, TextInput, TouchableOpacity, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme/colors';

export default function FormField({ label, error, secureTextEntry, ...inputProps }) {
  const [visible, setVisible] = useState(false);
  const isPassword = secureTextEntry === true;

  return (
    <View style={styles.container}>
      <Text style={styles.label}>{label}</Text>
      <View style={styles.inputWrapper}>
        <TextInput
          style={[styles.input, error ? styles.inputError : null]}
          placeholderTextColor="#94A3B8"
          secureTextEntry={isPassword && !visible}
          {...inputProps}
        />
        {isPassword ? (
          <TouchableOpacity
            style={styles.eye}
            onPress={() => setVisible((prev) => !prev)}
          >
            <Ionicons
              name={visible ? 'eye-off-outline' : 'eye-outline'}
              size={20}
              color={colors.muted}
            />
          </TouchableOpacity>
        ) : null}
      </View>
      {error ? <Text style={styles.error}>{error}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    marginBottom: 18,
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.text,
    letterSpacing: 1,
    marginBottom: 8,
  },
  inputWrapper: {
    position: 'relative',
    justifyContent: 'center',
  },
  input: {
    backgroundColor: '#F1F5F9',
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    paddingRight: 44,
    fontSize: 15,
    color: colors.text,
  },
  inputError: {
    borderWidth: 1,
    borderColor: '#EF4444',
  },
  eye: {
    position: 'absolute',
    right: 14,
  },
  error: {
    fontSize: 12,
    color: '#EF4444',
    marginTop: 6,
  },
});

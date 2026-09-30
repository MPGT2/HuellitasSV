# HuellitasSV — Documentación del proceso actual

> App móvil para conectar mascotas con hogares y refugios en El Salvador.
> Los usuarios adoptan mascotas, reportan animales y donan insumos; los refugios gestionan rescates y solicitudes de adopción. La administración se manejará en una web separada.

---

## 1. Stack tecnológico

| Tecnología | Versión | Descripción |
|---|---|---|
| **Expo SDK** | ~57.0.23 | Framework principal sobre React Native, facilita desarrollo y build |
| **React** | 19.2.3 | Librería de UI (componentes funcionales + hooks) |
| **React Native** | 0.86.3 | Apps nativas para Android e iOS desde JS/JSX |
| **Lenguaje** | JavaScript (JSX) | Sin TypeScript por decisión del proyecto |
| **Plataformas** | Android, iOS, Web | Configuradas en `app.json` |

## 2. Librerías (dependencias)

### Instaladas de base (proyecto inicial)

| Librería | Versión | ¿Para qué se ocupa? |
|---|---|---|
| `@react-navigation/native` | ^7.4.1 | Núcleo de navegación entre pantallas (Stack, Tabs, etc.) |
| `@react-navigation/native-stack` | ^7.19.1 | Navegador de tipo stack con transiciones nativas |
| `expo-status-bar` | ~57.0.1 | Controla el estilo/color de la barra de estado del sistema |
| `react-native-safe-area-context` | ~5.7.0 | Respeta notches, barras del sistema y áreas seguras del dispositivo |
| `react-native-screens` | ~4.26.0 | Optimización nativa de pantallas (memoria y rendimiento en navegación) |

### Agregada manualmente

| Librería | ¿Cómo se instaló? | ¿Para qué se ocupa? |
|---|---|---|
| `@expo/vector-icons` | `npx expo install @expo/vector-icons` | Íconos listos para usar (pata, usuario, casa, chevron, ojo, documento). Usamos la familia `Ionicons` |

```bash
# comando usado
npx expo install @expo/vector-icons
```

> `npx expo install` en lugar de `npm install` garantiza una versión compatible con el SDK 57 de Expo.

## 3. Estructura del proyecto

```
HuellitasSV/
├── App.js                          # Entry point: NavigationContainer + AppNavigator
├── index.js                        # Registro de la app
├── app.json                        # Config de Expo (nombre, slug, iconos, orientación)
├── package.json                    # Dependencias y scripts
├── assets/                         # Iconos e imágenes de la app
└── src/
    ├── theme/
    │   └── colors.js               # Paleta central de colores
    ├── components/
    │   ├── PrimaryButton.js        # Botón principal (soporta ícono y color personalizado)
    │   ├── RoleCard.js             # Card de selección de rol con ícono y chevron
    │   ├── RoleTabs.js             # Tabs Usuario/Refugio del registro
    │   └── FormField.js            # Input con label, error inline y toggle de ojo
    ├── navigation/
    │   └── AppNavigator.js         # Rutas: RoleSelection → Register → Catalog
    └── screens/
        ├── RoleSelectionScreen.js  # Pantalla de inicio / selección de rol
        ├── RegisterScreen.js       # Registro Usuario y Refugio (misma pantalla)
        └── CatalogScreen.js        # Catálogo (placeholder por ahora)
```

### Paleta de colores (`src/theme/colors.js`)

| Nombre | Hex | Uso |
|---|---|---|
| `primary` | `#0EA5E9` | Azul — rol Usuario, links, botón principal |
| `accent` | `#F59E0B` | Naranja — rol Refugio, botón de solicitud de registro |
| `text` | `#1E293B` | Texto principal y títulos |
| `muted` | `#64748B` | Textos secundarios y subtítulos |
| `border` | `#E2E8F0` | Bordes de cards e inputs |
| `card` | `#FFFFFF` | Fondo de cards |
| `background` | `#FFFFFF` | Fondo general |

## 4. Flujo de navegación

```
RoleSelection (inicio)
   │
   ├─ card Usuario / card Refugio ──→ Catalog (placeholder)
   └─ "Regístrate" ──→ Register
                          │
                          ├─ tab Usuario → formulario usuario → (TODO: BD)
                          └─ tab Refugio → formulario refugio → (TODO: BD)
```

- `Register` acepta rol inicial por params: `navigation.navigate('Register', { role: 'refugio' })`
- El botón atrás usa `navigation.goBack()`
- La ruta `Register` tiene `headerShown: false` (header custom propio)

## 5. Pantallas implementadas

### 5.1 RoleSelectionScreen (selección de rol)

- Logo círculo azul con ícono de pata + **HuellitasSV** + subtítulo
- Label `INICIAR SESIÓN COMO` (uppercase, letter-spacing)
- Dos `RoleCard`:
  - **Usuario** (azul, ícono persona): "Adopta mascotas, reporta animales y dona insumos."
  - **Refugio** (naranja, ícono casa): "Gestiona rescates, mascotas y solicitudes de adopción."
- Footer: "¿No tienes cuenta? **Regístrate**"
- **Decisión de diseño:** no existe la opción Administrador (será una web separada)

### 5.2 RegisterScreen (registro, ambos roles en la misma pantalla)

- Header: botón circular atrás + "Crear cuenta" + subtítulo
- **RoleTabs:** intercambio Usuario/Refugio sin perder datos de campos comunes (el estado del formulario se comparte)
- **Formulario Usuario:**
  - Nombre completo, Correo, Contraseña, Confirmar contraseña
  - Botón azul **🐾 Crear mi cuenta**
- **Formulario Refugio:**
  - Banner amarillo informativo (verificación de documentos, 1-2 días hábiles)
  - Nombre completo, Nombre del refugio, Ubicación, Año de fundación
  - Caja punteada para subir acta/permiso (visual, sin funcionalidad aún)
  - Correo, Contraseñas
  - Botón naranja **🐾 Enviar solicitud de registro**
- Footer: Términos de Uso / Política de Privacidad

### Validaciones del formulario

| Campo | Regla |
|---|---|
| Nombre completo | Requerido |
| Correo | Requerido + regex `/^[^\s@]+@[^\s@]+\.[^\s@]+$/` |
| Contraseña | Requerida, mínimo 8 caracteres |
| Confirmar | Debe coincidir con la contraseña |
| Año de fundación (refugio) | 4 dígitos, ej. `2015` |

Los errores se muestran inline bajo cada campo (borde rojo + mensaje). Al corregir un campo, su error se limpia automáticamente.

## 6. Manejo del teclado (lección aprendida)

**Implementación actual:**

```jsx
<KeyboardAvoidingView
  style={styles.container}
  behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
>
  <ScrollView
    contentContainerStyle={styles.scroll}
    keyboardShouldPersistTaps="handled"
    indicatorStyle="black"
  >
    {/* formulario */}
  </ScrollView>
</KeyboardAvoidingView>
```

**Historia:** primero se intentó una solución custom que en el `onFocus` de cada input medía su posición con `measureInWindow` y hacía `scrollBy` al detectar el teclado. **Fallaba en Expo Go** porque en versiones recientes de React Native `e.target` del evento focus es un node handle numérico, no un componente, por lo que no acepta métodos de medición. Se reemplazó por el patrón estándar `KeyboardAvoidingView` + `ScrollView`, más simple y estable.

**Extras UX:**
- `keyboardShouldPersistTaps="handled"`: permite tocar otros inputs/botones con el teclado abierto sin cerrarlo primero
- Toggle de ojo (👁) integrado **dentro** de cada input de contraseña (en `FormField`), en vez de un control externo
- Toggle mostrar/ocultar contraseñas: `secureTextEntry` condicional por campo

## 7. Componentes reutilizables

| Componente | Props | Notas |
|---|---|---|
| `PrimaryButton` | `title`, `icon?`, `color?`, `onPress` | El color default es `primary`; Refugio usa `accent` |
| `RoleCard` | `icon`, `color`, `title`, `description`, `onPress` | Card con sombra sutil, ícono circular y chevron |
| `RoleTabs` | `value`, `onChange` | Resalta tab activa con borde oscuro, fondo celeste e ícono de color |
| `FormField` | `label`, `error?`, `secureTextEntry`, `...TextInputProps` | Si es password muestra el ojo para alternar visibilidad |

## 8. Pendientes (TODO)

- [ ] Base de datos y backend (registro/login reales)
- [ ] Pantalla de login
- [ ] Integrar `expo-document-picker` para subir acta/permiso del refugio
- [ ] Conectar `handleSubmit` del registro con la API
- [ ] Web de administración (proyecto separado)
- [ ] Pantalla de Catálogo real (hoy es placeholder)
- [ ] Términos de Uso y Política de Privacidad (rutas/contenido)

## 9. Comandos útiles

```bash
npx expo start              # servidor de desarrollo (Expo Go)
npx expo start --android    # abre directamente en Android
npx expo start --ios        # abre directamente en iOS
npx expo start --web        # versión web
```

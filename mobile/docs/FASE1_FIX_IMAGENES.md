# FASE 1: Corrección de Bug de Upload de Imágenes

## ✅ Cambios Implementados

### 1. Nuevo archivo: `src/utils/fileHelpers.js`
**Funciones principales:**
- `buildFormDataFile(asset, defaultFileName)` - Construye objetos compatibles con FormData
- `isValidAsset(asset)` - Valida que un asset sea válido
- `getAssetInfo(asset)` - Obtiene información del archivo para debugging
- `extractMimeType(uri, mimeType)` - Extrae el MIME type correcto
- `extractFileName(asset, defaultName)` - Extrae nombre de archivo válido

**Soluciona:**
- ✅ Propiedades inconsistentes de expo-image-picker v57
- ✅ fileName puede ser null
- ✅ mimeType puede ser undefined
- ✅ Genera nombres de archivo válidos automáticamente
- ✅ Detecta MIME type por extensión como fallback

### 2. Actualización de `src/services/api.js`
**Métodos actualizados:**
- `registerRefugio()` - Línea 124-145
- `registrarMascota()` - Línea 235-256
- `actualizarMascota()` - Línea 258-278
- `crearReporte()` - Línea 333-351

**Mejoras en manejo de errores:**
- ✅ Mensajes más descriptivos
- ✅ Diferenciación entre error de red y error de archivo
- ✅ Logging detallado en modo desarrollo
- ✅ Sugerencias específicas para cada tipo de error

### 3. Actualización de Screens
**Archivos modificados:**
- `src/screens/MascotaFormScreen.js`
- `src/screens/ReporteFormScreen.js`
- `src/screens/RegisterScreen.js`

**Mejoras:**
- ✅ Validación de assets antes de guardar en estado
- ✅ Logging de información del archivo en desarrollo
- ✅ Simplificación del código (eliminada construcción manual)
- ✅ Mejor manejo de errores al seleccionar archivos

---

## 🧪 Cómo Probar

### Pre-requisitos
1. Backend corriendo con perfil "lan":
   ```bash
   cd HuellitasSV.API/HuellitasSV.API
   dotnet run --launch-profile lan
   ```

2. Verificar que el backend esté accesible:
   - Abrir navegador: `http://<TU_IP_LOCAL>:5299/swagger`
   - Ejemplo: `http://192.168.1.68:5299/swagger`

3. App mobile corriendo:
   ```bash
   cd mobile
   npm start
   ```

### Casos de Prueba

#### Test 1: Registro de Mascota con Imagen
**Pasos:**
1. Iniciar sesión como Refugio
2. Ir a "Nueva mascota"
3. Llenar todos los campos obligatorios
4. Tocar "Elegir foto"
5. Seleccionar una imagen de la galería
6. Verificar en consola (solo desarrollo):
   ```
   [MascotaForm] Foto seleccionada: {
     valid: true,
     fileName: "...",
     mimeType: "image/jpeg",
     fileSizeFormatted: "2.3 MB"
   }
   ```
7. Tocar "Registrar mascota"
8. Verificar que se registre exitosamente

**Resultado esperado:**
- ✅ La imagen se sube correctamente
- ✅ La mascota aparece en el listado
- ✅ La imagen se muestra en el detalle

**Si falla:**
- Revisar logs en consola de Metro
- Verificar que el backend esté en red local
- Verificar que la imagen sea menor a 10MB

#### Test 2: Actualizar Mascota con Nueva Imagen
**Pasos:**
1. Iniciar sesión como Refugio
2. Seleccionar una mascota existente
3. Tocar "Editar"
4. Tocar "Cambiar foto"
5. Seleccionar una imagen diferente
6. Tocar "Guardar cambios"

**Resultado esperado:**
- ✅ La imagen se actualiza correctamente
- ✅ La nueva imagen se muestra en el detalle

#### Test 3: Crear Reporte con Foto
**Pasos:**
1. Iniciar sesión como Usuario
2. Ir a "Reportar animal"
3. Llenar descripción
4. Tocar "Marcar ubicación"
5. Tocar "Adjuntar foto"
6. Seleccionar una imagen
7. Verificar en consola:
   ```
   [ReporteForm] Foto seleccionada: {...}
   ```
8. Tocar "Enviar reporte"

**Resultado esperado:**
- ✅ El reporte se crea exitosamente
- ✅ La foto se sube correctamente
- ✅ Aparece en "Mis reportes"

#### Test 4: Registro de Refugio con Documento
**Pasos:**
1. En pantalla de Login, ir a "Crear cuenta"
2. Seleccionar "Refugio"
3. Llenar todos los campos
4. Tocar "Subir acta o permiso"
5. Seleccionar un PDF o imagen
6. Verificar en consola:
   ```
   [RegisterScreen] Documento seleccionado: {...}
   ```
7. Tocar "Enviar solicitud de registro"

**Resultado esperado:**
- ✅ El documento se sube correctamente
- ✅ El refugio queda en estado "pendiente"

#### Test 5: Diferentes Formatos de Imagen
**Probar con:**
- ✅ JPG
- ✅ PNG
- ✅ WEBP (si el dispositivo lo soporta)

**Para cada formato:**
1. Seguir Test 1
2. Verificar que el MIME type se detecte correctamente en consola

#### Test 6: Diferentes Tamaños de Archivo
**Probar con:**
- ✅ Imagen pequeña (< 100KB)
- ✅ Imagen mediana (1-3MB)
- ✅ Imagen grande (5-9MB)
- ❌ Imagen muy grande (> 10MB) - Debe fallar con error descriptivo

#### Test 7: Casos Edge
**Probar:**
1. Cancelar el selector de imágenes
   - Resultado: No debe causar error
2. Seleccionar imagen y luego cancelar el formulario
   - Resultado: La imagen no se debe subir
3. Editar mascota sin cambiar la imagen
   - Resultado: La imagen original se mantiene

---

## 🐛 Debugging

### Habilitar Logs Detallados
Los logs solo aparecen en modo desarrollo (`__DEV__ === true`).

**Qué se loguea:**
1. **En fileHelpers.js:**
   - Asset original completo
   - Archivo construido para FormData
   
2. **En api.js:**
   - URL del endpoint
   - Si es FormData
   - Detalles del error si falla

3. **En Screens:**
   - Información del archivo seleccionado
   - Tamaño formateado
   - MIME type detectado

### Inspeccionar Request en Red
**Opción 1: Chrome DevTools (Remote Debugging)**
1. Ejecutar `npx react-native log-android` o `npx react-native log-ios`
2. Buscar en consola: `[API]` o `[fileHelpers]`

**Opción 2: Reactotron (Recomendado)**
1. Instalar: `npm install --save-dev reactotron-react-native`
2. Configurar en `App.js`
3. Ver requests en Reactotron app

**Opción 3: Backend Logs**
1. Ver consola del backend
2. Buscar logs de `ArchivoService`
3. Verificar tamaño y tipo de archivo recibido

### Errores Comunes y Soluciones

#### Error: "No se pudo conectar con la API"
**Causa:** Backend no accesible
**Solución:**
1. Verificar que el backend esté corriendo con perfil "lan"
2. Verificar IP en `mobile/src/config/env.js`
3. Ping a la IP del backend desde el celular
4. Verificar que estén en la misma red Wi-Fi

#### Error: "Error al enviar archivo. Verifica que el archivo sea válido y menor a 10MB"
**Causa:** Problema con el archivo o tamaño excedido
**Solución:**
1. Verificar tamaño del archivo en logs
2. Reducir calidad en ImagePicker (quality: 0.5)
3. Probar con otra imagen

#### Error: "El archivo seleccionado no es válido"
**Causa:** Asset sin URI o corrupto
**Solución:**
1. Verificar permisos de galería
2. Probar con otra imagen
3. Revisar logs de `isValidAsset()`

#### Warning: "buildFormDataFile: asset inválido o sin URI"
**Causa:** Se pasó null o asset sin URI
**Solución:**
1. Verificar que ImagePicker retornó assets
2. Verificar que no se canceló la selección

---

## 📊 Checklist de Verificación

Antes de considerar completada la FASE 1, verificar:

- [ ] Registro de mascota con imagen funciona
- [ ] Actualización de mascota con nueva imagen funciona
- [ ] Actualización de mascota sin cambiar imagen funciona
- [ ] Crear reporte con foto funciona
- [ ] Registro de refugio con documento funciona
- [ ] Diferentes formatos de imagen funcionan (JPG, PNG)
- [ ] Archivos grandes (cerca de 10MB) funcionan
- [ ] Archivos muy grandes (> 10MB) fallan con error descriptivo
- [ ] Cancelar selector no causa errores
- [ ] Logs aparecen correctamente en desarrollo
- [ ] Mensajes de error son descriptivos
- [ ] No hay warnings en consola

---

## 🔍 Análisis de Rendimiento

### Antes (Código Original)
- ❌ Construcción manual de objeto (3-5 líneas por uso)
- ❌ Código duplicado en 4 lugares
- ❌ Sin validación de asset
- ❌ MIME type hardcodeado
- ❌ Errores genéricos

### Después (Con fileHelpers)
- ✅ Construcción centralizada (1 función)
- ✅ Código reutilizable
- ✅ Validación automática
- ✅ MIME type detectado por extensión
- ✅ Errores descriptivos
- ✅ Logs de debugging
- ✅ Mantenible y testeable

---

## 📝 Notas Adicionales

### Compatibilidad
- ✅ Expo SDK 57
- ✅ React Native 0.86.3
- ✅ expo-image-picker v57
- ✅ expo-document-picker v57
- ✅ Android e iOS

### Limitaciones Conocidas
- Tamaño máximo: 10MB (configurable en backend)
- Formatos soportados: JPG, PNG, WEBP, GIF, PDF
- Requiere permisos de galería/cámara

### Próximos Pasos (FASE 2)
Una vez verificado que todo funciona:
1. Refactorizar servicios en módulos separados
2. Implementar retry logic para uploads
3. Agregar progress bar para uploads grandes
4. Implementar compresión de imágenes automática
5. Agregar tests unitarios

---

## 🆘 Soporte

Si encuentras problemas:
1. Revisar logs en consola
2. Verificar checklist de verificación
3. Consultar sección de "Errores Comunes"
4. Revisar código de fileHelpers.js (está bien documentado)

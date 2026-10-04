/**
 * Utilidades para manejo de archivos en FormData.
 * Resuelve inconsistencias de expo-image-picker y expo-document-picker.
 */

import * as ImageManipulator from 'expo-image-manipulator';

/**
 * Extrae el tipo MIME del URI del archivo.
 * Fallback a tipos comunes basados en extensión si no está disponible.
 * 
 * @param {string} uri - URI del archivo
 * @param {string|undefined} mimeType - MIME type proporcionado por el picker
 * @returns {string} - MIME type válido
 */
function extractMimeType(uri, mimeType) {
  // Si ya viene un mimeType válido, usarlo
  if (mimeType && mimeType.includes('/')) {
    return mimeType;
  }

  // Extraer extensión del URI
  const extension = uri.split('.').pop()?.toLowerCase();
  
  // Mapeo de extensiones a MIME types
  const mimeMap = {
    // Imágenes
    'jpg': 'image/jpeg',
    'jpeg': 'image/jpeg',
    'png': 'image/png',
    'gif': 'image/gif',
    'webp': 'image/webp',
    'bmp': 'image/bmp',
    'svg': 'image/svg+xml',
    
    // Documentos
    'pdf': 'application/pdf',
    'doc': 'application/msword',
    'docx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'xls': 'application/vnd.ms-excel',
    'xlsx': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    
    // Otros
    'txt': 'text/plain',
    'json': 'application/json',
  };

  return mimeMap[extension] || 'application/octet-stream';
}

/**
 * Extrae un nombre de archivo válido del asset.
 * 
 * @param {object} asset - Asset del picker (ImagePicker o DocumentPicker)
 * @param {string} defaultName - Nombre por defecto si no se puede extraer
 * @returns {string} - Nombre de archivo válido
 */
function extractFileName(asset, defaultName = 'file.jpg') {
  // Intentar obtener el nombre del asset
  if (asset.fileName && asset.fileName.trim()) {
    // Si fileName contiene path completo, extraer solo el nombre
    return asset.fileName.split('/').pop();
  }
  
  if (asset.name && asset.name.trim()) {
    return asset.name.split('/').pop();
  }

  // Si el URI tiene una extensión clara, generar nombre con esa extensión
  if (asset.uri) {
    const uriParts = asset.uri.split('.');
    if (uriParts.length > 1) {
      const extension = uriParts.pop().toLowerCase();
      // Validar que la extensión sea razonable (máximo 5 caracteres)
      if (extension && extension.length <= 5) {
        const timestamp = Date.now();
        return `file_${timestamp}.${extension}`;
      }
    }
  }

  // Fallback al nombre por defecto
  return defaultName;
}

/**
 * Indica si Android/iOS pueden leer ese URI como archivo para subirlo.
 *
 * El modulo nativo de React Native solo abre file://, content:// y asset://.
 * Cualquier otra cosa (por ejemplo el SharedRef que devuelve expo-image-manipulator
 * en el SDK 57) hace que fetch reviente con
 * "Unsupported FormDataPart implementation".
 *
 * @param {*} uri
 * @returns {boolean}
 */
export function esUriDeArchivoLegible(uri) {
  return typeof uri === 'string' && /^(file|content|asset):\/\//i.test(uri.trim());
}

/**
 * Construye un objeto compatible con FormData para React Native.
 * Resuelve el problema de propiedades inconsistentes de expo-image-picker v57.
 * 
 * @param {object} asset - Asset retornado por ImagePicker o DocumentPicker
 * @param {object} asset.uri - URI del archivo (REQUERIDO)
 * @param {string} [asset.fileName] - Nombre del archivo (puede ser null)
 * @param {string} [asset.name] - Nombre alternativo del archivo
 * @param {string} [asset.mimeType] - Tipo MIME (puede ser undefined)
 * @param {string} [asset.type] - Tipo de asset (NO es MIME type)
 * @param {string} defaultFileName - Nombre de archivo por defecto
 * 
 * @returns {object|null} - Objeto compatible con FormData o null si el asset es inválido
 * 
 * @example
 * const asset = await ImagePicker.launchImageLibraryAsync(...);
 * const file = buildFormDataFile(asset.assets[0], 'mascota.jpg');
 * formData.append('imagen', file);
 */
export function buildFormDataFile(asset, defaultFileName = 'file.jpg') {
  // Validación: el asset debe existir y tener un URI que la plataforma sepa abrir
  if (!asset || !asset.uri) {
    console.warn('[fileHelpers] buildFormDataFile: asset inválido o sin URI', asset);
    return null;
  }

  if (!esUriDeArchivoLegible(asset.uri)) {
    console.warn(
      '[fileHelpers] buildFormDataFile: el URI no es legible por el sistema de archivos, ' +
        'se descarta el archivo',
      asset.uri,
    );
    return null;
  }

  try {
    // Extraer información del asset
    const uri = asset.uri.trim();
    const mimeType = extractMimeType(uri, asset.mimeType);
    const name = extractFileName(asset, defaultFileName);

    // React Native exige que las tres claves sean strings no vacias; si alguna
    // llega como undefined, la parte del multipart se descarta y fetch falla con
    // "Unsupported FormDataPart implementation".
    const file = {
      uri,
      type: String(mimeType || 'application/octet-stream'),
      name: String(name || defaultFileName),
    };

    // Log para debugging (solo en desarrollo)
    if (__DEV__) {
      console.log('[fileHelpers] Archivo construido:', {
        original: {
          uri: asset.uri,
          fileName: asset.fileName,
          name: asset.name,
          mimeType: asset.mimeType,
          type: asset.type,
        },
        resultado: file,
      });
    }

    return file;
  } catch (error) {
    console.error('[fileHelpers] Error al construir archivo FormData:', error);
    return null;
  }
}

/**
 * Reduce el peso de una imagen antes de subirla: la reescala si es más ancha que
 * maxWidth y la recomprime en JPEG.
 *
 * Subir una foto de varios MB por la red (sobre todo a través de ngrok) hace que
 * la petición multipart se corte y el fetch falle con "Network request failed".
 * Con esto el archivo pesa mucho menos y la subida termina.
 *
 * Si algo falla, devuelve el asset original para no bloquear la subida.
 *
 * @param {object} asset - Asset de ImagePicker (con uri/width/height).
 * @param {object} [options]
 * @param {number} [options.maxWidth] - Ancho máximo al que reescalar.
 * @param {number} [options.compress] - Calidad JPEG (0 a 1).
 * @returns {Promise<object>} asset listo para FormData ({ uri, type, name }).
 */
export async function prepareImageForUpload(asset, { maxWidth = 1280, compress = 0.6 } = {}) {
  if (!asset?.uri) return asset;

  try {
    const actions = [];
    if (asset.width && asset.width > maxWidth) {
      actions.push({ resize: { width: maxWidth } });
    }

    const result = await ImageManipulator.manipulateAsync(asset.uri, actions, {
      compress,
      format: ImageManipulator.SaveFormat.JPEG,
    });

    // En el SDK 57 el manipulador trabaja con SharedRef y puede devolver un URI
    // que React Native no sabe abrir. Si no es un archivo real, se sube el
    // asset original del picker: pesa mas, pero la subida funciona.
    if (!esUriDeArchivoLegible(result?.uri)) {
      console.warn(
        '[fileHelpers] El manipulador devolvio un URI no legible, se usa la original',
        result?.uri,
      );
      return asset;
    }

    return {
      uri: result.uri,
      width: result.width,
      height: result.height,
      fileName: 'upload.jpg',
      mimeType: 'image/jpeg',
    };
  } catch (error) {
    console.warn('[fileHelpers] No se pudo optimizar la imagen, se sube la original', error);
    return asset;
  }
}

/**
 * Valida que un asset tenga la estructura mínima requerida.
 * 
 * @param {object} asset - Asset a validar
 * @returns {boolean} - true si es válido, false si no
 */
export function isValidAsset(asset) {
  return !!(asset && asset.uri && typeof asset.uri === 'string');
}

/**
 * Obtiene información legible del asset para mostrar al usuario.
 * 
 * @param {object} asset - Asset del picker
 * @returns {object} - Información formateada
 */
export function getAssetInfo(asset) {
  if (!isValidAsset(asset)) {
    return {
      valid: false,
      message: 'Archivo inválido',
    };
  }

  const fileName = extractFileName(asset, 'archivo');
  const mimeType = extractMimeType(asset.uri, asset.mimeType);
  const fileSize = asset.fileSize || asset.size;

  return {
    valid: true,
    fileName,
    mimeType,
    fileSize,
    fileSizeFormatted: fileSize ? formatBytes(fileSize) : 'Desconocido',
  };
}

/**
 * Formatea bytes a formato legible (KB, MB, etc.)
 * 
 * @param {number} bytes - Tamaño en bytes
 * @param {number} decimals - Decimales a mostrar
 * @returns {string} - Tamaño formateado
 */
function formatBytes(bytes, decimals = 2) {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
}

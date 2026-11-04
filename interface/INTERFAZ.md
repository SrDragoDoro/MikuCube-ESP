# Guía de Interfaz y Localización — MikuCube-ESP

Este directorio contiene las definiciones de traducción al español para la interfaz de usuario de **MikuCube**, manteniendo compatibilidad total con la lógica del dispositivo, el protocolo Bluetooth y el firmware original.

---

## Estructura de Archivos

- **[`es.json`](es.json)**: Diccionario i18n estructurado para React Native / Expo.
- **[`strings_es.xml`](strings_es.xml)**: Recursos de cadenas en formato estándar de Android (`res/values-es/strings.xml`) para su uso con Android Studio o reconstrucción mediante herramientas de ingeniería inversa (como Apktool).

---

## Mapeo de Pantallas y Elementos de Interfaz

### 1. Búsqueda y Conexión (`Scan` & `Connect`)

| Elemento Original (EN) | Traducción al Español | Función / Contexto técnico |
|---|---|---|
| **Find your Miku Cube** | Encuentra tu Miku Cube | Encabezado principal de la pantalla de búsqueda |
| **Scan** | Escanear | Inicia el escaneo BLE de periféricos cercanos |
| **LIKELY YOUR CUBE** | PROBABLEMENTE TU CUBO | Etiqueta de heurística para el dispositivo filtrado por nombre/UUID |
| **Show all Bluetooth devices** | Mostrar todos los dispositivos Bluetooth | Despliega todos los anuncios BLE sin filtrar |
| **Connect / Disconnect** | Conectar / Desconectar | Gestión del estado del canal GATT Bluetooth |
| **Add to Bluetooth** | Añadir a Bluetooth | Vincula los auriculares como dispositivo de audio del sistema Android |

> [!IMPORTANT]
> **Compatibilidad Bluetooth:** La detección requiere que la tapa trasera del cubo esté abierta. El protocolo de enlace BLE no se ve afectado por el idioma seleccionado en la interfaz.

---

### 2. Estudio de Caras (`Face Studio`)

| Elemento Original (EN) | Traducción al Español | Notas de Compatibilidad |
|---|---|---|
| **Face Studio** | Estudio de caras | Pantalla principal de control visual |
| **Upload Custom Image** | Subir imagen personalizada | Carga, recorta y convierte imagen al formato del cubo |
| **Playback Count (1–1000 / 0)** | Repeticiones (1–1000 / 0) | `0` = reproducción continua e infinita |
| **Breakfast** | Desayuno | Expresión matutina predeterminada |
| **Brush Teeth** | Cepillarse los dientes | Animación de higiene bucal |
| **Studying** | Estudiando | Animación de lectura |
| **Grinding** | Trabajando duro | Animación de trabajo intensivo |
| **Milk Tea** | Té de burbujas | Animación bebiendo té |
| **Slacking Off** | Holgazaneando (Pez) | Corregido en v1.0.1 (ilustración del pez) |
| **Sleepy** | Somnolienta | Animación de bostezo |
| **Eating Rice** | Comiendo arroz | Animación comiendo bol de arroz |
| **Hungry** | Hambrienta | Animación pidiendo comida |
| **Happy** | Feliz | Animación de celebración |
| **Popcorn** | Palomitas | Animación con palomitas de maíz |
| **Fell Asleep** | Dormida | Animación durmiendo plácidamente |
| **Fully Charged** | Carga completa (Gafas de sol) | Corregido en v1.0.1 (gafas de sol) |

---

### 3. Selección de Animaciones (`Animation Selection`)

| Elemento Original (EN) | Traducción al Español | Notas de Compatibilidad |
|---|---|---|
| **Animation Selection** | Selección de animaciones | Configuración del carrusel por horario |
| **Dawn (06:00–08:00)** | Amanecer (06:00–08:00) | Franja horaria 1 |
| **Morning (08:00–12:00)** | Mañana (08:00–12:00) | Franja horaria 2 |
| **Noon (12:00–14:00)** | Mediodía (12:00–14:00) | Franja horaria 3 |
| **Afternoon (14:00–18:00)** | Tarde (14:00–18:00) | Franja horaria 4 |
| **Evening (18:00–20:00)** | Anochecer (18:00–20:00) | Franja horaria 5 |
| **Night (20:00–22:00)** | Noche (20:00–22:00) | Franja horaria 6 |
| **Restore default times** | Restaurar horas predeterminadas | Repara campos numéricos ausentes sin borrar selecciones |
| **Sync from device** | Sincronizar desde dispositivo | Lee la tabla de temporizadores actual del firmware |

---

### 4. Alarma de Despertar (`Wake Alarm`)

| Elemento Original (EN) | Traducción al Español | Notas de Compatibilidad |
|---|---|---|
| **Wake Alarm** | Alarma de despertar | Alarmas sincronizadas con el hardware |
| **Use next alarm from Clock** | Usar próxima alarma del Reloj | Lee la próxima alarma de la app Reloj de Android |
| **Repeat / Every day** | Repetir / Todos los días | Días de la semana (L, M, X, J, V, S, D) |
| **Danmaku phrase** | Frase de Danmaku | Texto que se desplaza en pantalla durante la alarma |

> [!NOTE]
> **Compatibilidad de Zona Horaria:** El firmware del cubo lee las alarmas internamente en hora de China (CST, UTC+8). La aplicación calcula y aplica automáticamente el desfase horario local para garantizar que la alarma suene a la hora local configurada.

---

### 5. Mensajes en Pantalla (`Danmaku`)

| Elemento Original (EN) | Traducción al Español | Notas de Compatibilidad |
|---|---|---|
| **Scroll lyrics while music plays** | Desplazar letras mientras suena música | Solo compatible con reproductores multimedia específicos |
| **Use large text danmaku** | Usar texto danmaku grande | Modificador de tamaño de fuente del display |
| **Remember to drink water** | Recuerda beber agua | Frase predefinida de salud |
| **Sleep early** | Duérmete temprano | Frase predefinida nocturna |
| **Turn off lights and lock up** | Apaga las luces y cierra con llave | Frase predefinida de salida |

---

### 6. Ajustes del Sistema y Diagnóstico (`System Settings` & `Settings`)

| Elemento Original (EN) | Traducción al Español | Notas de Compatibilidad |
|---|---|---|
| **System Version** | Versión del sistema | Lectura directa del firmware (ej. V6.4.12) |
| **Active theme** | Tema activo | Firmware en ejecución |
| **Switch** | Cambiar | Flashea imagen de firmware (~2 MB) por BLE |
| **Cube clock** | Reloj del cubo | Sincronización horaria mediante los auriculares |
| **Copy device data** | Copiar datos del dispositivo | Volcado JSON de diagnóstico para reporte de incidencias |
| **Change / Forget device** | Cambiar / Olvidar dispositivo | Elimina la vinculación en la app sin alterar el SO |

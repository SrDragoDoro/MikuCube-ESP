<!-- Pega este contenido en la descripción del Release en GitHub, y luego elimina este archivo
     (o consérvalo — ocupa muy poco y documenta lo publicado en esta versión). -->

Esta versión se enfoca principalmente en los dos puntos donde los usuarios solían tener dificultades: que el cubo no se mostraba en los ajustes de Bluetooth del teléfono y la incertidumbre de no saber si la pantalla mostraba la configuración real del cubo o una copia guardada anterior.

Instalar esta versión sobre la v1.0.0 conserva tu cubo emparejado y todas tus configuraciones.

## El emparejamiento ahora también se realiza con el teléfono

Al seleccionar tu cubo en la aplicación, se vincula automáticamente con los dispositivos Bluetooth de Android, permitiendo que los auriculares funcionen como dispositivo de audio sin tener que acudir a los ajustes del sistema por separado. Confirma el aviso de emparejamiento cuando aparezca en pantalla.

Todas las funciones de la aplicación siguen operativas sin este paso — si el emparejamiento falla o descartas el aviso, la pantalla de conexión incluye el botón **Add to Bluetooth** (**Añadir a Bluetooth**) y un acceso directo a los ajustes del sistema.

## Sin necesidad de guardar ni sincronizar manualmente

- Al acceder a **Alarms (Alarmas)**, **Animation Selection (Selección de animaciones)** o **System Settings (Ajustes del sistema)**, la app consulta la configuración directamente desde el cubo. Una indicación en la parte superior te informa si estás viendo los ajustes del dispositivo o tu copia guardada local, con un botón **Refresh (Actualizar)** por si realizaste cambios desde la app de Bilibili.
- **Danmaku guarda automáticamente** un instante después de terminar de escribir, tal como siempre lo han hecho las alarmas. El botón Save (Guardar) se ha retirado; el encabezado simplemente te informa del estado de sincronización. Las ediciones realizadas sin conexión se enviarán automáticamente en cuanto el cubo vuelva a conectarse.

## Novedades adicionales

- **Wake Alarm → Use next alarm from Clock** (**Alarma de despertar → Usar próxima alarma del Reloj**) extrae la hora directamente de la siguiente alarma programada en tu teléfono para no tener que introducirla de nuevo.
- **Mantén pulsada tu imagen personalizada** en Face Studio para removerla de la vista previa de la app.
- La pantalla de escaneo ahora advierte claramente que el cubo debe estar **encendido y con la tapa trasera abierta**, la causa predominante por la que el escaneo no arroja resultados.

## Correcciones

- **Dos caras mostraban la ilustración equivocada.** *Slacking Off* (摸鱼) y *Fully Charged* (充电完毕) mostraban imágenes que pertenecían a otras caras. Ambas han sido corregidas — 摸鱼 muestra el pez y 充电完毕 muestra las gafas de sol.

## Problemas conocidos aún en investigación

- **Animation Selection (Selección de animaciones) sigue en solo lectura.** Sin cambios respecto a la v1.0.0 — el cubo parece reproducir invariablemente la primera franja (Dawn / Amanecer) en lugar de alternar por horario, por lo que la opción de guardar sigue desactivada hasta esclarecer el motivo. Puedes visualizar lo asignado y la lectura desde el cubo ahora es automática.
  **Si tu cubo es nuevo o solo has utilizado la aplicación de Bilibili, por favor [comparte los datos de tu dispositivo](../../#-can-you-help-share-your-cubes-data)** — será de gran utilidad.
- **"Scroll lyrics while music plays" ("Desplazar letras mientras suena música")** solo opera con ciertas aplicaciones de reproducción musical. Spotify y YouTube Music no lo controlan.
- Las alarmas corrigen de forma automática el error de zona horaria del firmware para sonar a la hora configurada. Efecto secundario: si se consultan desde la app oficial de Bilibili, los horarios se apreciarán desplazados.

## Instalación

Descarga el APK a continuación y procede a abrirlo. Android advertirá que la fuente es desconocida — autoriza a tu navegador a instalar aplicaciones. Play Protect puede reportar que no reconoce la aplicación; es el comportamiento ordinario para instaladores fuera de Play Store.

**Abre la parte trasera del cubo antes de iniciar el escaneo** — la transmisión Bluetooth solo opera con la tapa abierta.

Para actualizaciones automáticas, agrega este repositorio a [Obtainium](https://github.com/ImranR98/Obtainium).

## Verificación de la descarga

```
SHA-256 (APK):  <fill in from the built APK>
```

Certificado de firma (idéntico desde la v1.0.0):

```
SHA-256:  12496329e6955991105e1057954ea328198950b06e823fc8415bb30a5d627289
```

Verifícalo mediante `apksigner verify --print-certs MikuCube-v1.0.1.apk`. Si el certificado muestra alguna discrepancia, la compilación no pertenece a este proyecto.

---

Proyecto no oficial y sin vinculación con bilibili, Crypton Future Media ni con el fabricante del hardware. Su uso es bajo tu propia responsabilidad, en especial el flasheo de firmware.

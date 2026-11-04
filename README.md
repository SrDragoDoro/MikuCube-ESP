# MikuCube

Una aplicación de Android para el **Hatsune Miku CUBEFACE** — cambia caras, sube tu propia
imagen, configura alarmas, ajusta danmaku y cambia temas de firmware.

**Sin app de Bilibili. Sin cuenta. Sin inicio de sesión.** Se comunica con tu cubo directamente
por Bluetooth.

### [⬇ Descargar el APK más reciente](../../releases/latest)

<p align="center">
  <img src="screenshots/03-face-studio.png" width="30%" alt="Face Studio — elige una cara o sube la tuya propia" />
  <img src="screenshots/06-alarms.png"      width="30%" alt="Alarma de despertar" />
  <img src="screenshots/09-system.png"      width="30%" alt="Ajustes del sistema — versión de firmware y temas" />
</p>

---

## Instalación

La aplicación no está en Google Play, por lo que debes instalar el APK manualmente.

1. Descarga el archivo `.apk` desde [**Releases (Lanzamientos)**](../../releases/latest).
2. Ábrelo. Android advertirá que la fuente no es de confianza — pulsa en **Ajustes** (**Settings**) en ese
   aviso y autoriza a tu navegador (o aplicación de Archivos) a instalar aplicaciones desconocidas.
3. Instala y abre la app.

**"Play Protect no reconoce esta aplicación"** — comportamiento esperado para cualquier app instalada
fuera de Play Store. Pulsa en **Más detalles → Instalar de todas formas** (**More details → Install anyway**).

## Cómo recibir actualizaciones

No hay un actualizador automático fuera de Play Store. La opción más sencilla es:

1. Instalar [**Obtainium**](https://github.com/ImranR98/Obtainium)
2. Añadir la URL de este repositorio
3. Obtainium comprobará nuevos lanzamientos y actualizará la app automáticamente

O simplemente descarga un APK más reciente e instálalo sobre el anterior — tu cubo emparejado y
tus configuraciones se mantendrán intactos.

---

## Configuración de tu cubo

1. Enciende el cubo y mantenlo cerca de tu teléfono.
2. **Abre la tapa trasera del cubo.** Solo emite señal por Bluetooth mientras la
   tapa trasera permanece abierta — si está cerrada, el escaneo no encontrará nada. Esta es con
   diferencia la razón más común por la que un escaneo aparece vacío, por lo que la pantalla de
   escaneo también te lo recordará.
3. Abre la aplicación y pulsa **Scan** (**Escanear**). Los cubos detectados con mayor probabilidad estarán marcados como **LIKELY YOUR CUBE** (**PROBABLEMENTE TU CUBO**) —
   pulsa sobre el tuyo. (Si no aparece en la lista, pulsa *Show all Bluetooth devices* / *Mostrar todos los dispositivos Bluetooth*).
4. Confirma el aviso de emparejamiento de Android. Al seleccionar tu cubo, también se añade a los
   dispositivos Bluetooth de tu propio teléfono, de modo que los auriculares funcionen como dispositivo de audio — sin
   necesidad de emparejarlos en los ajustes del sistema primero. Todo en la aplicación funciona incluso
   sin esto, por lo que si el emparejamiento inicial no se realiza, la pantalla de conexión ofrece el botón **Add to Bluetooth** (**Añadir a Bluetooth**).
5. Pulsa **Connect** (**Conectar**).

<p align="center">
  <img src="screenshots/01-find-your-cube.png" width="32%" alt="Escaneo — tu cubo aparece resaltado" />
  <img src="screenshots/02-connect.png"        width="32%" alt="Pantalla de conexión" />
</p>

Todo funciona de manera local en el dispositivo — sin internet y sin cuentas. La aplicación recuerda tu
cubo, por lo que la próxima vez se conectará de inmediato. Para emparejar uno diferente, utiliza
**Settings → Forget device** (**Ajustes → Olvidar dispositivo**) (esto deja el cubo en la lista de Bluetooth de tu
teléfono; elimínalo allí si deseas desvincularlo por completo).

Las conexiones guardadas pueden reconectarse con la tapa trasera cerrada. La configuración inicial
siempre requiere abrir la parte trasera.

### Configuración del reloj del cubo

Conecta los **auriculares** (**earbuds**) en los ajustes de Bluetooth de Android para sincronizar el reloj del cubo
con el teléfono. En la aplicación, la ruta **System Settings → Cube clock → Open Bluetooth settings** (**Ajustes del sistema → Reloj del cubo → Abrir ajustes de Bluetooth**)
abre la página adecuada. Si el reloj muestra una hora incorrecta, comprueba la fecha, hora y zona horaria de tu teléfono,
y luego desconecta y vuelve a conectar los auriculares. Conectar únicamente la app no sincroniza el reloj mostrado en pantalla.

### ¿Por qué solicita permisos de Ubicación?

Android lo requiere para *cualquier* aplicación que realice escaneo de dispositivos Bluetooth. La app
no utiliza, no recopila ni almacena tu ubicación bajo ningún concepto.

---

## Qué puedes hacer

| Función | Descripción |
|---|---|
| **Face Studio (Estudio de caras)** | Previsualiza una cara y selecciona de 1 a 1000 reproducciones, o 0 para reproducción ilimitada; sube y recorta tu propia imagen personalizada |
| **Animation Selection (Selección de animaciones)** | Elige de 1 a 5 animaciones para cada momento del día; repara programaciones de horarios incompletas conservando tus selecciones |
| **Wake Alarm (Alarma de despertar)** | Alarmas con días de repetición y mensaje desplazable en pantalla. **Use next alarm from Clock** (**Usar próxima alarma del Reloj**) rellena la hora a partir de la siguiente alarma de tu teléfono |
| **Danmaku** | Mensajes de texto desplazables personalizados y recordatorios |
| **System Settings (Ajustes del sistema)** | Consulta la versión real del firmware y cambia de temas |

Las imágenes de las caras están integradas en la aplicación, por lo que la navegación funciona sin conexión. Solo
se requiere conexión a Internet para **cambiar temas** (**switch themes**), lo que descarga una imagen de firmware.

**Las alarmas y el danmaku se guardan automáticamente mientras editas.** En Face Studio, selecciona la cantidad y pulsa
**Confirm** (**Confirmar**) para aplicarla. Para las animaciones programadas, selecciona una franja, elige sus
animaciones y pulsa **Save** (**Guardar**).

Al abrir Alarms, Animation Selection o System Settings, la aplicación lee los datos directamente desde el cubo.
Una línea en la parte superior te indica si estás viendo la configuración actual del cubo o tu copia guardada localmente,
con un botón **Refresh** (**Actualizar**) por si has modificado parámetros desde la aplicación oficial de Bilibili.

<p align="center">
  <img src="screenshots/04-menu.png"         width="24%" alt="Menú" />
  <img src="screenshots/05-animation.png"    width="24%" alt="Selección de animaciones" />
  <img src="screenshots/07-alarm-editor.png" width="24%" alt="Editor de alarmas" />
  <img src="screenshots/08-danmaku.png"      width="24%" alt="Danmaku" />
</p>

---

## Particularidades conocidas

Limitaciones actuales y comportamientos del dispositivo:

- **Alarmas.** El cubo interpreta internamente las horas de alarma según la hora de China (CST, UTC+8), por lo que
  normalmente sonarían con horas de desfase. La aplicación compensa esto automáticamente para que las alarmas suenen
  a la hora exacta en que las programes. Efecto secundario: si revisas esas alarmas dentro de la aplicación de Bilibili,
  las horas aparecerán desplazadas. Es un comportamiento totalmente esperado.
- **Las programaciones de animación antiguas pueden requerir reparación.** Si la aplicación detecta campos de
  temporización faltantes, pulsa en **Restore default times** (**Restaurar horas predeterminadas**). Esto conservará tus selecciones
  de animaciones y restablecerá los rangos horarios predeterminados. Las programaciones normales pueden editarse sin problemas.
  Se comprobó que la franja seleccionada cambia con la hora local tras despertar el cubo; no cambió visiblemente mientras
  permanecía inactivo durante la prueba.
- **"Desplazar letras mientras suena música" ("Scroll lyrics while music plays")** solo funciona con ciertas aplicaciones
  multimedia compatibles. Spotify y YouTube Music no lo controlan.

---

## 🙏 ¿Puedes ayudar? Comparte los datos de tu cubo

Muchas gracias a todos los que compartieron volcados de datos de sus dispositivos. La comparación de dichos archivos
permitió descubrir los campos de temporización ausentes en las animaciones, y la versión v1.0.2 restableció la programación y edición.

Si tu cubo continúa comportándose de manera inusual, un volcado de diagnóstico puede ayudar a investigarlo:

1. Conecta tu cubo.
2. Ve a **Settings → Copy device data** (**Ajustes → Copiar datos del dispositivo**).
3. Revisa el texto y pégalo en una [nueva incidencia (issue)](../../issues/new) indicando tu versión de firmware, qué
   comportamiento esperabas y qué ocurrió en realidad.

La exportación es exclusivamente de solo lectura. Incluye programaciones de animaciones, nombres y mensajes de alarma,
frases de danmaku, temas, la cara en uso, el catálogo de caras y el desfase de tu zona horaria. Puedes eliminar cualquier
nombre o mensaje privado antes de publicarlo.

---

## Cambio de temas

Un "tema" en este dispositivo *es* una imagen de firmware completa, descargada y flasheada a través de Bluetooth. El proceso
toma un par de minutos y el cubo se reinicia automáticamente al finalizar — vuelve a conectarte una vez que termine de iniciar.

**Mantén el cubo con suficiente batería y no interrumpas el proceso.** El cubo conserva una copia de seguridad interna
de su firmware anterior, por lo que una falla de flasheo no es definitiva y puede reintentarse, pero siempre es aconsejable
proceder con precaución.

---

## Solución de problemas

**El cubo no aparece al escanear**
Abre la tapa trasera del cubo — solo emite por Bluetooth mientras la tapa está abierta, y esta es la causa más frecuente
por la que la lista aparece vacía. Comprueba también que esté encendido y cerca, que el Bluetooth y la Ubicación estén activados,
y prueba pulsando *Show all Bluetooth devices* (*Mostrar todos los dispositivos Bluetooth*) por si no se filtró automáticamente.

**El cubo no figura en la lista Bluetooth del teléfono / los auriculares no reproducen sonido**
Elegir el cubo en la aplicación intenta emparejarlo a nivel del sistema Android, pero el sistema puede rechazarlo o el usuario
puede descartar el diálogo sin darse cuenta. Ve a la pantalla de conexión y pulsa **Add to Bluetooth** (**Añadir a Bluetooth**),
o selecciona **Open Bluetooth settings** (**Abrir ajustes de Bluetooth**) para vincularlo manualmente.

**Se conecta, pero no realiza ninguna acción**
Fuerza la detención de la aplicación y vuelve a abrirla. Si el cubo se reinició hace poco, espera unos segundos antes de reconectar.

**Dejó de responder tras un cambio de tema**
El dispositivo se reinicia después de flashear el firmware. Aguarda a que encienda por completo y vuelve a conectarte.

**Otros problemas**
Abre una [incidencia (issue)](../../issues) detallando los pasos que seguiste, lo que ocurrió y la versión de firmware indicada en
**System Settings** (**Ajustes del sistema**).

---

## Acerca de

Software independiente y no oficial. No está afiliado, respaldado ni respaldado por bilibili, Crypton Future Media ni por
el fabricante del dispositivo. "Hatsune Miku" y los personajes asociados son marcas registradas de sus respectivos propietarios.

**Gratuito, pero no de código abierto.** Este repositorio aloja los paquetes y lanzamientos, no el código fuente original.
La app incorpora las animaciones faciales del cubo para que la selección funcione fuera de línea, y parte de la lógica de comunicación
se obtuvo analizando la aplicación oficial — ninguno de estos recursos es de mi propiedad para distribuirlos libremente. Aun así,
los reportes de errores y volcados de datos son muy apreciados en la sección de [issues (incidencias)](../../issues).

Distribuido tal cual ("as-is"), sin garantías de ninguna clase. Úsalo bajo tu propia responsabilidad — especialmente el flasheo de firmware.

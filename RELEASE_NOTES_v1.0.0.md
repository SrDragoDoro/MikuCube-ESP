<!-- Pega este contenido en la descripción del Release en GitHub, y luego elimina este archivo
     (o consérvalo — ocupa muy poco y documenta lo publicado en esta versión). -->

Primer lanzamiento público. Una aplicación de Android para Hatsune Miku CUBEFACE que funciona
**sin la aplicación de Bilibili** — sin cuenta, sin inicio de sesión y sin requerir conexión a internet para emparejar.

## Lo que funciona

- **Face Studio (Estudio de caras)** — cambia la cara del cubo o sube y recorta tu propia imagen personalizada
- **Wake Alarm (Alarma de despertar)** — alarmas con repetición por días y mensaje desplazable en pantalla
- **Danmaku** — recordatorios y mensajes de texto desplazables personalizados
- **System Settings (Ajustes del sistema)** — consulta la versión real de firmware y cambia de temas
- Las ilustraciones de las caras están integradas en la app, por lo que todo funciona sin conexión

## Problemas conocidos

- **Animation Selection (Selección de animaciones) es de solo lectura.** El cubo parece reproducir siempre la primera franja (Dawn / Amanecer) en lugar de elegir según la hora del día, y aún investigamos el motivo. La función de guardado permanece desactivada en esta sección para evitar que una escritura corrompa tus ajustes.
  **Si tu cubo es nuevo o solo has utilizado la aplicación de Bilibili, por favor [comparte los datos de tu dispositivo](../../#-can-you-help-share-your-cubes-data)** — será de enorme ayuda.
- **"Scroll lyrics while music plays" ("Desplazar letras mientras suena música")** solo funciona con ciertas aplicaciones de música compatibles. Spotify y YouTube Music no lo controlan.
- Las alarmas se corrigen automáticamente para resolver un error de zona horaria en el firmware, de modo que suenan a la hora que tú programas. Efecto secundario: si las consultas desde la app oficial de Bilibili, las horas se mostrarán desplazadas.

## Instalación

Descarga el archivo APK indicado abajo y ábrelo. Android advertirá que el origen no es de confianza — permite a tu navegador instalar aplicaciones desconocidas. Play Protect podría indicar que no reconoce la aplicación; es el comportamiento estándar para archivos instalados fuera de Play Store.

**Abre la tapa trasera del cubo antes de escanear** — solo emite señal Bluetooth mientras la tapa trasera permanece abierta. Esta es la causa más común por la que un escaneo no detecta el dispositivo.

Para recibir actualizaciones automáticas, añade este repositorio a [Obtainium](https://github.com/ImranR98/Obtainium).

## Verificación de la descarga

```
SHA-256 (APK):  c2cca359c6fe29a6371d9b8d3289b1ec46207dab2af370df271d236e53539e9d
```

Certificado de firma (se mantendrá idéntico en todos los lanzamientos futuros):

```
SHA-256:  12496329e6955991105e1057954ea328198950b06e823fc8415bb30a5d627289
```

Puedes comprobarlo con `apksigner verify --print-certs MikuCube-v1.0.0.apk`. Si el certificado llega a ser diferente, la compilación no procede de este proyecto.

---

Proyecto no oficial y sin afiliación con bilibili, Crypton Future Media ni con el fabricante del dispositivo. Úsalo bajo tu propia responsabilidad, especialmente las funciones de flasheo de firmware.

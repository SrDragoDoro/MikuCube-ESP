La programación de animaciones vuelve a estar operativa, incorporando una función de reparación para los cubos afectados.
Asimismo, ahora es posible elegir el número de repeticiones para las animaciones faciales. Muchas gracias a todos
los que facilitaron volcados de diagnóstico de sus dispositivos y colaboraron probando estas soluciones.

Instala esta versión sobre la v1.0.1 para mantener tu cubo emparejado y tus configuraciones en la app.

## Programación de animaciones

- **Animation Selection (Selección de animaciones) vuelve a ser editable.** Selecciona de 1 a 5 animaciones por intervalo horario.
- Los volcados aportados por la comunidad desvelaron campos numéricos de temporización ausentes en las programaciones generadas por versiones previas. Si tu cubo se encuentra afectado, la opción **Restore default times (Restaurar horas predeterminadas)** conserva tus animaciones seleccionadas y repara la temporización. Los intervalos horarios personalizados se reajustan a los valores de fábrica.
- El guardado convencional de selecciones preserva la temporización existente de la programación y el resto de los parámetros.
- Verificado en versión de firmware V6.4.12: el cubo eligió la franja correcta según la hora local, y conmutó a la siguiente franja al despertar tras cruzar un límite horario de prueba. No se apreció cambio visible durante el estado de reposo a lo largo de dicha prueba.

## Elección de cantidad de repeticiones en animaciones GIF

Pulsa sobre una cara en **Face Studio (Estudio de caras)** para previsualizarla y configurar el contador de reproducciones:

- **1–1000:** un número específico de reproducciones.
- **0:** reproducción continua e ilimitada.
- **Cancel (Cancelar):** mantener la cara actual sin modificaciones.

Se comprobó en el cubo físico la ejecución exacta de un ciclo de tres reproducciones. Este control aplica a Face Studio; las selecciones de animaciones programadas por horario permanecen independientes.

## Conectividad, alarmas y reloj

- Se corrigieron condiciones de carrera en los procesos de autenticación y descubrimiento de direcciones MAC del dispositivo que ocasionaban fallos intermitentes de conexión.
- Se verificó la reconexión de dispositivos previamente vinculados con la tapa trasera cerrada. La vinculación inicial sigue requiriendo tener la tapa abierta.
- Se solventó un desfase de una hora en la visualización y guardado de alarmas cuando la fecha almacenada y la fecha actual pertenecían a distintos periodos de horario de verano. La compensación de huso horario del firmware continúa activa; se verificó el disparo de alarmas en el minuto exacto configurado.
- Se incorporó la ruta **System Settings → Cube clock → Open Bluetooth settings** (**Ajustes del sistema → Reloj del cubo → Abrir ajustes de Bluetooth**). Conecta los **auriculares** (**earbuds**) en dicha pantalla para sincronizar el reloj del cubo con tu teléfono móvil. Si resulta necesario, revisa la fecha, hora y zona horaria de tu teléfono, y luego desconecta y vuelve a conectar los auriculares. La vinculación exclusiva de la aplicación no sincroniza el reloj que se muestra en la pantalla del cubo.

## Instalación

Descarga el archivo **MikuCube-v1.0.2.apk** a continuación e instálalo sobre la versión instalada actualmente.
El paquete APK es compatible con las arquitecturas arm64-v8a, armeabi-v7a, x86 y x86_64. Para recibir actualizaciones automáticas,
añade [este repositorio](https://github.com/SrDragoDoro/MikuCube-ESP) a Obtainium.

## Limitaciones conocidas

- Las alarmas configuradas aquí pueden verse con horarios desplazados en la app oficial de Bilibili debido a que esta herramienta compensa la gestión de huso horario defectuosa del firmware.
- El envío de letras musicales a danmaku está sujeto a la compatibilidad del reproductor de audio; Spotify y YouTube Music no lo accionan en el entorno de pruebas verificado.
- La validación del hardware se efectuó con el firmware V6.4.12. El comportamiento en otras versiones puede variar.

## Verificación

Se superaron satisfactoriamente las 26 pruebas automáticas, las validaciones de tipos en TypeScript y la compilación firmada de producción.
Las herramientas experimentales de sondeo de reloj, barrido de alarmas y pruebas de animación se mantienen ocultas en este paquete de lanzamiento.

SHA-256 del APK:

```text
04244ae9be66ed01b99115df018c91d291604e1d84b60fcab419343631edf1de
```

SHA-256 del certificado de firma (sin modificaciones):

```text
12496329e6955991105e1057954ea328198950b06e823fc8415bb30a5d627289
```

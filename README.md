# NotTask

Herramienta de automatización de mouse/teclado para Windows, estilo TinyTask,
con autoclicker integrado. Simula clicks usando `SendInput` a nivel de API de
Windows (la misma vía que reportan los dispositivos reales), combinado con
`SetCursorPos`, que es más confiable que mover el mouse por software cuando
la app objetivo a veces "no toma" los clicks.

## Funciones

- **Autoclicker**: click izquierdo/derecho/medio, simple o doble, en la
  posición actual del cursor o en una posición fija capturada con cuenta
  regresiva. Intervalo configurable en hs/min/seg/ms. Repetición infinita o
  por cantidad de clicks.
- **Macro (grabar/reproducir)**: graba movimientos de mouse, clicks y teclas
  con su tiempo real (como TinyTask) y los reproduce igual, con velocidad y
  cantidad de repeticiones configurables. Se puede guardar/cargar como
  archivo `.actm`.
- **Multi-posición**: lista de coordenadas capturadas con cuenta regresiva;
  hace click en cada una en secuencia, en loop.

## Hotkeys globales

| Tecla | Acción                              |
|-------|--------------------------------------|
| F6    | Iniciar/detener el autoclicker       |
| F7    | Iniciar/detener la secuencia de posiciones |
| F8    | Iniciar/detener grabación de macro   |
| F9    | Iniciar/detener reproducción de macro |

Estas cuatro teclas nunca se graban dentro de una macro porque están
reservadas para controlar la app.

## Compilar

No hace falta instalar el SDK de .NET: el script usa el compilador de
.NET Framework 4 que ya viene con Windows.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

Esto genera `AutoClicker.exe` en la misma carpeta. Se puede copiar y
ejecutar en cualquier PC con Windows 10/11 sin instalar nada más.

## Sobre apps/juegos que "no toman" los clicks

- **Causa más común: UIPI.** Si la ventana destino (juego, app, o incluso un
  ícono de la barra de tareas) corre con más privilegios que AutoClicker,
  Windows descarta silenciosamente el input simulado (User Interface
  Privilege Isolation). La app detecta si se está ejecutando como
  administrador y, si no, muestra un botón **"Reiniciar como
  administrador"** arriba de todo para solucionarlo con un click.
- Si el juego usa **anti-cheat de kernel** (Easy Anti-Cheat, BattlEye,
  Vanguard, etc.), bloquea intencionalmente los eventos de `SendInput` como
  medida anti-trampas. Esto es una protección deliberada del juego: esta
  herramienta no intenta evadirla.

## Otras funciones de la interfaz

- **Mantener encima de otras ventanas**: fija la ventana de AutoClicker
  siempre visible por encima de cualquier otra app (activado por defecto).
- La app declara soporte DPI per-monitor, para que las posiciones
  capturadas coincidan con las coordenadas reales en pantallas con
  escalado (125%, 150%, etc.) o en setups multi-monitor con distinto DPI.

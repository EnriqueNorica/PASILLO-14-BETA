# PASILLO 14 — videojuego 2D

Prototipo arcade de vista superior con perspectiva 2.5D para Unity 6. Recorre el pasillo, esquiva las mesas, reúne los útiles escolares y llega a la puerta.

## Requisitos

- Unity **6000.6.9f1**
- Universal Render Pipeline 2D e Input System incluidos en el proyecto.

## Abrir y jugar

1. Clona este repositorio:
   ```bash
   git clone https://github.com/EnriqueNorica/PASILLO-14-BETA.git
   ```
2. En Unity Hub, agrega la carpeta clonada y ábrela con la versión indicada.
3. Abre `Assets/Scenes/SampleScene.unity` y presiona **Play**.
4. Muévete con **WASD** o las **flechas**; mantén **Shift** para correr.
5. Recoge los cinco útiles y llega a la puerta del lado derecho.
6. Después de ganar, presiona **R** para reiniciar.

## Movimiento y cámara

El control conserva el plano de juego 2D, con aceleración y frenado graduales, giro hacia la dirección de marcha, carrera y cámara en perspectiva que sigue al jugador desde un ángulo elevado. La sombra y los coleccionables flotantes ayudan a dar profundidad sin añadir modelos ni descargas externas.

## Estructura

- `Assets/Scripts/`: creación del nivel, movimiento, coleccionables, salida y HUD.
- `Assets/Scenes/SampleScene.unity`: escena incluida en la configuración de compilación.
- `Packages/manifest.json` y `packages-lock.json`: dependencias y versiones del proyecto.

El escenario se construye al iniciar Play; las carpetas de caché del editor (como `Library` y `Temp`) están excluidas por `.gitignore`.

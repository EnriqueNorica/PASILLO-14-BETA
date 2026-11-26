# PASILLO 14 — videojuego 2D

Prototipo arcade de vista superior para Unity 6. El nivel se genera al iniciar la escena: recorre el pasillo, esquiva las mesas, reúne los útiles escolares y alcanza la puerta.

## Requisitos

- Unity **6000.6.9f1**
- Módulos incluidos en el proyecto: Universal Render Pipeline 2D e Input System.

## Abrir y jugar

1. Clona este repositorio:
   ```bash
   git clone https://github.com/EnriqueNorica/PASILLO-14-BETA.git
   ```
2. En Unity Hub, agrega la carpeta clonada y ábrela con la versión indicada.
3. Abre `Assets/Scenes/SampleScene.unity` y presiona **Play**.
4. Muévete con **WASD** o las **flechas**.
5. Recoge los cinco útiles escolares y llega a la puerta del lado derecho.

Al ganar, presiona **R** para reiniciar el nivel.

## Estructura

- `Assets/Scripts/`: inicialización automática, construcción del nivel, movimiento, coleccionables y salida.
- `Assets/Scenes/SampleScene.unity`: escena incluida en la configuración de compilación.
- `Packages/manifest.json` y `packages-lock.json`: dependencias y versiones del proyecto.

El escenario se construye en tiempo de ejecución; no requiere descargar recursos externos. Las carpetas de caché del editor (como `Library` y `Temp`) están excluidas por `.gitignore`.

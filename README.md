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
4. Muévete con **WASD** o las **flechas**; mantén **Shift** para correr mientras tengas estamina.
5. Vigila al vigilante: te detecta dentro de su cono de visión y no puede verte a través de muros ni mesas.
6. Recoge los cinco útiles y llega a la puerta del lado derecho.
7. Si te atrapa o ganas, presiona **R** para reiniciar.

## Movimiento y cámara

El control conserva el plano de juego 2D, con aceleración y frenado graduales, giro hacia la dirección de marcha y carrera limitada por una barra de estamina que se recupera al caminar o detenerse. La cámara en perspectiva sigue al jugador desde una esquina elevada para dar una vista isométrica inclinada sin alejar demasiado la cámara. Un vigilante persigue al jugador cuando lo detecta; su cono comprueba la línea de visión con los colisionadores sólidos del escenario y busca durante unos segundos la última posición donde lo vio.

## Estructura

- `Assets/Scripts/`: creación del nivel, movimiento y estamina, vigilante con detección, coleccionables, salida y HUD.
- `Assets/Scenes/SampleScene.unity`: escena incluida en la configuración de compilación.
- `Packages/manifest.json` y `packages-lock.json`: dependencias y versiones del proyecto.

El escenario se construye al iniciar Play; las carpetas de caché del editor (como `Library` y `Temp`) están excluidas por `.gitignore`.

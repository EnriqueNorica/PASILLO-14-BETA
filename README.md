# PASILLO 14 — videojuego 2D

Prototipo de aventura arcade con vista superior, creado para Unity 6 y la plantilla 2D URP.

## Cómo jugar

1. Abre el proyecto con Unity **6000.6.4f1**.
2. Abre `Assets/Scenes/SampleScene.unity`.
3. Presiona **Play**.
4. Muévete con **WASD** o las **flechas**.
5. Recoge los cinco útiles escolares y llega a la puerta del lado derecho.
6. Presiona **R** después de ganar para reiniciar.

El nivel se genera al iniciar la escena. No requiere paquetes adicionales a los que ya incluye el proyecto.

## Código

Los scripts del prototipo están en `Assets/Scripts`. `Pasillo14Bootstrap` inicia el juego al cargar la escena; el resto implementa el escenario, movimiento, coleccionables, salida y HUD.

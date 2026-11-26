# PASILLO 14 — videojuego 2D

Videojuego escolar de movimiento 2D con escenario 2.5D para Unity 6. Recorre los pasillos y aulas, reúne los útiles y evita a La Profesora.

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
5. Acércate a una puerta de aula y pulsa **E** para abrirla o cerrarla.
6. La Profesora te persigue si te ve; los muros y muebles sólidos bloquean su visión.
7. Recoge los cinco útiles y llega a la salida.
8. Si te atrapa o ganas, presiona **R** para reiniciar.

## Movimiento y cámara

El control conserva el plano de juego y las colisiones 2D. La cámara en perspectiva mira el escenario desde una esquina elevada, y los personajes, casilleros, pupitres, pizarrones y puertas usan volúmenes sencillos para crear una vista escolar 2.5D. El Estudiante camina a 2.8 unidades/s y corre con Shift a 4.8 unidades/s, la misma velocidad de persecución de La Profesora. La barra de estamina limita la carrera y se recupera cuando deja de correr. La Profesora tiene un cono de visión de 110°; su línea de visión se bloquea con colisionadores sólidos y busca durante unos segundos el último punto donde vio al estudiante.

## Estructura

- `Assets/Scripts/`: construcción del entorno escolar, movimiento y estamina, puertas interactivas, La Profesora, detección, coleccionables, salida y HUD.
- `Assets/Scenes/SampleScene.unity`: escena incluida en la configuración de compilación.
- `Packages/manifest.json` y `packages-lock.json`: dependencias y versiones del proyecto.

El escenario se construye al iniciar Play; las carpetas de caché del editor (como `Library` y `Temp`) están excluidas por `.gitignore`.

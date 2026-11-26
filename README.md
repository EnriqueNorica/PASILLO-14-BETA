# PASILLO 14 — videojuego escolar 2.5D

Videojuego de movimiento y colisiones 2D con cámara en perspectiva, personajes y mobiliario volumétrico. El Estudiante recoge libros en las aulas y los entrega a sus compañeros antes de que termine el tiempo, mientras evita a La Profesora.

## Requisitos

- Unity **6000.6.9f1**
- Universal Render Pipeline e Input System incluidos en el proyecto.

## Abrir y jugar

1. Clona este repositorio:
   ```bash
   git clone https://github.com/EnriqueNorica/PASILLO-14-BETA.git
   ```
2. En Unity Hub, agrega la carpeta clonada y ábrela con la versión indicada.
3. Abre `Assets/Scenes/SampleScene.unity` y presiona **Play**.
4. Usa **WASD** o las **flechas** para moverte según la cámara; mantén **Shift** para correr mientras tengas estamina.
5. Las puertas se abren al acercarse el jugador, un compañero o La Profesora y se cierran al pasar.
6. Recoge los **9 libros** y entrégaselos a Ana (2), Luis (3) y Sofía (4): acércate y pulsa **E**.
7. Las cámaras avisan a La Profesora cuando te detectan; los muros bloquean su línea de visión.
8. Vigila el puntaje y el temporizador. Si te atrapa La Profesora o se acaba el tiempo, pulsa **R** para reiniciar.

## Mecánicas

- El Estudiante camina a 2.8 unidades/s y corre a 4.8 unidades/s, la misma velocidad de persecución de La Profesora.
- La estamina limita la carrera y se recupera al dejar de correr.
- Las puertas tienen animación de giro en el sentido de paso de cada personaje.
- Tres cámaras de seguridad vigilan los pasillos y reportan la última posición del Estudiante a La Profesora. Ella comienza lejos y puede abrir las puertas automáticas al perseguir.
- La vista 2.5D combina colisiones 2D con modelos volumétricos sencillos para el escenario escolar.

## Estructura

- `Assets/Scripts/`: entorno escolar, movimiento, estamina, puertas automáticas, estudiantes NPC, cámaras, profesora, libros, puntaje, temporizador y HUD.
- `Assets/Scenes/SampleScene.unity`: escena incluida en la configuración de compilación.
- `Packages/manifest.json` y `packages-lock.json`: dependencias y versiones del proyecto.

El escenario se construye al iniciar Play; las carpetas de caché del editor (como `Library` y `Temp`) están excluidas por `.gitignore`.

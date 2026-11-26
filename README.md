# PASILLO 14 — videojuego escolar 2.5D

Videojuego de movimiento y colisiones 2D con cámara isométrica y modelos volumétricos. El Estudiante recoge libros en las aulas y los entrega a sus compañeros antes de que termine el tiempo, mientras evita a La Profesora. El escenario usa como referencia visual las diapositivas 6, 8, 10 y 14 de la presentación: paredes amarillas con zoclo verde, casilleros verdes, puertas y pupitres de madera, pisos claros y personajes animales con uniformes de colores.

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
5. Las puertas se abren al acercarse el jugador, un compañero o La Profesora y se cierran al pasar. Cada hoja está anclada a la jamba izquierda y gira hacia el lado de quien cruza.
6. Recoge los **9 libros** y entrégaselos a Ana (2), Luis (3) y Sofía (4): acércate y pulsa **E**.
7. Las cámaras avisan a La Profesora cuando te detectan; los muros bloquean su línea de visión.
8. Vigila el puntaje y el temporizador. Si te atrapa La Profesora o se acaba el tiempo, pulsa **R** para reiniciar.

## Mecánicas

- El Estudiante camina a 2.8 unidades/s y corre a 4.8 unidades/s, la misma velocidad de persecución de La Profesora.
- La estamina limita la carrera, se recupera al caminar o detenerse y se muestra con porcentaje y colores que cambian de turquesa/verde a naranja/rojo.
- Las puertas tienen una bisagra fija y un recorrido de apertura limitado; el sensor las mantiene abiertas mientras pasa una persona.
- La Profesora comienza en una zona alejada del pasillo y hace rondas aleatorias. Si ve al Estudiante o recibe el aviso de una cámara, lo persigue.
- Seis cámaras están montadas en lo alto de las paredes. Tienen un cono amplio de detección y los muros bloquean su línea de visión, de modo que se pueden esquivar manteniéndose fuera del cono.
- Cuando una pared tapa al Estudiante desde la cámara del juego, su modelo se vuelve translúcido para conservarlo visible.
- Los pasillos y las aulas tienen más espacio libre entre pupitres y rutas de acceso despejadas hacia las puertas.
- La escena combina movimiento y colisiones 2D con geometría 3D sencilla generada al iniciar Play.

## Estructura

- `Assets/Scripts/`: entorno escolar, movimiento, estamina, puertas automáticas, estudiantes NPC, cámaras, profesora, libros, puntaje, temporizador y HUD.
- `Assets/Scenes/SampleScene.unity`: escena incluida en la configuración de compilación.
- `Packages/manifest.json` y `packages-lock.json`: dependencias y versiones del proyecto.

El escenario se construye al iniciar Play; las carpetas de caché del editor (como `Library` y `Temp`) están excluidas por `.gitignore`.

using System.Collections.Generic;
using UnityEngine;

namespace Pasillo14
{
    public sealed class Pasillo14Game : MonoBehaviour
    {
        private const float RoomWidth = 18f;
        private const float RoomDepth = 17f;
        private const int RequiredBooks = 9;
        private const float GameDuration = 240f;

        private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        private readonly List<Pasillo14Student> students = new List<Pasillo14Student>();
        private int booksCollected;
        private int booksDelivered;
        private int bookInventory;
        private int score;
        private int studentsServed;
        private float timeRemaining;
        private bool securityAlarmTriggered;
        private bool hasWon;
        private bool wasCaught;
        private string message = "Recoge los libros y entrégaselos a tus compañeros.";
        private Pasillo14Player? player;
        private Pasillo14Enemy? teacher;
        private Camera? followCamera;
        private Vector3 cameraVelocity;
        private static Sprite? squareSprite;

        public static Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                    squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f), 1f);
                return squareSprite;
            }
        }

        private void Start()
        {
            BuildRoom();
        }

        private void BuildRoom()
        {
            timeRemaining = GameDuration;
            EnsureLighting();
            SetupCamera();

            CreateBlock("Piso de la escuela", Vector2.zero, new Vector2(RoomWidth, RoomDepth),
                new Color(0.67f, 0.71f, 0.69f), -10, false);
            CreateFloorTiles();
            CreateOuterWalls();
            CreateClassroomPartitions(2.75f, true);
            CreateClassroomPartitions(-2.75f, false);
            CreateClassroomDividers();
            CreateClassroomInteriors();
            CreateLockers();
            CreatePlayer(new Vector2(-7f, 5.2f));
            CreateClassroomDoors();
            CreateEnemy(new Vector2(7f, -6.7f));
            CreateStudents();
            CreateSecurityCameras();

            CreateCollectible(new Vector2(-7f, 6f), new Color(1f, 0.73f, 0.18f));
            CreateCollectible(new Vector2(-4.4f, 4.5f), new Color(0.95f, 0.35f, 0.26f));
            CreateCollectible(new Vector2(-1.1f, 5.6f), new Color(0.28f, 0.68f, 0.95f));
            CreateCollectible(new Vector2(1.2f, 4.5f), new Color(0.63f, 0.42f, 0.88f));
            CreateCollectible(new Vector2(-7f, -5.7f), new Color(0.35f, 0.76f, 0.45f));
            CreateCollectible(new Vector2(-1.1f, -5.6f), new Color(0.95f, 0.65f, 0.23f));
            CreateCollectible(new Vector2(1.2f, -4.5f), new Color(0.33f, 0.73f, 0.68f));
            CreateCollectible(new Vector2(7f, 6.1f), new Color(0.70f, 0.47f, 0.88f));
            CreateCollectible(new Vector2(7f, -5.4f), new Color(0.39f, 0.62f, 0.91f));
        }

        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = false;
            camera.fieldOfView = 48f;
            camera.transform.position = new Vector3(8f, -9f, 13f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.14f, 0.18f);
            followCamera = camera;
        }

        private void EnsureLighting()
        {
            RenderSettings.ambientLight = new Color(0.63f, 0.67f, 0.72f);
            if (FindAnyObjectByType<Light>() != null)
                return;

            var lightObject = new GameObject("Luz cenital de la escuela");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.91f, 0.78f);
            light.intensity = 1.25f;
            lightObject.transform.rotation = Quaternion.Euler(35f, -28f, 0f);
        }

        private void CreateFloorTiles()
        {
            var seamColor = new Color(0.57f, 0.62f, 0.61f);
            for (float x = -8f; x <= 8f; x += 1f)
                CreateVisualBox(transform, "Junta de piso", new Vector3(x, 0f, 0.005f),
                    new Vector3(0.025f, RoomDepth - 0.25f, 0.025f), seamColor);
            for (float y = -8f; y <= 8f; y += 1f)
                CreateVisualBox(transform, "Junta de piso", new Vector3(0f, y, 0.006f),
                    new Vector3(RoomWidth - 0.25f, 0.025f, 0.025f), seamColor);
        }

        private void CreateOuterWalls()
        {
            var wall = new Color(0.79f, 0.76f, 0.66f);
            var baseboard = new Color(0.29f, 0.37f, 0.41f);
            CreateBlock("Muro norte", new Vector2(0f, 8.35f), new Vector2(RoomWidth, 0.35f), wall, 2, true);
            CreateBlock("Muro sur", new Vector2(0f, -8.35f), new Vector2(RoomWidth, 0.35f), wall, 2, true);
            CreateBlock("Muro oeste", new Vector2(-8.85f, 0f), new Vector2(0.35f, RoomDepth), wall, 2, true);
            CreateBlock("Muro este", new Vector2(8.85f, 0f), new Vector2(0.35f, RoomDepth), wall, 2, true);

            CreateVisualBox(transform, "Zoclo norte", new Vector3(0f, 8.12f, 0.12f),
                new Vector3(RoomWidth - 0.4f, 0.08f, 0.22f), baseboard);
            CreateVisualBox(transform, "Zoclo sur", new Vector3(0f, -8.12f, 0.12f),
                new Vector3(RoomWidth - 0.4f, 0.08f, 0.22f), baseboard);
        }

        private void CreateClassroomPartitions(float y, bool northSide)
        {
            float[] doorCenters = { -6f, 0f, 6f };
            float cursor = -8.65f;
            foreach (float center in doorCenters)
            {
                float gapStart = center - 0.82f;
                if (gapStart > cursor)
                    CreateWallSegment(cursor, gapStart, y, northSide ? "Muro de aulas norte" : "Muro de aulas sur");
                cursor = center + 0.82f;
            }
            if (cursor < 8.65f)
                CreateWallSegment(cursor, 8.65f, y, northSide ? "Muro de aulas norte" : "Muro de aulas sur");

        }

        private void CreateWallSegment(float startX, float endX, float y, string name)
        {
            float width = endX - startX;
            CreateBlock(name, new Vector2((startX + endX) * 0.5f, y), new Vector2(width, 0.3f),
                new Color(0.78f, 0.75f, 0.65f), 2, true);
        }

        private void CreateClassroomDividers()
        {
            var wallColor = new Color(0.80f, 0.77f, 0.68f);
            foreach (float x in new[] { -3f, 3f })
            {
                CreateBlock("División entre aulas", new Vector2(x, 5.48f), new Vector2(0.24f, 5.9f),
                    wallColor, 2, true);
                CreateBlock("División entre aulas", new Vector2(x, -5.48f), new Vector2(0.24f, 5.9f),
                    wallColor, 2, true);
            }
        }

        private void CreateClassroomInteriors()
        {
            var floorColors = new[]
            {
                new Color(0.75f, 0.73f, 0.65f),
                new Color(0.70f, 0.74f, 0.69f),
                new Color(0.74f, 0.70f, 0.64f)
            };

            for (int i = 0; i < 3; i++)
            {
                float centerX = -6f + i * 6f;
                foreach (float centerY in new[] { -5.5f, 5.5f })
                {
                    float boardY = centerY > 0f ? centerY + 2.65f : centerY - 2.65f;
                    CreateVisualBox(transform, $"Piso del aula {i + 1}",
                        new Vector3(centerX, centerY, -0.025f), new Vector3(5.75f, 5.35f, 0.04f), floorColors[i]);
                    CreateClassroomFurniture(centerX, centerY);
                    CreateVisualBox(transform, "Pizarrón", new Vector3(centerX, boardY, 1.75f),
                        new Vector3(2.7f, 0.12f, 1.15f), new Color(0.11f, 0.29f, 0.27f));
                    CreateVisualBox(transform, "Marco de pizarrón", new Vector3(centerX, boardY - Mathf.Sign(centerY) * 0.09f, 1.75f),
                        new Vector3(2.85f, 0.08f, 1.28f), new Color(0.48f, 0.33f, 0.20f));
                    CreateVisualBox(transform, "Reloj del aula", new Vector3(centerX + 2.35f, boardY - Mathf.Sign(centerY) * 0.08f, 2.15f),
                        new Vector3(0.35f, 0.12f, 0.35f), new Color(0.93f, 0.91f, 0.79f));
                }
            }
        }

        private void CreateClassroomFurniture(float centerX, float centerY)
        {
            var wood = new Color(0.49f, 0.30f, 0.18f);
            var seat = new Color(0.22f, 0.31f, 0.38f);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    float x = centerX - 1.15f + column * 2.3f;
                    float y = centerY - 0.6f + row * 1.45f;
                    CreateSolidProp("Pupitre", new Vector2(x, y), new Vector2(0.95f, 0.58f), wood, 0.78f);
                    CreateVisualBox(transform, "Cubierta del pupitre", new Vector3(x, y, 0.83f),
                        new Vector3(1.02f, 0.64f, 0.09f), new Color(0.67f, 0.46f, 0.29f));
                    CreateSolidProp("Silla", new Vector2(x, y - 0.55f), new Vector2(0.48f, 0.42f), seat, 0.5f);
                }
            }
        }

        private void CreateLockers()
        {
            var colors = new[]
            {
                new Color(0.25f, 0.45f, 0.55f),
                new Color(0.31f, 0.51f, 0.58f),
                new Color(0.36f, 0.48f, 0.53f)
            };

            for (int side = -1; side <= 1; side += 2)
            {
                float y = side * 2.15f;
                for (int i = 0; i < 21; i++)
                {
                    float x = -8.1f + i * 0.81f;
                    if (Mathf.Abs(x + 6f) < 1.12f || Mathf.Abs(x) < 1.12f || Mathf.Abs(x - 6f) < 1.12f)
                        continue;
                    CreateLocker(new Vector2(x, y), side, colors[i % colors.Length], i + 1);
                }
            }
        }

        private void CreateLocker(Vector2 position, int side, Color color, int number)
        {
            var locker = new GameObject($"Casillero {number:00}");
            locker.transform.SetParent(transform);
            locker.transform.position = position;
            var collider = locker.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.68f, 0.55f);
            CreateVisualBox(locker.transform, "Cuerpo metálico", new Vector3(0f, 0f, 0.95f),
                new Vector3(0.68f, 0.55f, 1.9f), color);

            float frontY = side > 0 ? -0.285f : 0.285f;
            CreateVisualBox(locker.transform, "Puerta de casillero", new Vector3(0f, frontY, 0.96f),
                new Vector3(0.56f, 0.045f, 1.72f), Color.Lerp(color, Color.white, 0.16f));
            for (int vent = 0; vent < 3; vent++)
                CreateVisualBox(locker.transform, "Rejilla de ventilación",
                    new Vector3(0f, frontY * 1.12f, 1.57f - vent * 0.09f),
                    new Vector3(0.2f, 0.025f, 0.035f), new Color(0.14f, 0.24f, 0.29f));
            CreateVisualBox(locker.transform, "Manija", new Vector3(0.2f, frontY * 1.18f, 0.92f),
                new Vector3(0.06f, 0.035f, 0.2f), new Color(0.87f, 0.72f, 0.38f));
        }

        private void CreateClassroomDoors()
        {
            for (int i = 0; i < 3; i++)
            {
                float x = -6f + i * 6f;
                CreateDoor($"Aula {i + 1} norte", new Vector2(x, 2.75f));
                CreateDoor($"Aula {i + 1} sur", new Vector2(x, -2.75f));
            }
        }

        private void CreateDoor(string doorName, Vector2 position)
        {
            var door = new GameObject(doorName);
            door.transform.SetParent(transform);
            door.transform.position = new Vector3(position.x - 0.74f, position.y, 0f);
            var collider = door.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.48f, 0.24f);
            collider.offset = new Vector2(0.74f, 0f);
            var sensor = door.AddComponent<BoxCollider2D>();
            sensor.size = new Vector2(2.65f, 2.55f);
            sensor.offset = new Vector2(0.74f, 0f);
            sensor.isTrigger = true;

            var leaf = new GameObject("Hoja de puerta");
            leaf.transform.SetParent(door.transform, false);
            leaf.transform.localPosition = new Vector3(0.74f, 0f, 0f);
            CreateVisualBox(leaf.transform, "Panel de madera", new Vector3(0f, 0f, 1.15f),
                new Vector3(1.48f, 0.18f, 2.3f), new Color(0.40f, 0.23f, 0.15f));
            CreateVisualBox(leaf.transform, "Ventana de puerta", new Vector3(0f, -0.105f, 1.68f),
                new Vector3(0.54f, 0.035f, 0.54f), new Color(0.38f, 0.67f, 0.77f));
            CreateVisualBox(leaf.transform, "Manija de puerta", new Vector3(0.48f, -0.12f, 1.0f),
                new Vector3(0.07f, 0.05f, 0.22f), new Color(0.91f, 0.73f, 0.38f));

            var doorLogic = door.AddComponent<Pasillo14Door>();
            doorLogic.Initialize(collider, leaf.transform);
        }

        private GameObject CreateBlock(string objectName, Vector2 position, Vector2 size,
            Color color, int sortingOrder, bool solid)
        {
            var block = new GameObject(objectName);
            block.transform.SetParent(transform);
            block.transform.position = position;
            if (solid)
            {
                var collider = block.AddComponent<BoxCollider2D>();
                collider.size = size;
                CreateVisualBox(block.transform, "Volumen del muro", new Vector3(0f, 0f, 1.35f),
                    new Vector3(size.x, size.y, 2.7f), color);
                CreateVisualBox(block.transform, "Franja del muro", new Vector3(0f, 0f, 0.25f),
                    new Vector3(size.x, size.y + 0.03f, 0.22f), new Color(0.34f, 0.42f, 0.46f));
            }
            else
            {
                CreateVisualBox(block.transform, "Superficie del piso", new Vector3(0f, 0f, -0.12f),
                    new Vector3(size.x, size.y, 0.2f), color);
            }
            return block;
        }

        private GameObject CreateSolidProp(string objectName, Vector2 position, Vector2 footprint, Color color, float height)
        {
            var prop = new GameObject(objectName);
            prop.transform.SetParent(transform);
            prop.transform.position = position;
            var collider = prop.AddComponent<BoxCollider2D>();
            collider.size = footprint;
            CreateVisualBox(prop.transform, "Volumen", new Vector3(0f, 0f, height * 0.5f),
                new Vector3(footprint.x, footprint.y, height), color);
            return prop;
        }

        private GameObject CreateVisualBox(Transform parent, string objectName, Vector3 localPosition,
            Vector3 size, Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = size;
            var collider = cube.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = GetMaterial(color);
            return cube;
        }

        private Material GetMaterial(Color color)
        {
            if (materials.TryGetValue(color, out Material? material) && material != null)
                return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                            Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
            material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            materials.Add(color, material);
            return material;
        }

        private void CreatePlayer(Vector2 position)
        {
            var playerObject = new GameObject("El Estudiante");
            playerObject.transform.SetParent(transform);
            playerObject.transform.position = position;

            CreateVisualBox(playerObject.transform, "Sombra", new Vector3(0f, 0f, 0.04f),
                new Vector3(0.75f, 0.58f, 0.08f), new Color(0.08f, 0.10f, 0.12f));
            CreateVisualBox(playerObject.transform, "Uniforme azul", new Vector3(0f, 0f, 0.55f),
                new Vector3(0.48f, 0.38f, 0.68f), new Color(0.10f, 0.30f, 0.74f));
            CreateVisualBox(playerObject.transform, "Cabeza", new Vector3(0f, 0.01f, 1.02f),
                new Vector3(0.34f, 0.33f, 0.34f), new Color(0.88f, 0.66f, 0.48f));
            CreateVisualBox(playerObject.transform, "Cabello", new Vector3(0f, 0.01f, 1.20f),
                new Vector3(0.36f, 0.35f, 0.12f), new Color(0.20f, 0.15f, 0.12f));
            CreateVisualBox(playerObject.transform, "Rostro", new Vector3(0f, 0.19f, 1.02f),
                new Vector3(0.18f, 0.035f, 0.10f), new Color(0.19f, 0.16f, 0.14f));
            CreateVisualBox(playerObject.transform, "Pantalón", new Vector3(0f, 0f, 0.16f),
                new Vector3(0.43f, 0.34f, 0.28f), new Color(0.12f, 0.19f, 0.31f));

            var body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.55f, 0.48f);
            player = playerObject.AddComponent<Pasillo14Player>();
            player.Initialize(this, body);
        }

        private void CreateEnemy(Vector2 position)
        {
            var enemyObject = new GameObject("La Profesora");
            enemyObject.transform.SetParent(transform);
            enemyObject.transform.position = position;

            CreateVisualBox(enemyObject.transform, "Sombra de la profesora", new Vector3(0f, 0f, 0.04f),
                new Vector3(0.78f, 0.62f, 0.08f), new Color(0.08f, 0.10f, 0.12f));
            CreateVisualBox(enemyObject.transform, "Suéter rojo", new Vector3(0f, 0f, 0.58f),
                new Vector3(0.52f, 0.4f, 0.78f), new Color(0.68f, 0.14f, 0.13f));
            CreateVisualBox(enemyObject.transform, "Cuello blanco", new Vector3(0f, 0.01f, 0.99f),
                new Vector3(0.35f, 0.35f, 0.35f), new Color(0.83f, 0.66f, 0.51f));
            CreateVisualBox(enemyObject.transform, "Cabello oscuro", new Vector3(0f, 0.01f, 1.18f),
                new Vector3(0.38f, 0.37f, 0.14f), new Color(0.15f, 0.13f, 0.14f));
            CreateVisualBox(enemyObject.transform, "Lentes", new Vector3(0f, 0.20f, 1.00f),
                new Vector3(0.23f, 0.04f, 0.075f), new Color(0.12f, 0.23f, 0.28f));

            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = enemyObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.62f, 0.52f);

            teacher = enemyObject.AddComponent<Pasillo14Enemy>();
            teacher.Initialize(this, player, body);
        }

        private void CreateStudents()
        {
            CreateStudent("Compañera Ana", new Vector2(-5.5f, 7.35f), 2, new Color(0.31f, 0.58f, 0.75f));
            CreateStudent("Compañero Luis", new Vector2(0.7f, -7.35f), 3, new Color(0.37f, 0.66f, 0.46f));
            CreateStudent("Compañera Sofía", new Vector2(6.5f, 7.35f), 4, new Color(0.76f, 0.49f, 0.67f));
        }

        private void CreateStudent(string studentName, Vector2 position, int booksNeeded, Color uniformColor)
        {
            var studentObject = new GameObject(studentName);
            studentObject.transform.SetParent(transform);
            studentObject.transform.position = position;

            var visuals = new GameObject("Apariencia");
            visuals.transform.SetParent(studentObject.transform, false);
            CreateVisualBox(visuals.transform, "Sombra", new Vector3(0f, 0f, 0.04f),
                new Vector3(0.72f, 0.55f, 0.07f), new Color(0.08f, 0.10f, 0.12f));
            CreateVisualBox(visuals.transform, "Uniforme", new Vector3(0f, 0f, 0.54f),
                new Vector3(0.46f, 0.36f, 0.65f), uniformColor);
            CreateVisualBox(visuals.transform, "Cabeza", new Vector3(0f, 0.02f, 1.02f),
                new Vector3(0.33f, 0.32f, 0.34f), new Color(0.88f, 0.66f, 0.49f));
            CreateVisualBox(visuals.transform, "Cabello", new Vector3(0f, 0.02f, 1.20f),
                new Vector3(0.35f, 0.34f, 0.12f), new Color(0.22f, 0.16f, 0.13f));

            var body = studentObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = studentObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.5f, 0.44f);
            collider.isTrigger = true;

            var student = studentObject.AddComponent<Pasillo14Student>();
            student.Initialize(this, player, booksNeeded, visuals.transform);
            students.Add(student);
        }

        private void CreateSecurityCameras()
        {
            CreateSecurityCamera(new Vector2(-5.5f, 0f), Vector2.right);
            CreateSecurityCamera(Vector2.zero, Vector2.right);
            CreateSecurityCamera(new Vector2(5.5f, 0f), Vector2.left);
        }

        private void CreateSecurityCamera(Vector2 position, Vector2 direction)
        {
            var cameraObject = new GameObject("Cámara de seguridad");
            cameraObject.transform.SetParent(transform);
            cameraObject.transform.position = position;
            CreateVisualBox(cameraObject.transform, "Carcasa de cámara", new Vector3(0f, 0f, 2.55f),
                new Vector3(0.48f, 0.34f, 0.28f), new Color(0.18f, 0.21f, 0.24f));
            CreateVisualBox(cameraObject.transform, "Lente", new Vector3(direction.x * 0.25f, direction.y * 0.25f, 2.55f),
                new Vector3(0.16f, 0.16f, 0.16f), new Color(0.18f, 0.56f, 0.79f));
            var securityCamera = cameraObject.AddComponent<Pasillo14SecurityCamera>();
            securityCamera.Initialize(this, player, teacher, direction);
        }

        private void CreateCollectible(Vector2 position, Color color)
        {
            var item = new GameObject("Libro escolar");
            item.transform.SetParent(transform);
            item.transform.position = new Vector3(position.x, position.y, 0.06f);
            CreateVisualBox(item.transform, "Páginas", new Vector3(0f, 0f, 0.045f),
                new Vector3(0.38f, 0.52f, 0.09f), new Color(0.91f, 0.87f, 0.72f));
            CreateVisualBox(item.transform, "Portada", new Vector3(0f, 0f, 0.105f),
                new Vector3(0.43f, 0.58f, 0.045f), color);
            CreateVisualBox(item.transform, "Título del libro", new Vector3(0f, 0.12f, 0.132f),
                new Vector3(0.22f, 0.045f, 0.015f), new Color(0.94f, 0.87f, 0.66f));

            var collider = item.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.5f, 0.65f);
            collider.isTrigger = true;
            item.AddComponent<Pasillo14Collectible>();
        }

        private void CreateExit(Vector2 position)
        {
            var exit = new GameObject("Salida del pasillo");
            exit.transform.SetParent(transform);
            exit.transform.position = position;
            var renderer = CreateVisualBox(exit.transform, "Puerta de salida", new Vector3(0f, 0f, 1.2f),
                new Vector3(0.35f, 1.3f, 2.4f), new Color(0.24f, 0.38f, 0.29f)).GetComponent<MeshRenderer>();
            CreateVisualBox(exit.transform, "Ventana de salida", new Vector3(0.20f, 0f, 1.7f),
                new Vector3(0.08f, 0.56f, 0.48f), new Color(0.43f, 0.72f, 0.76f));
            CreateVisualBox(exit.transform, "Manija de salida", new Vector3(0.22f, -0.36f, 1.0f),
                new Vector3(0.06f, 0.08f, 0.16f), new Color(0.93f, 0.75f, 0.38f));
            if (renderer != null)
                renderer.sharedMaterial = GetMaterial(new Color(0.24f, 0.38f, 0.29f));

            var collider = exit.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 1.4f);
            collider.isTrigger = true;
            exit.AddComponent<Pasillo14Exit>();
        }

        public void CollectItem()
        {
            if (hasWon || wasCaught)
                return;
            booksCollected++;
            bookInventory++;
            score += 25;
            message = $"Libro recogido. Llevas {bookInventory} para entregar.";
        }

        public int DeliverBooksTo(Pasillo14Student student)
        {
            if (bookInventory <= 0)
            {
                message = "No tienes libros para entregar.";
                return 0;
            }

            int accepted = student.AddBooks(bookInventory);
            if (accepted <= 0)
            {
                message = $"{student.DisplayName} ya recibió todos sus libros.";
                return 0;
            }

            bookInventory -= accepted;
            booksDelivered += accepted;
            score += accepted * 100;
            message = $"Entregaste {accepted} libro(s) a {student.DisplayName}.";
            return accepted;
        }

        public void MarkStudentServed(Pasillo14Student student)
        {
            studentsServed++;
            message = $"¡{student.DisplayName} ya tiene sus libros! ({studentsServed}/{students.Count})";
            if (studentsServed < students.Count)
                return;

            hasWon = true;
            score += Mathf.CeilToInt(timeRemaining) * 2;
            message = $"¡Todos recibieron sus libros! Puntaje final: {score}. Presiona R para jugar otra vez.";
            player?.Stop();
        }

        public string GetStudentDisplayName(Pasillo14Student student) => student.gameObject.name;

        public Vector2 GetPursuitWaypoint(Vector2 pursuerPosition, Vector2 targetPosition)
        {
            float[] classroomDoorX = { -6f, 0f, 6f };
            bool pursuerInClassroom = Mathf.Abs(pursuerPosition.y) > 3.05f;
            bool targetInClassroom = Mathf.Abs(targetPosition.y) > 3.05f;

            if (pursuerInClassroom)
            {
                bool sameClassroomSide = targetInClassroom && Mathf.Sign(pursuerPosition.y) == Mathf.Sign(targetPosition.y);
                int pursuerRoom = Mathf.Clamp(Mathf.FloorToInt((pursuerPosition.x + 9f) / 6f), 0, 2);
                int targetRoom = Mathf.Clamp(Mathf.FloorToInt((targetPosition.x + 9f) / 6f), 0, 2);
                if (sameClassroomSide && pursuerRoom == targetRoom)
                    return targetPosition;

                float exitDoorX = NearestDoorX(pursuerPosition.x, classroomDoorX);
                return new Vector2(exitDoorX, Mathf.Sign(pursuerPosition.y) * 2.75f);
            }

            if (targetInClassroom)
            {
                float entryDoorX = NearestDoorX(targetPosition.x, classroomDoorX);
                return new Vector2(entryDoorX, Mathf.Sign(targetPosition.y) * 2.75f);
            }

            return targetPosition;
        }

        private static float NearestDoorX(float x, float[] doors)
        {
            float nearest = doors[0];
            for (int i = 1; i < doors.Length; i++)
                if (Mathf.Abs(x - doors[i]) < Mathf.Abs(x - nearest))
                    nearest = doors[i];
            return nearest;
        }

        public void TryExit()
        {
            if (studentsServed < students.Count)
            {
                message = "Entrega los libros pendientes a tus compañeros.";
                return;
            }

            hasWon = true;
            message = "¡Llegaste a la salida! Presiona R para jugar otra vez.";
            player?.Stop();
        }

        public void CaughtByEnemy()
        {
            if (hasWon || wasCaught)
                return;
            wasCaught = true;
            message = "¡La Profesora te atrapó! Presiona R para intentarlo otra vez.";
            player?.Stop();
        }

        public void NotifySecurityAlert()
        {
            if (securityAlarmTriggered || hasWon || wasCaught)
                return;
            securityAlarmTriggered = true;
            message = "¡Una cámara te detectó! La Profesora recibió el aviso.";
        }

        private void Update()
        {
            if (!hasWon && !wasCaught)
            {
                timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
                if (timeRemaining <= 0f)
                {
                    wasCaught = true;
                    message = "¡Se acabó el tiempo! Presiona R para intentarlo otra vez.";
                    player?.Stop();
                }
            }

            if ((hasWon || wasCaught) && Pasillo14Player.RestartPressed())
                RestartLevel();
        }

        private void RestartLevel()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            booksCollected = 0;
            booksDelivered = 0;
            bookInventory = 0;
            score = 0;
            studentsServed = 0;
            securityAlarmTriggered = false;
            hasWon = false;
            wasCaught = false;
            message = "Recoge los libros y entrégaselos a tus compañeros.";
            students.Clear();
            player = null;
            teacher = null;
            cameraVelocity = Vector3.zero;
            BuildRoom();
        }

        private void LateUpdate()
        {
            if (followCamera == null || player == null)
                return;

            Vector3 target = player.transform.position + new Vector3(8f, -9f, 13f);
            followCamera.transform.position = Vector3.SmoothDamp(
                followCamera.transform.position, target, ref cameraVelocity, 0.18f);
            followCamera.transform.LookAt(player.transform.position + new Vector3(0f, 0f, 0.7f), Vector3.forward);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16f, 16f, 550f, 200f), GUIContent.none);
            GUI.Label(new Rect(30f, 22f, 510f, 22f), $"PASILLO 14    Libros: {booksCollected}/{RequiredBooks} recogidos | {booksDelivered} entregados | Mochila: {bookInventory}");
            int seconds = Mathf.CeilToInt(timeRemaining);
            GUI.Label(new Rect(30f, 45f, 500f, 20f), $"Tiempo: {seconds / 60:00}:{seconds % 60:00}    Puntaje: {score}    Compañeros: {studentsServed}/{students.Count}");
            GUI.Label(new Rect(30f, 69f, 90f, 20f), "Estamina:");
            GUI.Box(new Rect(105f, 72f, 170f, 14f), GUIContent.none);
            if (player != null)
            {
                Color oldColor = GUI.color;
                GUI.color = Color.Lerp(new Color(0.86f, 0.18f, 0.13f), new Color(0.22f, 0.78f, 0.36f), player.StaminaFraction);
                GUI.Box(new Rect(108f, 75f, 164f * player.StaminaFraction, 8f), GUIContent.none);
                GUI.color = oldColor;
            }
            GUI.Label(new Rect(30f, 95f, 500f, 22f), message);
            GUI.Label(new Rect(30f, 121f, 500f, 20f), "WASD/flechas: moverte según cámara    Shift: correr");
            GUI.Label(new Rect(30f, 145f, 500f, 20f), "Recoge libros, acércate a cada compañero y pulsa E para entregarlos.");
            string deliveryStatus = "";
            for (int i = 0; i < students.Count; i++)
            {
                Pasillo14Student student = students[i];
                deliveryStatus += $"{student.gameObject.name}: {student.DeliveredBooks}/{student.RequiredBooks}";
                if (i < students.Count - 1)
                    deliveryStatus += "   ";
            }
            GUI.Label(new Rect(30f, 168f, 510f, 20f), deliveryStatus);
        }
    }
}

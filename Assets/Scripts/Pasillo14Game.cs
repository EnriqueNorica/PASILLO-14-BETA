using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pasillo14
{
    public sealed class Pasillo14Game : MonoBehaviour
    {
        private enum ScreenState { MainMenu, Settings, Playing, GameOver, Victory }
        private enum RebindAction { None, Up, Left, Down, Right, Sprint, Deliver }

        private const float RoomWidth = 24f;
        private const float RoomDepth = 23f;
        private const float HallHalfHeight = 4.2f;
        private const float RoomHalfWidth = 12f;
        private const float RoomHalfDepth = 11.5f;
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
        private bool hasWon;
        private bool wasCaught;
        private bool timeoutLoss;
        private ScreenState screenState = ScreenState.MainMenu;
        private ScreenState settingsReturnState = ScreenState.MainMenu;
        private RebindAction rebindAction;
        private bool sprintButtonToggled;
        private Key moveUpKey = Key.W;
        private Key moveLeftKey = Key.A;
        private Key moveDownKey = Key.S;
        private Key moveRightKey = Key.D;
        private Key sprintKey = Key.LeftShift;
        private Key deliverKey = Key.E;
        private float musicVolume = 0.65f;
        private float soundVolume = 0.8f;
        private string message = "Recoge los libros y entrégaselos a tus compañeros.";
        private Pasillo14Player? player;
        private Pasillo14Enemy? teacher;
        private Camera? followCamera;
        private Vector3 cameraVelocity;
        private static Sprite? squareSprite;
        private Pasillo14Audio? audioManager;
        private Texture2D? titleTexture;

        public Key MoveUpKey => moveUpKey;
        public Key MoveLeftKey => moveLeftKey;
        public Key MoveDownKey => moveDownKey;
        public Key MoveRightKey => moveRightKey;
        public Key SprintKey => sprintKey;
        public Key DeliverKey => deliverKey;
        public bool SprintButtonToggled => sprintButtonToggled;

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
            LoadPreferences();
            titleTexture = Resources.Load<Texture2D>("Pasillo14Title");
            audioManager = gameObject.AddComponent<Pasillo14Audio>();
            audioManager.SetVolumes(musicVolume, soundVolume);
            audioManager.SetTheme(Pasillo14Audio.Theme.Menu);
        }

        private void BuildRoom()
        {
            timeRemaining = GameDuration;
            EnsureLighting();
            SetupCamera();

            CreateBlock("Piso de la escuela", Vector2.zero, new Vector2(RoomWidth, RoomDepth),
                new Color(0.72f, 0.78f, 0.57f), -10, false);
            CreateFloorTiles();
            CreateOuterWalls();
            CreateClassroomPartitions(HallHalfHeight, true);
            CreateClassroomPartitions(-HallHalfHeight, false);
            CreateClassroomDividers();
            CreateClassroomInteriors();
            CreateLockers();
            CreateSchoolDetails();
            CreatePlayer(new Vector2(-10f, 8.5f));
            CreateClassroomDoors();
            CreateEnemy(GetDistantHallwaySpawn(new Vector2(-10f, 8.5f)));
            CreateStudents();
            CreateCollectible(new Vector2(-10f, 8f), new Color(0.94f, 0.36f, 0.18f));
            CreateCollectible(new Vector2(-5.5f, 6.1f), new Color(0.08f, 0.42f, 0.63f));
            CreateCollectible(new Vector2(-2f, 9f), new Color(0.81f, 0.18f, 0.26f));
            CreateCollectible(new Vector2(2f, 6.3f), new Color(0.95f, 0.68f, 0.12f));
            CreateCollectible(new Vector2(-10f, -7f), new Color(0.18f, 0.48f, 0.31f));
            CreateCollectible(new Vector2(-2f, -9f), new Color(0.11f, 0.37f, 0.68f));
            CreateCollectible(new Vector2(2f, -6.3f), new Color(0.78f, 0.24f, 0.49f));
            CreateCollectible(new Vector2(6f, 9f), new Color(0.08f, 0.48f, 0.62f));
            CreateCollectible(new Vector2(10f, -9f), new Color(0.88f, 0.44f, 0.12f));
        }

        private static Vector2 GetDistantHallwaySpawn(Vector2 playerPosition)
        {
            var candidates = new List<Vector2>
            {
                new Vector2(-9f, 0f),
                new Vector2(0f, 0f),
                new Vector2(9f, 0f)
            };
            candidates.RemoveAll(point => Vector2.Distance(point, playerPosition) < 12f);
            if (candidates.Count == 0)
                return new Vector2(9f, 0f);
            return candidates[Random.Range(0, candidates.Count)];
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
            camera.backgroundColor = new Color(0.34f, 0.54f, 0.79f);
            if (camera.GetComponent<AudioListener>() == null && FindAnyObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();
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
            var seamColor = new Color(0.59f, 0.68f, 0.48f);
            for (float x = -11f; x <= 11f; x += 1f)
                CreateVisualBox(transform, "Junta de piso", new Vector3(x, 0f, 0.005f),
                    new Vector3(0.025f, RoomDepth - 0.25f, 0.025f), seamColor);
            for (float y = -11f; y <= 11f; y += 1f)
                CreateVisualBox(transform, "Junta de piso", new Vector3(0f, y, 0.006f),
                    new Vector3(RoomWidth - 0.25f, 0.025f, 0.025f), seamColor);
        }

        private void CreateOuterWalls()
        {
            var wall = new Color(0.91f, 0.68f, 0.18f);
            var baseboard = new Color(0.16f, 0.43f, 0.28f);
            CreateBlock("Muro norte", new Vector2(0f, RoomHalfDepth - 0.15f), new Vector2(RoomWidth, 0.35f), wall, 2, true);
            CreateBlock("Muro sur", new Vector2(0f, -RoomHalfDepth + 0.15f), new Vector2(RoomWidth, 0.35f), wall, 2, true);
            CreateBlock("Muro oeste", new Vector2(-RoomHalfWidth + 0.15f, 0f), new Vector2(0.35f, RoomDepth), wall, 2, true);
            CreateBlock("Muro este", new Vector2(RoomHalfWidth - 0.15f, 0f), new Vector2(0.35f, RoomDepth), wall, 2, true);

            CreateVisualBox(transform, "Zoclo norte", new Vector3(0f, RoomHalfDepth - 0.38f, 0.12f),
                new Vector3(RoomWidth - 0.4f, 0.08f, 0.22f), baseboard);
            CreateVisualBox(transform, "Zoclo sur", new Vector3(0f, -RoomHalfDepth + 0.38f, 0.12f),
                new Vector3(RoomWidth - 0.4f, 0.08f, 0.22f), baseboard);
        }

        private void CreateClassroomPartitions(float y, bool northSide)
        {
            float[] doorCenters = { -8f, 0f, 8f };
            float cursor = -RoomHalfWidth + 0.35f;
            foreach (float center in doorCenters)
            {
                float gapStart = center - 0.82f;
                if (gapStart > cursor)
                    CreateWallSegment(cursor, gapStart, y, northSide ? "Muro de aulas norte" : "Muro de aulas sur");
                cursor = center + 0.82f;
            }
            if (cursor < RoomHalfWidth - 0.35f)
                CreateWallSegment(cursor, RoomHalfWidth - 0.35f, y, northSide ? "Muro de aulas norte" : "Muro de aulas sur");

        }

        private void CreateWallSegment(float startX, float endX, float y, string name)
        {
            float width = endX - startX;
            CreateBlock(name, new Vector2((startX + endX) * 0.5f, y), new Vector2(width, 0.3f),
                new Color(0.78f, 0.75f, 0.65f), 2, true);
        }

        private void CreateClassroomDividers()
        {
            var wallColor = new Color(0.91f, 0.68f, 0.18f);
            foreach (float x in new[] { -4f, 4f })
            {
                CreateBlock("División entre aulas", new Vector2(x, 7.65f), new Vector2(0.24f, 7.0f),
                    wallColor, 2, true);
                CreateBlock("División entre aulas", new Vector2(x, -7.65f), new Vector2(0.24f, 7.0f),
                    wallColor, 2, true);
            }
        }

        private void CreateClassroomInteriors()
        {
            var floorColors = new[]
            {
                new Color(0.84f, 0.85f, 0.63f),
                new Color(0.74f, 0.83f, 0.63f),
                new Color(0.88f, 0.79f, 0.54f)
            };

            for (int i = 0; i < 3; i++)
            {
                float centerX = -8f + i * 8f;
                foreach (float centerY in new[] { -7.7f, 7.7f })
                {
                    float boardY = centerY > 0f ? centerY + 3.05f : centerY - 3.05f;
                    CreateVisualBox(transform, $"Piso del aula {i + 1}",
                        new Vector3(centerX, centerY, -0.025f), new Vector3(7.7f, 6.65f, 0.04f), floorColors[i]);
                    CreateClassroomFurniture(centerX, centerY);
                    CreateVisualBox(transform, "Pizarrón", new Vector3(centerX, boardY, 1.75f),
                        new Vector3(3.0f, 0.12f, 1.3f), new Color(0.08f, 0.32f, 0.20f));
                    CreateVisualBox(transform, "Marco de pizarrón", new Vector3(centerX, boardY - Mathf.Sign(centerY) * 0.09f, 1.75f),
                        new Vector3(3.16f, 0.08f, 1.42f), new Color(0.52f, 0.30f, 0.13f));
                    CreateVisualBox(transform, "Reloj del aula", new Vector3(centerX + 2.35f, boardY - Mathf.Sign(centerY) * 0.08f, 2.15f),
                        new Vector3(0.42f, 0.12f, 0.42f), new Color(0.97f, 0.91f, 0.66f));
                    for (int window = -1; window <= 1; window += 2)
                    {
                        float windowX = centerX + window * 3.25f;
                        CreateVisualBox(transform, "Ventana del aula", new Vector3(windowX, boardY, 1.72f),
                            new Vector3(0.82f, 0.10f, 1.15f), new Color(0.22f, 0.65f, 0.87f));
                        CreateVisualBox(transform, "Marco de ventana", new Vector3(windowX, boardY - Mathf.Sign(centerY) * 0.08f, 1.72f),
                            new Vector3(0.96f, 0.06f, 1.29f), new Color(0.95f, 0.84f, 0.56f));
                    }
                }
            }
        }

        private void CreateClassroomFurniture(float centerX, float centerY)
        {
            var wood = new Color(0.64f, 0.34f, 0.13f);
            var seat = new Color(0.12f, 0.39f, 0.53f);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    float x = centerX - 1.75f + column * 3.5f;
                    float y = centerY - 0.95f + row * 2.1f;
                    CreateSolidProp("Pupitre", new Vector2(x, y), new Vector2(0.95f, 0.58f), wood, 0.78f);
                    CreateVisualBox(transform, "Cubierta del pupitre", new Vector3(x, y, 0.83f),
                        new Vector3(1.02f, 0.64f, 0.09f), new Color(0.79f, 0.48f, 0.22f));
                    CreateSolidProp("Silla", new Vector2(x, y - 0.55f), new Vector2(0.48f, 0.42f), seat, 0.5f);
                }
            }
        }

        private void CreateSchoolDetails()
        {
            foreach (float x in new[] { -4.8f, 4.8f })
            {
                foreach (int side in new[] { -1, 1 })
                {
                    float y = side * (HallHalfHeight - 0.19f);
                    CreateVisualBox(transform, "Cartel del pasillo", new Vector3(x, y, 1.7f),
                        new Vector3(0.82f, 0.055f, 0.95f), new Color(0.13f, 0.35f, 0.72f));
                    CreateVisualBox(transform, "Ilustración del cartel", new Vector3(x, y - side * 0.035f, 1.78f),
                        new Vector3(0.35f, 0.025f, 0.38f), new Color(0.98f, 0.72f, 0.14f));
                }
            }

            for (int room = 0; room < 3; room++)
            {
                float x = -8f + room * 8f;
                CreatePottedPlant(new Vector2(x - 3.25f, 10.25f));
                CreatePottedPlant(new Vector2(x + 3.25f, -10.25f));
            }
        }

        private void CreatePottedPlant(Vector2 position)
        {
            CreateVisualBox(transform, "Maceta de terracota", new Vector3(position.x, position.y, 0.28f),
                new Vector3(0.48f, 0.48f, 0.48f), new Color(0.72f, 0.31f, 0.15f));
            CreateVisualSphere(transform, "Follaje", new Vector3(position.x, position.y, 0.78f),
                new Vector3(0.72f, 0.72f, 0.76f), new Color(0.12f, 0.48f, 0.22f));
            CreateVisualSphere(transform, "Hoja brillante", new Vector3(position.x + 0.18f, position.y - 0.08f, 0.94f),
                new Vector3(0.32f, 0.30f, 0.42f), new Color(0.25f, 0.70f, 0.26f));
        }

        private void CreateLockers()
        {
            var colors = new[]
            {
                new Color(0.04f, 0.32f, 0.17f),
                new Color(0.08f, 0.42f, 0.22f),
                new Color(0.14f, 0.50f, 0.26f)
            };

            for (int side = -1; side <= 1; side += 2)
            {
                float y = side * 3.45f;
                for (int i = 0; i < 24; i++)
                {
                    float x = -11f + i * 0.95f;
                    if (Mathf.Abs(x + 8f) < 1.35f || Mathf.Abs(x) < 1.35f || Mathf.Abs(x - 8f) < 1.35f)
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
                float x = -8f + i * 8f;
                CreateDoor($"Aula {i + 1} norte", new Vector2(x, HallHalfHeight));
                CreateDoor($"Aula {i + 1} sur", new Vector2(x, -HallHalfHeight));
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
            leaf.transform.localPosition = Vector3.zero;
            CreateVisualBox(door.transform, "Jamba izquierda", new Vector3(0f, 0f, 1.16f),
                new Vector3(0.16f, 0.30f, 2.32f), new Color(0.93f, 0.73f, 0.24f));
            CreateVisualBox(door.transform, "Jamba derecha", new Vector3(1.48f, 0f, 1.16f),
                new Vector3(0.16f, 0.30f, 2.32f), new Color(0.93f, 0.73f, 0.24f));
            CreateVisualBox(door.transform, "Dintel", new Vector3(0.74f, 0f, 2.34f),
                new Vector3(1.64f, 0.30f, 0.18f), new Color(0.93f, 0.73f, 0.24f));
            GameObject panel = CreateVisualBox(leaf.transform, "Panel de madera", new Vector3(0.74f, 0f, 1.15f),
                new Vector3(1.48f, 0.18f, 2.3f), new Color(0.53f, 0.28f, 0.14f), true);
            var panelCollider = panel.GetComponent<BoxCollider>();
            panel.AddComponent<Pasillo14Occluder>();
            CreateVisualBox(leaf.transform, "Ventana de puerta", new Vector3(0.74f, -0.105f, 1.68f),
                new Vector3(0.54f, 0.035f, 0.54f), new Color(0.38f, 0.67f, 0.77f));
            CreateVisualBox(leaf.transform, "Manija de puerta", new Vector3(1.22f, -0.12f, 1.0f),
                new Vector3(0.07f, 0.05f, 0.22f), new Color(0.91f, 0.73f, 0.38f));
            var sign = new GameObject("Placa de aula");
            sign.transform.SetParent(door.transform, false);
            sign.transform.localPosition = new Vector3(0.74f, -0.205f, 2.53f);
            sign.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var signText = sign.AddComponent<TextMesh>();
            signText.text = doorName.Replace(" norte", "").Replace(" sur", "").ToUpperInvariant();
            signText.anchor = TextAnchor.MiddleCenter;
            signText.alignment = TextAlignment.Center;
            signText.fontSize = 24;
            signText.characterSize = 0.075f;
            signText.color = new Color(0.98f, 0.91f, 0.63f);

            var doorLogic = door.AddComponent<Pasillo14Door>();
            doorLogic.Initialize(collider, panelCollider, leaf.transform);
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
                GameObject wallVisual = CreateVisualBox(block.transform, "Volumen del muro", new Vector3(0f, 0f, 1.35f),
                    new Vector3(size.x, size.y, 2.7f), color, true);
                wallVisual.AddComponent<Pasillo14Occluder>();
                CreateVisualBox(block.transform, "Franja del muro", new Vector3(0f, 0f, 0.25f),
                    new Vector3(size.x, size.y + 0.03f, 0.24f), new Color(0.16f, 0.43f, 0.28f));
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
            Vector3 size, Color color, bool keep3DCollider = false)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = size;
            var collider = cube.GetComponent<Collider>();
            if (collider != null && !keep3DCollider)
                collider.enabled = false;
            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = GetMaterial(color);
            return cube;
        }

        private GameObject CreateVisualSphere(Transform parent, string objectName, Vector3 localPosition,
            Vector3 size, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = objectName;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = size;
            var collider = sphere.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            var renderer = sphere.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = GetMaterial(color);
            return sphere;
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
            Vector2 cameraFacing = new Vector2(8f, -9f).normalized;
            playerObject.transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(cameraFacing.y, cameraFacing.x) * Mathf.Rad2Deg - 90f);

            CreateVisualBox(playerObject.transform, "Sombra", new Vector3(0f, 0f, 0.04f),
                new Vector3(0.82f, 0.62f, 0.08f), new Color(0.09f, 0.08f, 0.09f));
            CreateVisualBox(playerObject.transform, "Uniforme azul", new Vector3(0f, 0f, 0.55f),
                new Vector3(0.48f, 0.40f, 0.68f), new Color(0.10f, 0.22f, 0.52f));
            CreateVisualBox(playerObject.transform, "Camisa blanca", new Vector3(0f, 0.21f, 0.57f),
                new Vector3(0.18f, 0.04f, 0.46f), new Color(0.96f, 0.91f, 0.76f));
            CreateVisualBox(playerObject.transform, "Corbata roja", new Vector3(0f, 0.24f, 0.52f),
                new Vector3(0.085f, 0.045f, 0.30f), new Color(0.83f, 0.12f, 0.19f));
            CreateVisualBox(playerObject.transform, "Pantalón", new Vector3(0f, 0f, 0.16f),
                new Vector3(0.45f, 0.36f, 0.28f), new Color(0.08f, 0.13f, 0.30f));
            CreateVisualSphere(playerObject.transform, "Cabeza de zorro naranja", new Vector3(0f, 0f, 1.06f),
                new Vector3(0.44f, 0.42f, 0.42f), new Color(0.95f, 0.34f, 0.09f));
            for (int side = -1; side <= 1; side += 2)
            {
                var ear = CreateVisualBox(playerObject.transform, "Oreja puntiaguda", new Vector3(side * 0.15f, 0f, 1.39f),
                    new Vector3(0.15f, 0.17f, 0.34f), new Color(0.91f, 0.28f, 0.07f));
                ear.transform.localRotation = Quaternion.Euler(0f, 0f, -side * 14f);
                CreateVisualSphere(playerObject.transform, "Ojo", new Vector3(side * 0.11f, 0.185f, 1.10f),
                    new Vector3(0.085f, 0.055f, 0.105f), new Color(1f, 0.92f, 0.72f));
                CreateVisualSphere(playerObject.transform, "Pupila", new Vector3(side * 0.11f, 0.23f, 1.10f),
                    new Vector3(0.036f, 0.025f, 0.07f), new Color(0.13f, 0.10f, 0.12f));
            }
            CreateVisualSphere(playerObject.transform, "Hocico crema", new Vector3(0f, 0.20f, 0.96f),
                new Vector3(0.20f, 0.11f, 0.13f), new Color(1f, 0.77f, 0.52f));
            CreateVisualSphere(playerObject.transform, "Cola", new Vector3(-0.34f, -0.12f, 0.45f),
                new Vector3(0.24f, 0.24f, 0.48f), new Color(0.93f, 0.32f, 0.09f));

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
            CreateVisualBox(enemyObject.transform, "Traje oscuro", new Vector3(0f, 0f, 0.58f),
                new Vector3(0.54f, 0.42f, 0.78f), new Color(0.12f, 0.13f, 0.17f));
            CreateVisualBox(enemyObject.transform, "Camisa", new Vector3(0f, 0.22f, 0.63f),
                new Vector3(0.21f, 0.04f, 0.55f), new Color(0.88f, 0.86f, 0.79f));
            CreateVisualBox(enemyObject.transform, "Corbata vino", new Vector3(0f, 0.25f, 0.57f),
                new Vector3(0.09f, 0.05f, 0.36f), new Color(0.72f, 0.07f, 0.18f));
            CreateVisualSphere(enemyObject.transform, "Cabeza de gata", new Vector3(0f, 0f, 1.10f),
                new Vector3(0.46f, 0.44f, 0.43f), new Color(0.13f, 0.14f, 0.17f));
            for (int side = -1; side <= 1; side += 2)
            {
                var ear = CreateVisualBox(enemyObject.transform, "Oreja oscura", new Vector3(side * 0.16f, 0f, 1.43f),
                    new Vector3(0.16f, 0.17f, 0.34f), new Color(0.10f, 0.11f, 0.14f));
                ear.transform.localRotation = Quaternion.Euler(0f, 0f, -side * 13f);
                CreateVisualSphere(enemyObject.transform, "Ojo amarillo", new Vector3(side * 0.12f, 0.19f, 1.14f),
                    new Vector3(0.085f, 0.05f, 0.11f), new Color(1f, 0.79f, 0.10f));
                CreateVisualSphere(enemyObject.transform, "Pupila", new Vector3(side * 0.12f, 0.23f, 1.14f),
                    new Vector3(0.028f, 0.02f, 0.078f), new Color(0.08f, 0.07f, 0.08f));
            }
            CreateVisualSphere(enemyObject.transform, "Hocico", new Vector3(0f, 0.20f, 1.00f),
                new Vector3(0.18f, 0.10f, 0.12f), new Color(0.39f, 0.39f, 0.43f));
            CreateVisualSphere(enemyObject.transform, "Cola de gata", new Vector3(-0.36f, -0.1f, 0.52f),
                new Vector3(0.23f, 0.22f, 0.48f), new Color(0.12f, 0.13f, 0.16f));

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
            CreateStudent("Compañera Ana", new Vector2(-6.2f, 9.6f), 2,
                new Color(0.94f, 0.92f, 0.84f), new Color(0.75f, 0.10f, 0.18f), "coneja");
            CreateStudent("Compañero Luis", new Vector2(0.7f, -9.6f), 3,
                new Color(0.66f, 0.68f, 0.67f), new Color(0.07f, 0.37f, 0.60f), "raton");
            CreateStudent("Compañera Sofía", new Vector2(6.5f, 9.6f), 4,
                new Color(0.72f, 0.42f, 0.77f), new Color(0.80f, 0.27f, 0.44f), "gata");
        }

        private void CreateStudent(string studentName, Vector2 position, int booksNeeded,
            Color furColor, Color uniformColor, string species)
        {
            var studentObject = new GameObject(studentName);
            studentObject.transform.SetParent(transform);
            studentObject.transform.position = position;
            Vector2 facingHall = position.y > 0f ? Vector2.down : Vector2.up;
            studentObject.transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(facingHall.y, facingHall.x) * Mathf.Rad2Deg - 90f);

            var visuals = new GameObject("Apariencia");
            visuals.transform.SetParent(studentObject.transform, false);
            CreateVisualBox(visuals.transform, "Sombra", new Vector3(0f, 0f, 0.04f),
                new Vector3(0.72f, 0.55f, 0.07f), new Color(0.08f, 0.08f, 0.09f));
            CreateVisualBox(visuals.transform, "Uniforme", new Vector3(0f, 0f, 0.55f),
                new Vector3(0.47f, 0.38f, 0.68f), uniformColor);
            CreateVisualBox(visuals.transform, "Camisa", new Vector3(0f, 0.20f, 0.57f),
                new Vector3(0.18f, 0.04f, 0.43f), new Color(0.97f, 0.92f, 0.79f));
            CreateVisualBox(visuals.transform, "Corbata", new Vector3(0f, 0.23f, 0.52f),
                new Vector3(0.075f, 0.045f, 0.27f), new Color(0.85f, 0.13f, 0.18f));
            CreateVisualSphere(visuals.transform, $"Cabeza de {species}", new Vector3(0f, 0.02f, 1.04f),
                new Vector3(0.41f, 0.39f, 0.40f), furColor);

            if (species == "coneja")
            {
                for (int side = -1; side <= 1; side += 2)
                    CreateVisualSphere(visuals.transform, "Oreja larga", new Vector3(side * 0.12f, 0f, 1.43f),
                        new Vector3(0.13f, 0.13f, 0.48f), furColor);
            }
            else if (species == "raton")
            {
                for (int side = -1; side <= 1; side += 2)
                    CreateVisualSphere(visuals.transform, "Oreja redonda", new Vector3(side * 0.25f, 0f, 1.23f),
                        new Vector3(0.23f, 0.16f, 0.24f), furColor);
            }
            else
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    var ear = CreateVisualBox(visuals.transform, "Oreja puntiaguda", new Vector3(side * 0.14f, 0f, 1.35f),
                        new Vector3(0.14f, 0.14f, 0.30f), furColor);
                    ear.transform.localRotation = Quaternion.Euler(0f, 0f, -side * 12f);
                }
            }
            for (int side = -1; side <= 1; side += 2)
            {
                CreateVisualSphere(visuals.transform, "Ojo", new Vector3(side * 0.105f, 0.18f, 1.06f),
                    new Vector3(0.07f, 0.045f, 0.09f), new Color(0.98f, 0.91f, 0.76f));
                CreateVisualSphere(visuals.transform, "Pupila", new Vector3(side * 0.105f, 0.215f, 1.06f),
                    new Vector3(0.03f, 0.02f, 0.065f), new Color(0.12f, 0.10f, 0.12f));
            }
            CreateVisualSphere(visuals.transform, "Cola", new Vector3(-0.3f, -0.11f, 0.42f),
                new Vector3(0.21f, 0.21f, 0.4f), furColor);

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
            message = $"¡Todos recibieron sus libros! Puntaje final: {score}.";
            screenState = ScreenState.Victory;
            player?.Stop();
        }

        public string GetStudentDisplayName(Pasillo14Student student) => student.gameObject.name;

        public Vector2 GetPursuitWaypoint(Vector2 pursuerPosition, Vector2 targetPosition)
        {
            float[] classroomDoorX = { -8f, 0f, 8f };
            bool pursuerInClassroom = Mathf.Abs(pursuerPosition.y) > HallHalfHeight + 0.25f;
            bool targetInClassroom = Mathf.Abs(targetPosition.y) > HallHalfHeight + 0.25f;

            if (pursuerInClassroom)
            {
                bool sameClassroomSide = targetInClassroom && Mathf.Sign(pursuerPosition.y) == Mathf.Sign(targetPosition.y);
                int pursuerRoom = Mathf.Clamp(Mathf.FloorToInt((pursuerPosition.x + RoomHalfWidth) / 8f), 0, 2);
                int targetRoom = Mathf.Clamp(Mathf.FloorToInt((targetPosition.x + RoomHalfWidth) / 8f), 0, 2);
                if (sameClassroomSide && pursuerRoom == targetRoom)
                    return targetPosition;

                float exitDoorX = NearestDoorX(pursuerPosition.x, classroomDoorX);
                return new Vector2(exitDoorX, Mathf.Sign(pursuerPosition.y) * HallHalfHeight);
            }

            if (targetInClassroom)
            {
                float entryDoorX = NearestDoorX(targetPosition.x, classroomDoorX);
                return new Vector2(entryDoorX, Mathf.Sign(targetPosition.y) * HallHalfHeight);
            }

            return targetPosition;
        }

        public Vector2 GetRandomHallwayPatrolPoint()
        {
            return new Vector2(Random.Range(-10.5f, 10.5f), Random.Range(-2.25f, 2.25f));
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
            message = "¡Llegaste a la salida!";
            screenState = ScreenState.Victory;
            player?.Stop();
        }

        public void CaughtByEnemy()
        {
            if (hasWon || wasCaught)
                return;
            wasCaught = true;
            message = "La Profesora te atrapó.";
            screenState = ScreenState.GameOver;
            audioManager?.SetTheme(Pasillo14Audio.Theme.Caught);
            player?.Stop();
        }

        private void Update()
        {
            if (rebindAction != RebindAction.None)
            {
                CaptureRebind();
                return;
            }

            if (screenState != ScreenState.Playing)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                screenState = ScreenState.MainMenu;
                player?.Stop();
                audioManager?.SetTheme(Pasillo14Audio.Theme.Menu);
                return;
            }

            if (Keyboard.current != null && Keyboard.current[deliverKey].wasPressedThisFrame)
                DeliverToNearbyStudent();
            if (screenState != ScreenState.Playing)
                return;

            if (!hasWon && !wasCaught)
            {
                timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
                if (timeRemaining <= 0f)
                {
                    wasCaught = true;
                    timeoutLoss = true;
                    message = "Se acabó el tiempo.";
                    screenState = ScreenState.GameOver;
                    audioManager?.SetTheme(Pasillo14Audio.Theme.Caught);
                    player?.Stop();
                }
            }

            if (teacher != null && player != null)
                audioManager?.SetTheme(Vector2.Distance(teacher.transform.position, player.transform.position) < 6f
                    ? Pasillo14Audio.Theme.Pursuit : Pasillo14Audio.Theme.Gameplay);
        }

        private void StartGame()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            booksCollected = 0;
            booksDelivered = 0;
            bookInventory = 0;
            score = 0;
            studentsServed = 0;
            hasWon = false;
            wasCaught = false;
            timeoutLoss = false;
            sprintButtonToggled = false;
            message = "Recoge los libros y entrégaselos a tus compañeros.";
            students.Clear();
            player = null;
            teacher = null;
            cameraVelocity = Vector3.zero;
            BuildRoom();
            screenState = ScreenState.Playing;
            audioManager?.SetTheme(Pasillo14Audio.Theme.Gameplay);
        }

        private void DeliverToNearbyStudent()
        {
            if (player == null)
                return;
            foreach (Pasillo14Student student in students)
                if (student.IsNear(player))
                {
                    student.DeliverFromPlayer();
                    audioManager?.PlayUiSound();
                    return;
                }
            message = "Acércate a un compañero para entregar libros.";
        }

        private void LoadPreferences()
        {
            moveUpKey = (Key)PlayerPrefs.GetInt("P14_Up", (int)Key.W);
            moveLeftKey = (Key)PlayerPrefs.GetInt("P14_Left", (int)Key.A);
            moveDownKey = (Key)PlayerPrefs.GetInt("P14_Down", (int)Key.S);
            moveRightKey = (Key)PlayerPrefs.GetInt("P14_Right", (int)Key.D);
            sprintKey = (Key)PlayerPrefs.GetInt("P14_Sprint", (int)Key.LeftShift);
            deliverKey = (Key)PlayerPrefs.GetInt("P14_Deliver", (int)Key.E);
            musicVolume = PlayerPrefs.GetFloat("P14_MusicVolume", 0.65f);
            soundVolume = PlayerPrefs.GetFloat("P14_SoundVolume", 0.8f);
        }

        private void SavePreferences()
        {
            PlayerPrefs.SetInt("P14_Up", (int)moveUpKey);
            PlayerPrefs.SetInt("P14_Left", (int)moveLeftKey);
            PlayerPrefs.SetInt("P14_Down", (int)moveDownKey);
            PlayerPrefs.SetInt("P14_Right", (int)moveRightKey);
            PlayerPrefs.SetInt("P14_Sprint", (int)sprintKey);
            PlayerPrefs.SetInt("P14_Deliver", (int)deliverKey);
            PlayerPrefs.SetFloat("P14_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("P14_SoundVolume", soundVolume);
            PlayerPrefs.Save();
            audioManager?.SetVolumes(musicVolume, soundVolume);
        }

        private void CaptureRebind()
        {
            if (Keyboard.current == null)
                return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                rebindAction = RebindAction.None;
                return;
            }
            foreach (var key in Keyboard.current.allKeys)
            {
                if (!key.wasPressedThisFrame || key.keyCode == Key.None || key.keyCode == Key.Escape)
                    continue;
                switch (rebindAction)
                {
                    case RebindAction.Up: moveUpKey = key.keyCode; break;
                    case RebindAction.Left: moveLeftKey = key.keyCode; break;
                    case RebindAction.Down: moveDownKey = key.keyCode; break;
                    case RebindAction.Right: moveRightKey = key.keyCode; break;
                    case RebindAction.Sprint: sprintKey = key.keyCode; break;
                    case RebindAction.Deliver: deliverKey = key.keyCode; break;
                }
                rebindAction = RebindAction.None;
                SavePreferences();
                break;
            }
        }

        private void LateUpdate()
        {
            if (followCamera == null || player == null)
                return;

            Vector3 target = player.transform.position + new Vector3(8f, -9f, 13f);
            followCamera.transform.position = Vector3.SmoothDamp(
                followCamera.transform.position, target, ref cameraVelocity, 0.18f);
            followCamera.transform.LookAt(player.transform.position + new Vector3(0f, 0f, 0.7f), Vector3.forward);
            player.SetOccluded(IsPlayerHiddenFromCamera());
        }

        private bool IsPlayerHiddenFromCamera()
        {
            if (followCamera == null || player == null)
                return false;

            Vector3 target = player.transform.position + new Vector3(0f, 0f, 0.9f);
            Vector3 rayVector = target - followCamera.transform.position;
            float distance = rayVector.magnitude;
            if (distance <= 0.01f)
                return false;

            RaycastHit[] hits = Physics.RaycastAll(followCamera.transform.position,
                rayVector / distance, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
                if (hit.collider.GetComponentInParent<Pasillo14Occluder>() != null)
                    return true;
            return false;
        }

        private void OnGUI()
        {
            if (screenState != ScreenState.Playing)
            {
                DrawMenuBackdrop();
                if (screenState == ScreenState.MainMenu) DrawMainMenu();
                else if (screenState == ScreenState.Settings) DrawSettings();
                else if (screenState == ScreenState.GameOver) DrawEndScreen(false);
                else if (screenState == ScreenState.Victory) DrawEndScreen(true);
                return;
            }

            GUI.Box(new Rect(16f, 16f, 550f, 200f), GUIContent.none);
            GUI.Label(new Rect(30f, 22f, 510f, 22f), $"PASILLO 14    Libros: {booksCollected}/{RequiredBooks} recogidos | {booksDelivered} entregados | Mochila: {bookInventory}");
            int seconds = Mathf.CeilToInt(timeRemaining);
            GUI.Label(new Rect(30f, 45f, 500f, 20f), $"Tiempo: {seconds / 60:00}:{seconds % 60:00}    Puntaje: {score}    Compañeros: {studentsServed}/{students.Count}");
            int staminaPercent = player != null ? Mathf.CeilToInt(player.StaminaFraction * 100f) : 0;
            GUI.Label(new Rect(30f, 69f, 90f, 20f), "ESTAMINA");
            Color oldColor = GUI.color;
            GUI.color = new Color(0.03f, 0.09f, 0.16f, 1f);
            GUI.Box(new Rect(112f, 70f, 210f, 21f), GUIContent.none);
            if (player != null)
            {
                float stamina = player.StaminaFraction;
                Color staminaColor = stamina > 0.6f
                    ? Color.Lerp(new Color(0.11f, 0.78f, 0.83f), new Color(0.47f, 0.97f, 0.35f), (stamina - 0.6f) / 0.4f)
                    : stamina > 0.25f
                        ? Color.Lerp(new Color(1f, 0.28f, 0.08f), new Color(1f, 0.83f, 0.10f), (stamina - 0.25f) / 0.35f)
                        : Color.Lerp(new Color(1f, 0.12f, 0.22f), new Color(1f, 0.42f, 0.06f), stamina / 0.25f);
                if (stamina <= 0.25f && Mathf.FloorToInt(Time.time * 4f) % 2 == 0)
                    staminaColor = Color.white;
                GUI.color = staminaColor;
                GUI.Box(new Rect(116f, 74f, 202f * stamina, 13f), GUIContent.none);
                GUI.color = new Color(0.02f, 0.12f, 0.18f, 0.8f);
                for (int segment = 1; segment < 10; segment++)
                    GUI.Box(new Rect(116f + segment * 20f, 74f, 2f, 13f), GUIContent.none);
            }
            GUI.color = oldColor;
            GUI.Label(new Rect(330f, 69f, 95f, 22f), $"{staminaPercent}%");
            GUI.Label(new Rect(30f, 95f, 500f, 22f), message);
            GUI.Label(new Rect(30f, 121f, 500f, 20f), $"Mover: {moveUpKey}/{moveLeftKey}/{moveDownKey}/{moveRightKey}    Correr: {sprintKey}");
            GUI.Label(new Rect(30f, 145f, 300f, 20f), $"Acércate a un compañero y pulsa {deliverKey} para entregar.");
            if (GUI.Button(new Rect(340f, 140f, 190f, 30f), "ENTREGAR TAREA"))
                DeliverToNearbyStudent();
            string deliveryStatus = "";
            for (int i = 0; i < students.Count; i++)
            {
                Pasillo14Student student = students[i];
                deliveryStatus += $"{student.gameObject.name}: {student.DeliveredBooks}/{student.RequiredBooks}";
                if (i < students.Count - 1)
                    deliveryStatus += "   ";
            }
            GUI.Label(new Rect(30f, 168f, 510f, 20f), deliveryStatus);
            if (GUI.Button(new Rect(Screen.width - 150f, 18f, 132f, 30f), "PAUSA / MENÚ"))
            {
                screenState = ScreenState.MainMenu;
                player?.Stop();
                audioManager?.SetTheme(Pasillo14Audio.Theme.Menu);
            }
            if (GUI.Button(new Rect(Screen.width - 150f, 55f, 132f, 30f), sprintButtonToggled ? "CORRER: SÍ" : "CORRER: NO"))
                sprintButtonToggled = !sprintButtonToggled;
        }

        private void DrawMenuBackdrop()
        {
            if (titleTexture != null)
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), titleTexture, ScaleMode.ScaleAndCrop);
            else
            {
                GUI.color = new Color(0.16f, 0.31f, 0.38f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            GUI.color = new Color(0.025f, 0.07f, 0.12f, 0.76f);
            GUI.DrawTexture(new Rect(Screen.width * 0.66f, 0f, Screen.width * 0.34f, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawMainMenu()
        {
            float x = Screen.width * 0.68f;
            float width = Screen.width * 0.29f;
            GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            GUIStyle subtitle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUI.Label(new Rect(x, Screen.height * 0.13f, width, 54f), "PASILLO 14", title);
            GUI.Label(new Rect(x + 12f, Screen.height * 0.22f, width - 24f, 48f), "Recoge los libros. Ayuda a tus compañeros. Evita a La Profesora.", subtitle);
            float y = Screen.height * 0.36f;
            if (MenuButton("INICIAR JUEGO", x + width * 0.15f, y, width * 0.70f, 48f)) StartGame();
            if (MenuButton("CONFIGURACIÓN", x + width * 0.15f, y + 62f, width * 0.70f, 44f))
            {
                settingsReturnState = ScreenState.MainMenu;
                screenState = ScreenState.Settings;
            }
            if (MenuButton("SALIR", x + width * 0.15f, y + 120f, width * 0.70f, 40f)) Application.Quit();
            GUI.Label(new Rect(x + 10f, Screen.height - 48f, width - 20f, 26f), "WASD · Shift · E", subtitle);
        }

        private void DrawSettings()
        {
            float x = Screen.width * 0.675f;
            float width = Screen.width * 0.30f;
            GUIStyle heading = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x, 34f, width, 42f), "CONFIGURACIÓN", heading);
            GUI.Label(new Rect(x + 20f, 90f, width - 40f, 24f), "TECLAS · pulsa un botón y luego una tecla");
            float y = 126f;
            DrawBindingRow(x + 18f, y, "Mover arriba", moveUpKey, RebindAction.Up); y += 39f;
            DrawBindingRow(x + 18f, y, "Mover izquierda", moveLeftKey, RebindAction.Left); y += 39f;
            DrawBindingRow(x + 18f, y, "Mover abajo", moveDownKey, RebindAction.Down); y += 39f;
            DrawBindingRow(x + 18f, y, "Mover derecha", moveRightKey, RebindAction.Right); y += 39f;
            DrawBindingRow(x + 18f, y, "Correr", sprintKey, RebindAction.Sprint); y += 39f;
            DrawBindingRow(x + 18f, y, "Entregar tarea", deliverKey, RebindAction.Deliver); y += 52f;
            GUI.Label(new Rect(x + 20f, y, width - 40f, 22f), $"Volumen de música   {Mathf.RoundToInt(musicVolume * 100f)}%"); y += 25f;
            float newMusic = GUI.HorizontalSlider(new Rect(x + 24f, y, width - 48f, 24f), musicVolume, 0f, 1f);
            if (!Mathf.Approximately(newMusic, musicVolume)) { musicVolume = newMusic; SavePreferences(); }
            y += 37f;
            GUI.Label(new Rect(x + 20f, y, width - 40f, 22f), $"Volumen general   {Mathf.RoundToInt(soundVolume * 100f)}%"); y += 25f;
            float newSound = GUI.HorizontalSlider(new Rect(x + 24f, y, width - 48f, 24f), soundVolume, 0f, 1f);
            if (!Mathf.Approximately(newSound, soundVolume)) { soundVolume = newSound; SavePreferences(); }
            if (rebindAction != RebindAction.None)
                GUI.Label(new Rect(x + 16f, y + 38f, width - 32f, 24f), "Pulsa la tecla nueva (Esc cancela)");
            if (MenuButton("VOLVER", x + width * 0.22f, Screen.height - 76f, width * 0.56f, 42f))
            {
                rebindAction = RebindAction.None;
                screenState = settingsReturnState;
                if (screenState == ScreenState.Playing) player?.Stop();
            }
        }

        private void DrawBindingRow(float x, float y, string label, Key key, RebindAction action)
        {
            float width = Screen.width * 0.30f;
            GUI.Label(new Rect(x, y, width * 0.55f, 30f), label);
            string caption = rebindAction == action ? "Pulsa tecla…" : key.ToString();
            if (GUI.Button(new Rect(x + width * 0.60f, y, width * 0.33f, 30f), caption))
                rebindAction = action;
        }

        private void DrawEndScreen(bool won)
        {
            GUI.color = new Color(0.02f, 0.03f, 0.06f, 0.78f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float x = Screen.width * 0.32f;
            float width = Screen.width * 0.36f;
            GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x, Screen.height * 0.27f, width, 58f), won ? "¡TAREA ENTREGADA!" : "GAME OVER", title);
            string endMessage = won ? "Tus compañeros recibieron sus libros." : timeoutLoss ? "Se acabó el tiempo." : "La Profesora te atrapó.";
            GUI.Label(new Rect(x, Screen.height * 0.39f, width, 38f), endMessage, new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter });
            float y = Screen.height * 0.52f;
            if (MenuButton(won ? "JUGAR DE NUEVO" : "INTENTAR DE NUEVO", x + width * 0.15f, y, width * 0.70f, 48f)) StartGame();
            if (MenuButton("MENÚ PRINCIPAL", x + width * 0.15f, y + 62f, width * 0.70f, 42f))
            {
                for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
                player = null; teacher = null; students.Clear();
                screenState = ScreenState.MainMenu;
                audioManager?.SetTheme(Pasillo14Audio.Theme.Menu);
            }
        }

        private bool MenuButton(string label, float x, float y, float width, float height)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold };
            bool clicked = GUI.Button(new Rect(x, y, width, height), label, style);
            if (clicked) audioManager?.PlayUiSound();
            return clicked;
        }
    }
}

using UnityEngine;

namespace Pasillo14
{
    public sealed class Pasillo14Game : MonoBehaviour
    {
        private const float RoomWidth = 18f;
        private const float RoomHeight = 10f;
        private const int RequiredItems = 5;

        private int collectedItems;
        private bool hasWon;
        private string message = "¡Reúne los 5 útiles y llega a la puerta!";
        private Pasillo14Player player;
        private static Sprite squareSprite;

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
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 5.7f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.09f, 0.14f, 0.2f);

            CreateBlock("Piso del pasillo", Vector2.zero, new Vector2(RoomWidth, RoomHeight),
                new Color(0.77f, 0.82f, 0.78f), -10, false);
            CreateBlock("Pared norte", new Vector2(0f, 5f), new Vector2(RoomWidth, 0.45f),
                new Color(0.15f, 0.25f, 0.31f), 2, true);
            CreateBlock("Pared sur", new Vector2(0f, -5f), new Vector2(RoomWidth, 0.45f),
                new Color(0.15f, 0.25f, 0.31f), 2, true);
            CreateBlock("Pared oeste", new Vector2(-9f, 0f), new Vector2(0.45f, RoomHeight),
                new Color(0.15f, 0.25f, 0.31f), 2, true);
            CreateBlock("Pared este", new Vector2(9f, 0f), new Vector2(0.45f, RoomHeight),
                new Color(0.15f, 0.25f, 0.31f), 2, true);

            // Mesas que forman pequeños desvíos sin bloquear el recorrido.
            CreateBlock("Mesa 1", new Vector2(-3.5f, 0.1f), new Vector2(2.1f, 0.75f),
                new Color(0.55f, 0.34f, 0.22f), 0, true);
            CreateBlock("Mesa 2", new Vector2(0.2f, 1.5f), new Vector2(2.1f, 0.75f),
                new Color(0.55f, 0.34f, 0.22f), 0, true);
            CreateBlock("Mesa 3", new Vector2(3.7f, -0.8f), new Vector2(2.1f, 0.75f),
                new Color(0.55f, 0.34f, 0.22f), 0, true);

            CreatePlayer(new Vector2(-7f, -3.5f));
            CreateCollectible(new Vector2(-6f, 2.8f), new Color(1f, 0.78f, 0.2f));
            CreateCollectible(new Vector2(-1f, -2.4f), new Color(0.95f, 0.42f, 0.32f));
            CreateCollectible(new Vector2(0f, 3.4f), new Color(0.35f, 0.75f, 0.95f));
            CreateCollectible(new Vector2(5.5f, -2.6f), new Color(0.65f, 0.48f, 0.9f));
            CreateCollectible(new Vector2(6f, 2.8f), new Color(0.42f, 0.78f, 0.48f));
            CreateExit(new Vector2(7.7f, 0f));
        }

        private GameObject CreateBlock(string objectName, Vector2 position, Vector2 size,
            Color color, int sortingOrder, bool solid)
        {
            var block = new GameObject(objectName);
            block.transform.SetParent(transform);
            block.transform.position = position;
            block.transform.localScale = size;
            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            if (solid)
                block.AddComponent<BoxCollider2D>();
            return block;
        }

        private void CreatePlayer(Vector2 position)
        {
            var playerObject = new GameObject("Estudiante");
            playerObject.transform.SetParent(transform);
            playerObject.transform.position = position;
            playerObject.transform.localScale = new Vector3(0.65f, 0.8f, 1f);

            var renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = new Color(0.12f, 0.34f, 0.82f);
            renderer.sortingOrder = 3;

            var body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            playerObject.AddComponent<BoxCollider2D>();
            player = playerObject.AddComponent<Pasillo14Player>();
            player.Initialize(this, body);
        }

        private void CreateCollectible(Vector2 position, Color color)
        {
            var item = new GameObject("Útil escolar");
            item.transform.SetParent(transform);
            item.transform.position = position;
            item.transform.localScale = new Vector3(0.42f, 0.58f, 1f);

            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = color;
            renderer.sortingOrder = 2;

            var collider = item.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            item.AddComponent<Pasillo14Collectible>();
        }

        private void CreateExit(Vector2 position)
        {
            var exit = new GameObject("Puerta de salida");
            exit.transform.SetParent(transform);
            exit.transform.position = position;
            exit.transform.localScale = new Vector3(0.8f, 1.5f, 1f);

            var renderer = exit.AddComponent<SpriteRenderer>();
            renderer.sprite = SquareSprite;
            renderer.color = new Color(0.35f, 0.19f, 0.12f);
            renderer.sortingOrder = 1;

            var collider = exit.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            exit.AddComponent<Pasillo14Exit>();
        }

        public void CollectItem()
        {
            if (hasWon)
                return;

            collectedItems++;
            message = collectedItems == RequiredItems
                ? "¡Ya tienes todo! Corre a la puerta."
                : "¡Útil encontrado! Sigue buscando.";
        }

        public void TryExit()
        {
            if (collectedItems < RequiredItems)
            {
                int missing = RequiredItems - collectedItems;
                message = missing == 1 ? "Te falta 1 útil escolar." : $"Te faltan {missing} útiles escolares.";
                return;
            }

            hasWon = true;
            message = "¡Proyecto entregado a tiempo! Presiona R para jugar otra vez.";
            if (player != null)
                player.Stop();
        }

        private void Update()
        {
            if (hasWon && Pasillo14Player.RestartPressed())
                RestartLevel();
        }

        private void RestartLevel()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            collectedItems = 0;
            hasWon = false;
            message = "¡Reúne los 5 útiles y llega a la puerta!";
            player = null;
            BuildRoom();
        }

        private void OnGUI()
        {
            var box = new Rect(16f, 16f, 390f, 92f);
            GUI.Box(box, GUIContent.none);
            GUI.Label(new Rect(30f, 25f, 360f, 24f), $"PASILLO 14    Útiles: {collectedItems}/{RequiredItems}");
            GUI.Label(new Rect(30f, 51f, 360f, 24f), message);
            GUI.Label(new Rect(30f, 77f, 360f, 24f), "Mover: WASD o flechas    Reiniciar: R");
        }
    }
}

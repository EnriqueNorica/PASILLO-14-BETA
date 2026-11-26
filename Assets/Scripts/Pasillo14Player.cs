using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Pasillo14
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pasillo14Player : MonoBehaviour
    {
        private const float WalkSpeed = 2.8f;
        private const float RunSpeed = 4.8f;
        private const float Acceleration = 24f;
        private const float Braking = 32f;
        private const float TurnSpeed = 720f;
        private const float MaxStamina = 5f;
        private const float StaminaDrainPerSecond = 1.25f;
        private const float StaminaRecoveryPerSecond = 0.9f;
        private const float RecoveryDelay = 0.65f;
        private const float ResumeSprintThreshold = 1f;

        private Rigidbody2D? body;
        private Pasillo14Game? game;
        private Vector2 moveInput;
        private bool sprinting;
        private bool sprintHeld;
        private bool stopped;
        private bool exhausted;
        private float stamina = MaxStamina;
        private float recoveryTimer;
        private Renderer[] visualRenderers = System.Array.Empty<Renderer>();
        private Material[][] solidMaterials = System.Array.Empty<Material[]>();
        private Material[][] ghostMaterials = System.Array.Empty<Material[]>();
        private bool isOccluded;

        public float StaminaFraction => stamina / MaxStamina;
        public bool IsSprinting => sprinting;

        public void Initialize(Pasillo14Game owner, Rigidbody2D rigidbody)
        {
            game = owner;
            body = rigidbody;
            visualRenderers = GetComponentsInChildren<Renderer>(true);
            solidMaterials = new Material[visualRenderers.Length][];
            ghostMaterials = new Material[visualRenderers.Length][];

            for (int rendererIndex = 0; rendererIndex < visualRenderers.Length; rendererIndex++)
            {
                Material[] originals = visualRenderers[rendererIndex].sharedMaterials;
                solidMaterials[rendererIndex] = originals;
                var ghosts = new Material[originals.Length];
                for (int materialIndex = 0; materialIndex < originals.Length; materialIndex++)
                    ghosts[materialIndex] = CreateGhostMaterial(originals[materialIndex]);
                ghostMaterials[rendererIndex] = ghosts;
            }
        }

        public void SetOccluded(bool occluded)
        {
            if (isOccluded == occluded)
                return;

            isOccluded = occluded;
            for (int i = 0; i < visualRenderers.Length; i++)
                visualRenderers[i].sharedMaterials = occluded ? ghostMaterials[i] : solidMaterials[i];
        }

        private static Material CreateGhostMaterial(Material source)
        {
            var ghost = new Material(source);
            Color baseColor = ghost.HasProperty("_BaseColor") ? ghost.GetColor("_BaseColor") : ghost.color;
            Color ghostTint = Color.Lerp(baseColor, new Color(0.18f, 0.88f, 1f, 1f), 0.45f);
            ghostTint.a = 0.36f;
            ghost.color = ghostTint;
            if (ghost.HasProperty("_BaseColor"))
                ghost.SetColor("_BaseColor", ghostTint);
            if (ghost.HasProperty("_Surface"))
                ghost.SetFloat("_Surface", 1f);
            if (ghost.HasProperty("_Blend"))
                ghost.SetFloat("_Blend", 0f);
            ghost.SetOverrideTag("RenderType", "Transparent");
            ghost.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            ghost.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            ghost.SetInt("_ZWrite", 0);
            ghost.DisableKeyword("_ALPHATEST_ON");
            ghost.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            ghost.renderQueue = (int)RenderQueue.Transparent;
            return ghost;
        }

        private void OnDestroy()
        {
            foreach (Material[] rendererMaterials in ghostMaterials)
                foreach (Material material in rendererMaterials)
                    if (material != null)
                        Destroy(material);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || stopped)
            {
                moveInput = Vector2.zero;
                sprintHeld = false;
                sprinting = false;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;
            Key leftKey = game != null ? game.MoveLeftKey : Key.A;
            Key rightKey = game != null ? game.MoveRightKey : Key.D;
            Key downKey = game != null ? game.MoveDownKey : Key.S;
            Key upKey = game != null ? game.MoveUpKey : Key.W;
            if (keyboard[leftKey].isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard[rightKey].isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard[downKey].isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
            if (keyboard[upKey].isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;

            Vector2 screenRight = Vector2.right;
            Vector2 screenUp = Vector2.up;
            var camera = Camera.main;
            if (camera != null)
            {
                Vector3 right = camera.transform.right;
                Vector3 up = camera.transform.up;
                Vector2 projectedRight = new Vector2(right.x, right.y);
                Vector2 projectedUp = new Vector2(up.x, up.y);
                if (projectedRight.sqrMagnitude > 0.001f)
                    screenRight = projectedRight.normalized;
                if (projectedUp.sqrMagnitude > 0.001f)
                    screenUp = projectedUp.normalized;
            }
            moveInput = (screenRight * horizontal + screenUp * vertical).normalized;
            Key sprintKey = game != null ? game.SprintKey : Key.LeftShift;
            sprintHeld = keyboard[sprintKey].isPressed || (game != null && game.SprintButtonToggled);

            bool moving = moveInput.sqrMagnitude > 0.001f;
            sprinting = sprintHeld && moving && !exhausted && stamina > 0f;

            if (sprinting)
            {
                stamina = Mathf.Max(0f, stamina - StaminaDrainPerSecond * Time.deltaTime);
                recoveryTimer = RecoveryDelay;
                if (stamina <= 0f)
                {
                    exhausted = true;
                    sprinting = false;
                }
            }
            else
            {
                recoveryTimer = Mathf.Max(0f, recoveryTimer - Time.deltaTime);
                if (recoveryTimer <= 0f)
                    stamina = Mathf.Min(MaxStamina, stamina + StaminaRecoveryPerSecond * Time.deltaTime);
                if (exhausted && stamina >= ResumeSprintThreshold)
                    exhausted = false;
            }
        }

        private void FixedUpdate()
        {
            if (body == null)
                return;

            Vector2 targetVelocity = moveInput * (sprinting ? RunSpeed : WalkSpeed);
            float rate = moveInput.sqrMagnitude > 0.001f ? Acceleration : Braking;
            body.linearVelocity = Vector2.MoveTowards(
                body.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);

            if (moveInput.sqrMagnitude > 0.001f)
            {
                float targetRotation = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
                float rotation = Mathf.MoveTowardsAngle(
                    body.rotation, targetRotation, TurnSpeed * Time.fixedDeltaTime);
                body.MoveRotation(rotation);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collectible = other.GetComponent<Pasillo14Collectible>();
            if (collectible != null && game != null)
                collectible.Collect(game);

            var exit = other.GetComponent<Pasillo14Exit>();
            if (exit != null)
                game?.TryExit();
        }

        public void Stop()
        {
            stopped = true;
            moveInput = Vector2.zero;
            sprintHeld = false;
            sprinting = false;
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        public static bool RestartPressed()
        {
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
        }
    }
}

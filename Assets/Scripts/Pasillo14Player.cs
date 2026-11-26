using UnityEngine;
using UnityEngine.InputSystem;

namespace Pasillo14
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pasillo14Player : MonoBehaviour
    {
        private const float WalkSpeed = 4.5f;
        private const float RunSpeed = 6.3f;
        private const float Acceleration = 24f;
        private const float Braking = 32f;
        private const float TurnSpeed = 720f;

        private Rigidbody2D? body;
        private Pasillo14Game? game;
        private Vector2 moveInput;
        private bool sprinting;
        private bool stopped;

        public void Initialize(Pasillo14Game owner, Rigidbody2D rigidbody)
        {
            game = owner;
            body = rigidbody;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || stopped)
            {
                moveInput = Vector2.zero;
                sprinting = false;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;

            moveInput = new Vector2(horizontal, vertical).normalized;
            sprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
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
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        public static bool RestartPressed()
        {
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
        }
    }
}

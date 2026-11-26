using UnityEngine;
using UnityEngine.InputSystem;

namespace Pasillo14
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pasillo14Player : MonoBehaviour
    {
        private const float MoveSpeed = 5f;
        private Rigidbody2D body;
        private Pasillo14Game game;
        private Vector2 moveInput;
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
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
            moveInput = new Vector2(horizontal, vertical).normalized;
        }

        private void FixedUpdate()
        {
            if (body != null)
                body.linearVelocity = stopped ? Vector2.zero : moveInput * MoveSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collectible = other.GetComponent<Pasillo14Collectible>();
            if (collectible != null)
                collectible.Collect(game);

            var exit = other.GetComponent<Pasillo14Exit>();
            if (exit != null)
                game.TryExit();
        }

        public void Stop()
        {
            stopped = true;
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        public static bool RestartPressed()
        {
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
        }
    }
}

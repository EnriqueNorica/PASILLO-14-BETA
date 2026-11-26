using UnityEngine;
using UnityEngine.InputSystem;

namespace Pasillo14
{
    /// <summary>Lets the student open or close a classroom door from nearby.</summary>
    public sealed class Pasillo14Door : MonoBehaviour
    {
        private const float InteractionDistance = 1.9f;

        private Pasillo14Player? player;
        private Collider2D? doorCollider;
        private Transform? leaf;
        private bool isOpen;

        public void Initialize(Pasillo14Player? student, Collider2D collider, Transform doorLeaf)
        {
            player = student;
            doorCollider = collider;
            leaf = doorLeaf;
        }

        private void Update()
        {
            if (player == null || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
                return;

            if (Vector2.Distance(transform.position, player.transform.position) > InteractionDistance)
                return;

            isOpen = !isOpen;
            if (doorCollider != null)
                doorCollider.enabled = !isOpen;
            if (leaf != null)
                leaf.localRotation = Quaternion.Euler(0f, 0f, isOpen ? 90f : 0f);
        }
    }
}

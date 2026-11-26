using System.Collections.Generic;
using UnityEngine;

namespace Pasillo14
{
    /// <summary>Automatically swings open for people and closes after they pass.</summary>
    public sealed class Pasillo14Door : MonoBehaviour
    {
        private const float SwingSpeed = 220f;

        private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();
        private Collider2D? solidCollider;
        private BoxCollider? panelCollider;
        private Transform? leaf;
        private float angle;
        private float openingDirection = 1f;

        public void Initialize(Collider2D collider, BoxCollider? solidPanel, Transform doorLeaf)
        {
            solidCollider = collider;
            panelCollider = solidPanel;
            leaf = doorLeaf;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPerson(other))
                return;

            bool wasEmpty = occupants.Count == 0;
            occupants.Add(other);
            if (wasEmpty)
                openingDirection = other.transform.position.y < transform.position.y ? 1f : -1f;
            if (solidCollider != null)
                solidCollider.enabled = false;
            if (panelCollider != null)
                panelCollider.enabled = false;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (IsPerson(other))
                occupants.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            occupants.Remove(other);
        }

        private void Update()
        {
            bool shouldBeOpen = occupants.Count > 0;
            // The hinge is fixed at the frame edge; the door has a 180-degree swing envelope (-90 to +90).
            float targetAngle = shouldBeOpen ? openingDirection * 90f : 0f;
            angle = Mathf.MoveTowardsAngle(angle, targetAngle, SwingSpeed * Time.deltaTime);
            if (leaf != null)
                leaf.localRotation = Quaternion.Euler(0f, 0f, angle);

            if (!shouldBeOpen && Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < 1f && solidCollider != null)
                solidCollider.enabled = true;
            if (!shouldBeOpen && Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < 1f && panelCollider != null)
                panelCollider.enabled = true;
        }

        private static bool IsPerson(Collider2D other)
        {
            return other.GetComponent<Pasillo14Player>() != null ||
                   other.GetComponent<Pasillo14Enemy>() != null ||
                   other.GetComponent<Pasillo14Student>() != null;
        }
    }
}

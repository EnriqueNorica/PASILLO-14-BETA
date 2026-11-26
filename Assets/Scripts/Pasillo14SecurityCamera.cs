using UnityEngine;

namespace Pasillo14
{
    /// <summary>Watches a corridor sector and relays confirmed sightings to La Profesora.</summary>
    public sealed class Pasillo14SecurityCamera : MonoBehaviour
    {
        private const float ViewDistance = 7f;
        private const float HalfAngle = 105f;
        private const float AlertInterval = 0.45f;

        private Pasillo14Game? game;
        private Pasillo14Player? player;
        private Pasillo14Enemy? teacher;
        private Vector2 forward = Vector2.right;
        private float alertCooldown;

        public void Initialize(Pasillo14Game owner, Pasillo14Player? watchedPlayer, Pasillo14Enemy? alertedTeacher, Vector2 direction)
        {
            game = owner;
            player = watchedPlayer;
            teacher = alertedTeacher;
            forward = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        }

        private void Update()
        {
            if (alertCooldown > 0f)
                alertCooldown -= Time.deltaTime;
            if (alertCooldown > 0f || game == null || player == null || teacher == null)
                return;

            Vector2 toPlayer = (Vector2)(player.transform.position - transform.position);
            float distance = toPlayer.magnitude;
            if (distance > ViewDistance || distance <= 0.001f)
                return;
            if (Vector2.Dot(forward, toPlayer / distance) < Mathf.Cos(HalfAngle * Mathf.Deg2Rad))
                return;
            if (!HasClearView(player))
                return;

            teacher.ReceiveSecurityAlert(player.transform.position);
            game.NotifySecurityAlert();
            alertCooldown = AlertInterval;
        }

        private bool HasClearView(Pasillo14Player watchedPlayer)
        {
            RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, watchedPlayer.transform.position);
            foreach (RaycastHit2D hit in hits)
            {
                Collider2D obstacle = hit.collider;
                if (obstacle == null || obstacle.isTrigger)
                    continue;
                if (obstacle.transform == watchedPlayer.transform || obstacle.transform.IsChildOf(watchedPlayer.transform))
                    continue;
                return false;
            }

            return true;
        }
    }
}

using UnityEngine;

namespace Pasillo14
{
    /// <summary>Spots the player inside a forward cone and pursues the last seen position.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pasillo14Enemy : MonoBehaviour
    {
        private const float ViewDistance = 9f;
        private const float ViewHalfAngle = 55f;
        private const float MoveSpeed = 4.8f;
        private const float PatrolSpeed = 2.1f;
        private const float CatchDistance = 0.75f;
        private const float SearchDuration = 6f;

        private Pasillo14Game? game;
        private Pasillo14Player? player;
        private Rigidbody2D? body;
        private Collider2D? ownCollider;
        private Vector2 facing = Vector2.left;
        private Vector2 lastSeenPosition;
        private Vector2 patrolTarget;
        private float searchTimer;
        private float patrolWaitTimer;
        private bool chasing;

        public void Initialize(Pasillo14Game owner, Pasillo14Player? target, Rigidbody2D rigidbody)
        {
            game = owner;
            player = target;
            body = rigidbody;
            ownCollider = GetComponent<Collider2D>();
            patrolTarget = body.position;
            PickPatrolTarget();

            if (player != null)
            {
                Vector2 toPlayer = (Vector2)(player.transform.position - transform.position);
                if (toPlayer.sqrMagnitude > 0.001f)
                    Face(toPlayer.normalized);
            }
        }

        public void ReceiveSecurityAlert(Vector2 lastKnownPlayerPosition)
        {
            if (game == null || player == null)
                return;

            chasing = true;
            searchTimer = SearchDuration;
            lastSeenPosition = lastKnownPlayerPosition;
        }

        private void Update()
        {
            if (player == null || game == null)
                return;

            Vector2 toPlayer = (Vector2)(player.transform.position - transform.position);
            if (toPlayer.magnitude <= CatchDistance)
            {
                game.CaughtByEnemy();
                return;
            }

            if (CanSeePlayer(toPlayer))
            {
                chasing = true;
                searchTimer = SearchDuration;
                lastSeenPosition = player.transform.position;
            }
            else if (chasing)
            {
                searchTimer -= Time.deltaTime;
                if (searchTimer <= 0f)
                {
                    chasing = false;
                    patrolTarget = body != null ? body.position : (Vector2)transform.position;
                    patrolWaitTimer = Random.Range(0.5f, 1.4f);
                }
            }
            else if (patrolWaitTimer > 0f)
            {
                patrolWaitTimer -= Time.deltaTime;
            }
            else if (body != null && Vector2.Distance(body.position, patrolTarget) < 0.45f)
            {
                PickPatrolTarget();
            }
        }

        private bool CanSeePlayer(Vector2 toPlayer)
        {
            float distance = toPlayer.magnitude;
            if (distance > ViewDistance || distance <= 0.001f)
                return false;

            Vector2 direction = toPlayer / distance;
            if (Vector2.Dot(facing, direction) < Mathf.Cos(ViewHalfAngle * Mathf.Deg2Rad))
                return false;

            Vector2 origin = transform.position;
            Vector2 target = player!.transform.position;
            RaycastHit2D[] hits = Physics2D.LinecastAll(origin, target);
            foreach (RaycastHit2D hit in hits)
            {
                Collider2D obstacle = hit.collider;
                if (obstacle == null || obstacle.isTrigger || obstacle == ownCollider)
                    continue;
                if (obstacle.transform == player.transform || obstacle.transform.IsChildOf(player.transform))
                    continue;

                // Any solid collider (walls, tables, or other solid props) blocks the view ray.
                return false;
            }

            return true;
        }

        private void FixedUpdate()
        {
            if (body == null || player == null)
                return;

            Vector2 current = body.position;
            Vector2 destination;
            float speed;
            if (chasing && game != null)
            {
                destination = game.GetPursuitWaypoint(current, lastSeenPosition);
                speed = MoveSpeed;
            }
            else
            {
                destination = patrolTarget;
                speed = PatrolSpeed;
                if (patrolWaitTimer > 0f)
                    destination = current;
            }
            Vector2 offset = destination - current;
            if (offset.sqrMagnitude < 0.02f)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = offset.normalized;
            Face(direction);
            body.MovePosition(current + direction * (speed * Time.fixedDeltaTime));
        }

        private void PickPatrolTarget()
        {
            if (game == null)
                return;

            patrolTarget = game.GetRandomHallwayPatrolPoint();
            patrolWaitTimer = Random.Range(0.8f, 2f);
        }

        private void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
                return;

            facing = direction.normalized;
            transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f);
        }
    }
}

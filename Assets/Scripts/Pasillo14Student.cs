using UnityEngine;

namespace Pasillo14
{
    /// <summary>Waits in an classroom and accepts the books the student has collected.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pasillo14Student : MonoBehaviour
    {
        private const float InteractionDistance = 1.8f;

        private Pasillo14Game? game;
        private Pasillo14Player? player;
        private Transform? visualRoot;
        private int requiredBooks;
        private int deliveredBooks;
        private bool awarded;

        public int RemainingBooks => Mathf.Max(0, requiredBooks - deliveredBooks);
        public bool IsComplete => RemainingBooks == 0;

        public void Initialize(Pasillo14Game owner, Pasillo14Player? student, int booksNeeded, Transform visuals)
        {
            game = owner;
            player = student;
            requiredBooks = Mathf.Clamp(booksNeeded, 2, 4);
            visualRoot = visuals;
        }

        private void Update()
        {
            if (visualRoot != null)
                visualRoot.localPosition = Vector3.forward * (Mathf.Sin(Time.time * 2.1f) * 0.025f);

        }

        public bool IsNear(Pasillo14Player candidate)
        {
            return !IsComplete && Vector2.Distance(transform.position, candidate.transform.position) <= InteractionDistance;
        }

        public int DeliverFromPlayer()
        {
            if (IsComplete || player == null || game == null)
                return 0;
            int delivered = game.DeliverBooksTo(this);
            if (delivered > 0 && IsComplete && !awarded)
            {
                awarded = true;
                game.MarkStudentServed(this);
            }
            return delivered;
        }

        public string DisplayName => game != null ? game.GetStudentDisplayName(this) : "Compañero";

        public int AddBooks(int count)
        {
            int accepted = Mathf.Clamp(count, 0, RemainingBooks);
            deliveredBooks += accepted;
            return accepted;
        }

        public void UndoBooks(int count)
        {
            deliveredBooks = Mathf.Max(0, deliveredBooks - count);
        }

        public int RequiredBooks => requiredBooks;
        public int DeliveredBooks => deliveredBooks;
    }
}

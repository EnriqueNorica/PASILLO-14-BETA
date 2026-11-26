using UnityEngine;

namespace Pasillo14
{
    public sealed class Pasillo14Collectible : MonoBehaviour
    {
        private bool collected;
        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.localPosition;
        }

        private void Update()
        {
            transform.localPosition = startPosition + Vector3.up * (Mathf.Sin(Time.time * 2.4f) * 0.06f);
            transform.Rotate(0f, 0f, 55f * Time.deltaTime);
        }

        public void Collect(Pasillo14Game game)
        {
            if (collected)
                return;

            collected = true;
            game.CollectItem();
            Destroy(gameObject);
        }
    }
}

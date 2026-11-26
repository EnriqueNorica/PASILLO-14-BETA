using UnityEngine;

namespace Pasillo14
{
    public sealed class Pasillo14Collectible : MonoBehaviour
    {
        private bool collected;

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

using UnityEngine;

namespace Pasillo14
{
    /// <summary>Creates the prototype automatically when SampleScene starts.</summary>
    public static class Pasillo14Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartPrototype()
        {
            if (Object.FindObjectOfType<Pasillo14Game>() != null)
                return;

            var gameObject = new GameObject("Pasillo 14 - Game");
            Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Pasillo14Game>();
        }
    }
}

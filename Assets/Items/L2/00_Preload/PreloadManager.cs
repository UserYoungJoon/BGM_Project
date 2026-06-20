using UnityEngine;
using YoungJoon.L1.App;

namespace YoungJoon.L2.Preload
{
    public class PreloadManager : MonoBehaviour
    {
        [SerializeField] private string _firstScene = SceneNames.BATTLE;

        private void Start()
        {
            Ensure<SceneLoadHelper>();
            Ensure<GameManager>();

            GameManager.Instance.Init();
            GameManager.Instance.GoTo(_firstScene);
        }

        private static void Ensure<T>() where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() == null)
                new GameObject(typeof(T).Name).AddComponent<T>();
        }
    }
}

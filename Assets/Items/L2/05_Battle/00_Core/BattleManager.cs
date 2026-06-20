using UnityEngine;
using YoungJoon.L1.App;

namespace YoungJoon.L2.Battle
{
    public class BattleManager : MonoBehaviour, ISceneManager
    {
        public static BattleManager Instance { get; private set; }

        public string SceneName => SceneNames.BATTLE;

        private void Awake()
        {
            Instance = this;
            GameManager.Instance.Register(this);
        }

        public void OnEnterScene(SceneContext context)
        {
            Debug.Log($"[Battle] enter from '{context.FromScene}'");
        }

        public void OnExitScene() { }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Unregister(this);
            if (Instance == this)
                Instance = null;
        }
    }
}

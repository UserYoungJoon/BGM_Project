using System.Collections.Generic;
using UnityEngine.SceneManagement;
using YoungJoon.L0.Core;

namespace YoungJoon.L1.App
{
    public class GameManager : SingletonDDOL<GameManager>
    {
        private readonly Dictionary<string, ISceneManager> _scenes = new();
        private ISceneManager _active;
        private SceneContext _pendingContext;

        public ISceneManager Active => _active;

        public void Init()
        {
            SceneLoadHelper.Instance.OnStartLoad += HandleStartLoad;
            SceneLoadHelper.Instance.OnEndLoad += HandleEndLoad;
        }

        public void Register(ISceneManager mgr) => _scenes[mgr.SceneName] = mgr;

        public void Unregister(ISceneManager mgr)
        {
            if (_scenes.TryGetValue(mgr.SceneName, out var cur) && ReferenceEquals(cur, mgr))
                _scenes.Remove(mgr.SceneName);
        }

        public void EnterActive(string sceneName, SceneContext context = null)
        {
            _pendingContext = context ?? new SceneContext();
            ActivateScene(sceneName);
        }

        public void GoTo(string sceneName, SceneContext context = null)
        {
            _pendingContext = context ?? new SceneContext();
            _pendingContext.FromScene = SceneManager.GetActiveScene().name;
            _pendingContext.ToScene = sceneName;
            SceneLoadHelper.Instance.LoadScene(sceneName);
        }

        private void HandleStartLoad(string scene) => _active?.OnExitScene();

        private void HandleEndLoad(string scene) => ActivateScene(scene);

        private void ActivateScene(string sceneName)
        {
            if (_scenes.TryGetValue(sceneName, out var mgr))
            {
                _active = mgr;
                _active.OnEnterScene(_pendingContext);
            }
        }

        public override void OnDestroy()
        {
            if (SceneLoadHelper.Instance != null)
            {
                SceneLoadHelper.Instance.OnStartLoad -= HandleStartLoad;
                SceneLoadHelper.Instance.OnEndLoad -= HandleEndLoad;
            }
            base.OnDestroy();
        }
    }
}

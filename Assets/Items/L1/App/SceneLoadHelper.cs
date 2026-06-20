using System;
using System.Collections;
using UnityEngine.SceneManagement;
using YoungJoon.L0.Core;

namespace YoungJoon.L1.App
{
    public class SceneLoadHelper : SingletonDDOL<SceneLoadHelper>
    {
        public event Action<string> OnStartLoad;
        public event Action<string> OnEndLoad;

        private bool _isLoading;

        public bool IsLoading => _isLoading;

        public void LoadScene(string target)
        {
            if (_isLoading || string.IsNullOrEmpty(target))
                return;

            StartCoroutine(LoadRoutine(target));
        }

        private IEnumerator LoadRoutine(string target)
        {
            _isLoading = true;
            OnStartLoad?.Invoke(target);

            var op = SceneManager.LoadSceneAsync(target);
            while (!op.isDone)
                yield return null;

            _isLoading = false;
            OnEndLoad?.Invoke(target);
        }
    }
}

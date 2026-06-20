using UnityEngine;

namespace YoungJoon.L0.Core
{
    public class SingletonDDOL<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindFirstObjectByType(typeof(T));
                }
                return _instance;
            }
        }

        public virtual void Awake()
        {
            if (_instance == null)
                _instance = this as T;

            DontDestroyOnLoad(gameObject);
        }

        public virtual void OnDestroy()
        {
            // 에디터에서 도메인 리로드를 끄면 static이 초기화되지 않으므로, 파괴 시 수동으로 리셋한다.
            _instance = null;
        }
    }
}

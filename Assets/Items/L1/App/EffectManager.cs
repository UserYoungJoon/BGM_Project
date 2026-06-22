using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoungJoon.L0.Core;

namespace YoungJoon.L1.App
{
    // 미리 만든 UI-ready 이펙트 프리팹(UIParticle 루트)을 키로 재생. 키별 풀링으로 재사용.
    public class EffectManager : SingletonDDOL<EffectManager>
    {
        [System.Serializable]
        public class EffectEntry
        {
            public string Key;
            public GameObject Prefab;
            public float Duration = 1.5f;
        }

        [SerializeField] private RectTransform _layer;     // 이펙트 부모 (FxLayer)
        [SerializeField] private EffectEntry[] _effects;

        private Dictionary<string, EffectEntry> _map;
        private Dictionary<string, Stack<GameObject>> _pool;

        public override void Awake()
        {
            base.Awake();
            _map = new Dictionary<string, EffectEntry>(_effects.Length);
            _pool = new Dictionary<string, Stack<GameObject>>(_effects.Length);
            foreach (var e in _effects)
                if (e != null && e.Prefab != null && !string.IsNullOrEmpty(e.Key))
                {
                    _map[e.Key] = e;
                    _pool[e.Key] = new Stack<GameObject>();
                }
        }

        public void PlayEffect(string key, Vector3 worldPos, float timeScale = 1f)
        {
            if (!_map.TryGetValue(key, out var e)) return;
            timeScale = Mathf.Max(0.01f, timeScale);

            var go = Rent(key, e.Prefab);
            ((RectTransform)go.transform).position = worldPos;
            go.SetActive(true);

            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.simulationSpeed = timeScale;
                ps.Clear(true);
                ps.Play(true);
            }

            StartCoroutine(ReturnAfter(key, go, e.Duration / timeScale));
        }

        private GameObject Rent(string key, GameObject prefab)
        {
            var stack = _pool[key];
            while (stack.Count > 0)
            {
                var pooled = stack.Pop();
                if (pooled != null) return pooled;   // 파괴됐을 수 있으니 검사
            }
            return Instantiate(prefab, _layer);
        }

        private IEnumerator ReturnAfter(string key, GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go == null) yield break;
            go.SetActive(false);
            _pool[key].Push(go);
        }
    }
}

using UnityEngine;

namespace YoungJoon.L1.UI
{
    // 자기 RectTransform을 좌우(X)로 흔든다. sin 기반 deterministic이라 같은 프레임에 여러 개 호출하면 자동 싱크.
    public class Shaker : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.22f;
        [SerializeField] private float _strength = 18f;
        [SerializeField] private float _frequency = 55f;

        private RectTransform _rt;
        private Vector2 _home;
        private float _timer;
        private bool _shaking;

        private void Awake() => _rt = (RectTransform)transform;

        public void Shake()
        {
            if (!_shaking) _home = _rt.anchoredPosition;   // 흔드는 중이면 home 재캡처 안 함(드리프트 방지)
            _shaking = true;
            _timer = _duration;
        }

        private void LateUpdate()
        {
            if (!_shaking) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _rt.anchoredPosition = _home;
                _shaking = false;
                return;
            }

            float damper = _timer / _duration;
            float offsetX = Mathf.Sin((_duration - _timer) * _frequency) * _strength * damper;
            _rt.anchoredPosition = _home + new Vector2(offsetX, 0f);
        }
    }
}

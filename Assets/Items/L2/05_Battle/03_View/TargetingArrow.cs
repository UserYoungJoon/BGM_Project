using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YoungJoon.L2.Battle.View
{
    // Slay the Spire식 타겟팅 화살표. 고정 k개 세그먼트를 2차 베지어를 따라 매 프레임 재배치.
    public class TargetingArrow : MonoBehaviour
    {
        public enum TargetKind { None, Enemy, Ally }

        [Header("Sprite")]
        [SerializeField] private Sprite _arrowSprite;
        [SerializeField] private Vector2 _segmentSize = new Vector2(32f, 32f);

        [Header("Tuning (런타임 변경 가능)")]
        [SerializeField] private int _count = 10;          // 머리 포함 총 개수
        [SerializeField] private float _anchor = 0.35f;    // p1 기준점 (0=시작, 1=끝)
        [SerializeField] private float _bowK = 0.3f;       // 휨 = 거리 × bowK
        [SerializeField] private float _maxBow = 320f;
        [SerializeField] private float _tailScale = 0.5f;
        [SerializeField] private float _headScale = 2.0f;

        [Header("Colors")]
        [SerializeField] private Color _neutralColor = Color.white;
        [SerializeField] private Color _enemyColor = new Color(1f, 0.35f, 0.3f);
        [SerializeField] private Color _allyColor = new Color(0.4f, 1f, 0.55f);

        private RectTransform _rt;
        private Camera _cam;
        private RectTransform _source;
        private bool _active;
        private readonly List<RectTransform> _segs = new List<RectTransform>();

        private void Awake()
        {
            _rt = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            _cam = canvas != null ? canvas.worldCamera : null;
            Hide();
        }

        public void Begin(RectTransform source)
        {
            _source = source;
            _active = true;
        }

        public void Hide()
        {
            _active = false;
            _source = null;
            for (int i = 0; i < _segs.Count; i++)
                _segs[i].gameObject.SetActive(false);
        }

        public void Aim(Vector2 pointerScreen, TargetKind kind)
        {
            if (!_active || _source == null) return;

            EnsurePool(_count);
            Color col = kind == TargetKind.Enemy ? _enemyColor : kind == TargetKind.Ally ? _allyColor : _neutralColor;

            Vector2 p0 = ToLocal(RectTransformUtility.WorldToScreenPoint(_cam, _source.position));
            Vector2 p2 = ToLocal(pointerScreen);

            // 오프셋은 항상 위(up), 크기는 가로거리에 비례 → 수직 드래그면 bow≈0(직선), 좌우 교차해도 연속.
            Vector2 chord = p2 - p0;
            float bow = Mathf.Min(Mathf.Abs(chord.x) * _bowK, _maxBow);
            Vector2 p1 = Vector2.Lerp(p0, p2, _anchor) + Vector2.up * bow;

            int k = Mathf.Max(1, _count);
            for (int i = 0; i < _segs.Count; i++)
            {
                var seg = _segs[i];
                if (i >= k) { seg.gameObject.SetActive(false); continue; }

                seg.gameObject.SetActive(true);
                float t = k == 1 ? 1f : (float)i / (k - 1);
                Vector2 pos = Bezier(p0, p1, p2, t);
                Vector2 tan = BezierTangent(p0, p1, p2, t);

                seg.anchoredPosition = pos;
                float ang = Mathf.Atan2(tan.y, tan.x) * Mathf.Rad2Deg - 90f;   // 스프라이트가 '위' 방향
                seg.localRotation = Quaternion.Euler(0f, 0f, ang);
                seg.localScale = Vector3.one * Mathf.Lerp(_tailScale, _headScale, t);
                _segs[i].GetComponent<Image>().color = col;
            }
        }

        private void EnsurePool(int n)
        {
            while (_segs.Count < n)
            {
                var go = new GameObject("Seg" + _segs.Count, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(_rt, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = _segmentSize;
                var img = go.GetComponent<Image>();
                img.sprite = _arrowSprite;
                img.raycastTarget = false;
                _segs.Add(rt);
            }
        }

        private Vector2 ToLocal(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, screen, _cam, out var local);
            return local;
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static Vector2 BezierTangent(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
        }
    }
}

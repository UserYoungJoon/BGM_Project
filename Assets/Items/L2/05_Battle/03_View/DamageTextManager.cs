using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YoungJoon.L2.Battle.View
{
    public class DamageTextManager : MonoBehaviour
    {
        [SerializeField] private RectTransform _layer;
        [SerializeField] private TextMeshProUGUI _textPrefab;
        [SerializeField] private float _riseHeight = 120f;   // 상승 높이(레퍼런스 px). 캔버스 스케일로 월드 변환.
        [SerializeField] private float _duration = 0.7f;

        public void Pop(int amount, Vector3 worldPos, bool heal)
        {
            var t = Instantiate(_textPrefab, _layer);
            var rt = (RectTransform)t.transform;
            rt.position = worldPos;
            rt.SetAsLastSibling();

            t.text = (heal ? "+" : "-") + amount;
            t.color = heal ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.45f, 0.4f);

            float riseWorld = _riseHeight * _layer.lossyScale.y;
            DOTween.Sequence()
                .Append(rt.DOMove(worldPos + new Vector3(0, riseWorld, 0), _duration).SetEase(Ease.OutCubic))
                .Join(t.DOFade(0f, _duration).SetEase(Ease.InQuad))
                .OnComplete(() => Destroy(t.gameObject));
        }
    }
}

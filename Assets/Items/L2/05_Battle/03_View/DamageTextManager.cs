using DG.Tweening;
using TMPro;
using UnityEngine;

namespace YoungJoon.L2.Battle.View
{
    public class DamageTextManager : MonoBehaviour
    {
        [SerializeField] private RectTransform _layer;
        [SerializeField] private TMP_Text _textPrefab;

        public void Pop(int amount, Vector2 anchoredPos, bool heal)
        {
            var t = Instantiate(_textPrefab, _layer);
            var rt = (RectTransform)t.transform;
            rt.anchoredPosition = anchoredPos;
            rt.SetAsLastSibling();

            t.text = (heal ? "+" : "-") + amount;
            t.color = heal ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.45f, 0.4f);

            DOTween.Sequence()
                .Append(rt.DOAnchorPos(anchoredPos + new Vector2(0, 120), 0.7f).SetEase(Ease.OutCubic))
                .Join(t.DOFade(0f, 0.7f).SetEase(Ease.InQuad))
                .OnComplete(() => Destroy(t.gameObject));
        }
    }
}

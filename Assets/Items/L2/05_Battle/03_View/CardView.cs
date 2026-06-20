using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle.View
{
    public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _glow;
        [SerializeField] private Image _bg;
        [SerializeField] private Image _hpFill;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _hpText;

        public CardBase Model { get; private set; }

        private RectTransform _rt;
        private CanvasGroup _cg;
        private RectTransform _layer;
        private Vector2 _home;
        private bool _dragging;

        public System.Func<CardView, bool> CanDrag;
        public System.Action<CardView, CardView> OnAttackDrop;

        public RectTransform Rect => _rt;
        public Vector2 Home => _home;
        public Vector2 CenterInLayer => _rt.anchoredPosition;

        private void Awake()
        {
            _rt = (RectTransform)transform;
            _cg = GetComponent<CanvasGroup>();
        }

        public void Init(RectTransform layer) => _layer = layer;

        public void Set(CardDataSO data)
        {
            _bg.color = CardColors.Of(data.Type);
            _nameText.text = !string.IsNullOrEmpty(data.CardName) ? data.CardName : data.Type.ToString();
        }

        public void Bind(CardBase model)
        {
            Model = model;
            Set(model.Data);
            RefreshHp();
        }

        public void RefreshHp()
        {
            _hpText.text = Model.CurrentHp.ToString();
            _hpFill.fillAmount = Model.MaxHp > 0 ? Mathf.Clamp01((float)Model.CurrentHp / Model.MaxHp) : 0f;
        }

        public void SetHome(Vector2 home)
        {
            _home = home;
            _rt.anchoredPosition = home;
        }

        public void SetGlow(bool on)
        {
            _glow.DOKill();
            _glow.DOFade(on ? 0.85f : 0f, 0.15f);
        }

        public Tween Lunge(Vector2 targetPos)
        {
            _rt.SetAsLastSibling();
            Vector2 dir = targetPos - _home;
            Vector2 lunge = _home + dir * 0.55f;
            return DOTween.Sequence()
                .Append(_rt.DOAnchorPos(lunge, 0.13f).SetEase(Ease.OutQuad))
                .Append(_rt.DOAnchorPos(_home, 0.13f).SetEase(Ease.InQuad));
        }

        public Tween Hit()
        {
            return DOTween.Sequence()
                .Append(_rt.DOShakeAnchorPos(0.22f, 16f, 22, 90, false, true))
                .Join(_bg.DOColor(Color.white, 0.06f).SetLoops(2, LoopType.Yoyo));
        }

        public Tween SetHp(int value)
        {
            _hpText.text = Mathf.Max(0, value).ToString();
            float f = Model.MaxHp > 0 ? Mathf.Clamp01((float)value / Model.MaxHp) : 0f;
            return _hpFill.DOFillAmount(f, 0.2f);
        }

        public Tween Die()
        {
            return DOTween.Sequence()
                .Append(_rt.DOScale(0f, 0.28f).SetEase(Ease.InBack))
                .Join(_cg.DOFade(0f, 0.28f));
        }

        public Tween Born()
        {
            _rt.localScale = Vector3.zero;
            _cg.alpha = 0f;
            return DOTween.Sequence()
                .Append(_rt.DOScale(1f, 0.28f).SetEase(Ease.OutBack))
                .Join(_cg.DOFade(1f, 0.28f));
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (CanDrag == null || !CanDrag(this)) { _dragging = false; return; }
            _dragging = true;
            _rt.DOKill();
            _rt.SetAsLastSibling();
            _cg.blocksRaycasts = false;
            _rt.DOScale(1.12f, 0.1f);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_dragging) return;
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, e.position, null, out lp);
            _rt.anchoredPosition = lp;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragging) return;
            _dragging = false;
            _cg.blocksRaycasts = true;
            _rt.DOScale(1f, 0.1f);
            _rt.DOAnchorPos(_home, 0.15f);

            var target = RaycastCardView(e);
            if (target != null && OnAttackDrop != null)
                OnAttackDrop(this, target);
        }

        private CardView RaycastCardView(PointerEventData e)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(e, results);
            for (int i = 0; i < results.Count; i++)
            {
                var cv = results[i].gameObject.GetComponentInParent<CardView>();
                if (cv != null && cv != this)
                    return cv;
            }
            return null;
        }
    }
}

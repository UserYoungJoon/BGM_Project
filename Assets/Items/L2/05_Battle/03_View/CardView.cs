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
        [SerializeField] private Image _damageFill;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private Image _portrait;

        public CardBase Model { get; private set; }

        private RectTransform _rt;
        private CanvasGroup _cg;
        private RectTransform _layer;
        private Vector2 _home;
        private bool _dragging;
        private Tweener _damageTween;

        private const float DamageBarDuration = 0.4f;

        public System.Func<CardView, bool> CanDrag;
        public System.Action<CardView, CardView> OnAttackDrop;
        public System.Action<CardView> OnDragBegin;
        public System.Action<CardView, CardView> OnDragHover;
        public System.Action OnDragEnd;

        public RectTransform Rect => _rt;
        public Vector2 Home => _home;
        public Vector3 WorldCenter => _rt.position;

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
            if (_portrait != null)
            {
                _portrait.sprite = data.Illustration;
                _portrait.enabled = data.Illustration != null;
            }
        }

        public void Bind(CardBase model)
        {
            Model = model;
            Set(model.Data);
            RefreshHp();
        }

        public void RefreshHp()
        {
            float ratio = Model.MaxHp > 0 ? Mathf.Clamp01((float)Model.CurrentHp / Model.MaxHp) : 0f;
            _hpText.text = Model.CurrentHp.ToString();
            _damageTween?.Kill();
            _hpFill.fillAmount = ratio;
            _damageFill.fillAmount = ratio;
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

        public Tween Lunge(Vector3 worldTarget)
        {
            _rt.SetAsLastSibling();
            Vector3 home = _rt.position;
            Vector3 lunge = Vector3.Lerp(home, worldTarget, 0.55f);
            return DOTween.Sequence()
                .Append(_rt.DOMove(lunge, 0.13f).SetEase(Ease.OutQuad))
                .Append(_rt.DOMove(home, 0.13f).SetEase(Ease.InQuad));
        }

        public Tween Hit()
        {
            return DOTween.Sequence()
                .Append(_rt.DOShakeAnchorPos(0.22f, 16f, 22, 90, false, true))
                .Join(_bg.DOColor(Color.white, 0.06f).SetLoops(2, LoopType.Yoyo));
        }

        // 앞 게이지(_hpFill)는 즉시 갱신, 데미지바(_damageFill)가 이전값에서 천천히 추격 → 손실분이 쭈욱 줄어듦.
        public Tween SetHp(int value)
        {
            _hpText.text = Mathf.Max(0, value).ToString();
            float newRatio = Model.MaxHp > 0 ? Mathf.Clamp01((float)value / Model.MaxHp) : 0f;
            float prevRatio = _hpFill.fillAmount;

            _hpFill.fillAmount = newRatio;
            _damageTween?.Kill();

            if (newRatio < prevRatio)
            {
                _damageFill.fillAmount = prevRatio;
                _damageTween = _damageFill.DOFillAmount(newRatio, DamageBarDuration).SetEase(Ease.OutQuad);
                return _damageTween;
            }

            _damageFill.fillAmount = newRatio;
            return null;
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
            OnDragBegin?.Invoke(this);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_dragging) return;
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, e.position, e.pressEventCamera, out lp);
            _rt.anchoredPosition = lp;
            OnDragHover?.Invoke(this, RaycastCardView(e));
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragging) return;
            _dragging = false;
            _cg.blocksRaycasts = true;

            var target = RaycastCardView(e);
            OnDragEnd?.Invoke();

            if (target != null)
            {
                _rt.DOKill();
                _rt.localScale = Vector3.one;
                _rt.anchoredPosition = _home;
                if (OnAttackDrop != null) OnAttackDrop(this, target);
            }
            else
            {
                _rt.DOScale(1f, 0.1f);
                _rt.DOAnchorPos(_home, 0.15f);
            }
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

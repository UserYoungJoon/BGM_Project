using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YoungJoon.L1.Sound;

namespace YoungJoon.L1.UI
{
    // MTM TabButton 적응판. 클릭 시 부모 그룹에 선택 요청 + 클릭 사운드. 선택/해제 시 가상 훅 제공.
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public string TabName;

        [Header("Optional Visual")]
        [SerializeField] private Image _tabImage;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _highlightSprite;
        [SerializeField] private Sprite _pressedSprite;
        [SerializeField] private TextMeshProUGUI _tabText;
        [SerializeField] private Color _selectedTextColor = Color.white;
        [SerializeField] private Color _unselectedTextColor = Color.gray;
        [SerializeField] private string _clickSoundKey = SoundKey.Click;

        private Sprite _normalSprite;
        private TabGroup _parentGroup;
        private int _index;
        private bool _isSelected;
        private bool _isInside;
        private bool _isDown;

        public int TabIndex => _index;
        public bool IsSelected => _isSelected;

        public void Init(TabGroup parentGroup, int index)
        {
            _parentGroup = parentGroup;
            _index = index;
            if (_tabImage != null) _normalSprite = _tabImage.sprite;
            GetComponent<Button>().onClick.AddListener(OnClicked);
            if (_tabText != null) _tabText.color = _unselectedTextColor;
        }

        private void OnClicked()
        {
            if (_parentGroup != null) _parentGroup.SendSelectRequest(this);
        }

        public void Select()
        {
            _isSelected = true;
            UpdateVisualState();
            if (_tabText != null) _tabText.color = _selectedTextColor;
            OnSelect();
        }

        public void Unselect()
        {
            _isSelected = false;
            UpdateVisualState();
            if (_tabText != null) _tabText.color = _unselectedTextColor;
            OnUnselect();
        }

        protected virtual void OnSelect() { }
        protected virtual void OnUnselect() { }

        public void OnPointerEnter(PointerEventData e) { _isInside = true; UpdateVisualState(); }
        public void OnPointerExit(PointerEventData e) { _isInside = false; _isDown = false; UpdateVisualState(); }
        public void OnPointerDown(PointerEventData e) { _isDown = true; UpdateVisualState(); }

        public void OnPointerUp(PointerEventData e)
        {
            _isDown = false;
            UpdateVisualState();
            if (!string.IsNullOrEmpty(_clickSoundKey)) SoundManager.Instance.Play(_clickSoundKey);
        }

        private void UpdateVisualState()
        {
            if (_tabImage == null) return;
            if (_isSelected) { _tabImage.sprite = _selectedSprite != null ? _selectedSprite : _normalSprite; return; }
            if (_isDown) _tabImage.sprite = _pressedSprite != null ? _pressedSprite : _normalSprite;
            else if (_isInside) _tabImage.sprite = _highlightSprite != null ? _highlightSprite : _normalSprite;
            else _tabImage.sprite = _normalSprite;
        }
    }
}

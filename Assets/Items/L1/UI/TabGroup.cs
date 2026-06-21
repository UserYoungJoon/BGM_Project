using UnityEngine;
using YoungJoon.L0.Events;

namespace YoungJoon.L1.UI
{
    // MTM TabGroup 적응판. 항상 1개 선택(기본 첫번째). 선택 변경 시 OnTabChanged 발행.
    [RequireComponent(typeof(GameObjectEventBus))]
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] private TabButton[] _tabButtons;

        private TabButton _currentSelectedTab;
        private GameObjectEventBus _eventBus;

        public TabButton CurrentSelectedTab => _currentSelectedTab;
        public TabButton[] TabButtons => _tabButtons;

        public GameObjectEventBus EventBus
        {
            get
            {
                if (_eventBus == null) _eventBus = GetComponent<GameObjectEventBus>();
                return _eventBus;
            }
        }

        // 런타임 동적 탭 구성용.
        public void SetTabs(TabButton[] tabs) => _tabButtons = tabs;

        public void Init()
        {
            _currentSelectedTab = null;
            for (int i = 0; i < _tabButtons.Length; i++)
                _tabButtons[i].Init(this, i);

            if (_tabButtons.Length > 0)
                SelectTabByIndex(0);
        }

        public void SendSelectRequest(TabButton tab)
        {
            if (tab == _currentSelectedTab) return;

            var prev = _currentSelectedTab;
            if (_currentSelectedTab != null) _currentSelectedTab.Unselect();

            _currentSelectedTab = tab;
            tab.Select();

            EventBus.SendEvent(new OnTabChanged(this, prev, tab));
        }

        public void SelectTabByIndex(int index)
        {
            if (index < 0 || index >= _tabButtons.Length) return;
            SendSelectRequest(_tabButtons[index]);
        }
    }
}

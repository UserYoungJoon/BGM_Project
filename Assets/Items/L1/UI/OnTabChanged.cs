using YoungJoon.L0.Events;

namespace YoungJoon.L1.UI
{
    // 탭이 변경됐을 때 발생. PrevTabButton은 null일 수 있음.
    public struct OnTabChanged : IGameEvent
    {
        public readonly TabGroup TabGroup;
        public readonly TabButton PrevTabButton;
        public readonly TabButton CurrTabButton;

        public OnTabChanged(TabGroup tabGroup, TabButton prev, TabButton curr)
        {
            TabGroup = tabGroup;
            PrevTabButton = prev;
            CurrTabButton = curr;
        }
    }
}

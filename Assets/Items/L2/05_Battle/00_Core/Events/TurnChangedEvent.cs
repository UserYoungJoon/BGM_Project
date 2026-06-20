using YoungJoon.L0.Events;

namespace YoungJoon.L2.Battle
{
    public struct TurnChangedEvent : IGameEvent { public BattleState State; }
}

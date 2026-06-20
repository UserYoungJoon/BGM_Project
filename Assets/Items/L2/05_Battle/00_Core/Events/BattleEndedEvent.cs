using YoungJoon.L0.Events;

namespace YoungJoon.L2.Battle
{
    public struct BattleEndedEvent : IGameEvent { public bool PlayerWon; public int Stage; }
}

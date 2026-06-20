using System.Collections.Generic;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle
{
    public class BattleStep
    {
        public InteractResult Interaction;
        public readonly List<CardBase> Spawned = new List<CardBase>();
    }
}

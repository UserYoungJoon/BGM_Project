using System.Collections.Generic;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle
{
    /// <summary>모델이 한 번의 처리에서 만든 연출 사실 묶음. View가 큐에 담아 순서대로 재생한다.</summary>
    public class BattleStep
    {
        public CardBase Attacker;
        public CardBase Target;
        public readonly List<DamageResult> Hits = new List<DamageResult>();
        public readonly List<BlockResult> Blocks = new List<BlockResult>();
        public readonly List<HealResult> Heals = new List<HealResult>();
        public readonly List<CardBase> Spawned = new List<CardBase>();
    }
}

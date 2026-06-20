using System.Collections.Generic;

namespace YoungJoon.L2.Battle.Card
{
    public struct DamageFact
    {
        public CardBase Card;
        public int Amount;
        public int HpAfter;
        public bool Died;
    }

    public struct HealFact
    {
        public CardBase Card;
        public int Amount;
        public int HpAfter;
    }

    public class InteractResult
    {
        public CardBase Attacker;
        public CardBase Target;
        public List<DamageFact> Hits = new List<DamageFact>();
    }
}

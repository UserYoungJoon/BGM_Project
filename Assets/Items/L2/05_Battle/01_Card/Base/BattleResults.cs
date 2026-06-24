namespace YoungJoon.L2.Battle.Card
{
    public struct DamageResult
    {
        public CardBase Card;
        public int Amount;
        public int HpAfter;
        public bool Died;
    }

    public struct HealResult
    {
        public CardBase Card;
        public int Amount;
        public int HpAfter;
    }

    public struct BlockResult
    {
        public CardBase Card;
        public int Amount;
    }
}

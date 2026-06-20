namespace YoungJoon.L2.Battle.Card
{
    public struct HealSource
    {
        public readonly CardBase Healer;
        public readonly int HealAmount;

        public HealSource(CardBase healer, int healAmount)
        {
            Healer = healer;
            HealAmount = healAmount;
        }
    }
}

namespace YoungJoon.L2.Battle.Card
{
    public struct AttackSource
    {
        public readonly CardBase Attacker;
        public readonly int Damage;

        public AttackSource(CardBase attacker, int damage)
        {
            Attacker = attacker;
            Damage = damage;
        }
    }
}

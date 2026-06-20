namespace YoungJoon.L2.Battle.Card
{
    // [일반] 자신의 현재 HP만큼 선택한 상대 카드에게 피해.
    // 공격한 카드는 상대 카드의 현재 HP만큼 반격 피해를 받음.
    public class Card1000001_Normal : CardBase
    {
        public override CardType Type => CardType.Normal;

        public override void InteractWith(CardBase target)
        {
            int damage = CurrentHp;
            int counter = target.CurrentHp;

            target.AttackedBy(new AttackSource(this, damage));
            AttackedBy(new AttackSource(target, counter));
        }
    }
}

namespace YoungJoon.L2.Battle.Card
{
    // [힐러] 자신의 턴 시작 시, 자신을 제외한 아군 카드들의 HP를 1 회복.
    // 공격은 일반 카드와 동일하게 동작.
    public class Card2000001_Healer : CardBase
    {
        public override CardType Type => CardType.Healer;
    }
}

namespace YoungJoon.L2.Battle.Card
{
    // [무쌍] 선택한 적 카드에게 자신의 현재 HP의 100% 피해.
    // 추가로 선택한 카드와 인접한 적 카드 중 무작위 1장에게 자신의 현재 HP의 50% 피해.
    // 인접한 적 카드가 없으면 추가 피해는 발생하지 않음.
    public class Card1000003_Mussang : CardBase
    {
        public override CardType Type => CardType.Mussang;
    }
}

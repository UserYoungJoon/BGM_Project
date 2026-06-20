namespace YoungJoon.L2.Battle.Card
{
    public static class CardFactory
    {
        public static CardBase Create(CardType type)
        {
            switch (type)
            {
                case CardType.Normal:  return new Card1000001_Normal();
                case CardType.Ranged:  return new Card1000002_Ranged();
                case CardType.Mussang: return new Card1000003_Mussang();
                case CardType.Healer:  return new Card2000001_Healer();
                case CardType.Guard:   return new Card2000002_Guard();
                default: return null;
            }
        }
    }
}

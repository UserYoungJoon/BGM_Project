using UnityEngine;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle.View
{
    public static class CardColors
    {
        public static Color Of(CardType t)
        {
            switch (t)
            {
                case CardType.Normal:  return new Color(0.82f, 0.34f, 0.30f);
                case CardType.Ranged:  return new Color(0.34f, 0.68f, 0.40f);
                case CardType.Mussang: return new Color(0.58f, 0.40f, 0.80f);
                case CardType.Healer:  return new Color(0.34f, 0.60f, 0.85f);
                case CardType.Guard:   return new Color(0.85f, 0.72f, 0.36f);
                default: return Color.gray;
            }
        }
    }
}

using System;

namespace YoungJoon.L2.Battle.Card
{
    public enum CardCategory
    {
        None,
        Attack,
        Skill,
    }

    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Flags]
    public enum CardInteractType
    {
        None = 0,
        EnemyCard = 1 << 0,
        AllyCard = 1 << 1,
        EnemyPlayer = 1 << 2,
        AllyPlayer = 1 << 3,
    }

    public enum CardType
    {
        None = 0,
        Normal = 1000001,
        Ranged = 1000002,
        Mussang = 1000003,
        Healer = 2000001,
        Guard = 2000002
    }

    public static class CardTypeExtensions
    {
        public static CardCategory GetCategory(this CardType cardType)
        {
            if ((int)cardType >= 1000000 && (int)cardType < 2000000) return CardCategory.Attack;
            else if ((int)cardType >= 2000000 && (int)cardType < 3000000) return CardCategory.Skill;
            else return CardCategory.None;
        }
    }
}

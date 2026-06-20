using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    // [힐러] 자신의 턴 시작 시, 자신을 제외한 아군 카드들의 HP를 1 회복.
    // 공격은 일반 카드와 동일하게 동작.
    public class Card2000001_Healer : CardBase
    {
        public override CardType Type => CardType.Healer;

        private float _damageRatio;
        private float _counterRatio;
        private int _turnStartHeal;

        protected override void OnSpawn()
        {
            _damageRatio = Data.GetData("damageBasedHpRatio");
            _counterRatio = Data.GetData("counterBasedHpRatio");
            _turnStartHeal = Mathf.RoundToInt(Data.GetData("turnStartHeal"));
        }

        public override List<HealFact> OnTurnStart()
        {
            var facts = new List<HealFact>();
            if (_turnStartHeal <= 0) return facts;

            foreach (var ally in Owner.AliveFieldCardsExcept(this))
            {
                ally.HealedBy(new HealSource(this, _turnStartHeal));
                facts.Add(new HealFact { Card = ally, Amount = _turnStartHeal, HpAfter = ally.CurrentHp });
            }
            return facts;
        }

        public override InteractResult InteractWith(CardBase target)
        {
            var result = new InteractResult { Attacker = this, Target = target };
            int damage = Mathf.RoundToInt(CurrentHp * _damageRatio);
            int counter = Mathf.RoundToInt(target.CurrentHp * _counterRatio);
            result.Hits.Add(Deal(this, target, damage));
            result.Hits.Add(Deal(target, this, counter));
            return result;
        }
    }
}

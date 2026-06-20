using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    // [일반] 자신의 현재 HP만큼 선택한 상대 카드에게 피해.
    // 공격한 카드는 상대 카드의 현재 HP만큼 반격 피해를 받음.
    public class Card1000001_Normal : CardBase
    {
        public override CardType Type => CardType.Normal;

        private float _damageRatio;
        private float _counterRatio;

        protected override void OnSpawn()
        {
            _damageRatio = Data.GetData("damageBasedHpRatio");
            _counterRatio = Data.GetData("counterBasedHpRatio");
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

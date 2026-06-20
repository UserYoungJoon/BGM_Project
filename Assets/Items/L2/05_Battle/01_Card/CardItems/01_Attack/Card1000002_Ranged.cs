using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    // [원거리] 자신의 현재 HP만큼 선택한 상대 카드에게 피해.
    // 공격한 카드는 반격 피해를 받지 않음.
    public class Card1000002_Ranged : CardBase
    {
        public override CardType Type => CardType.Ranged;

        private float _damageRatio;

        protected override void OnSpawn()
            => _damageRatio = Data.GetData("damageBasedHpRatio");

        public override InteractResult InteractWith(CardBase target)
        {
            var result = new InteractResult { Attacker = this, Target = target };
            int damage = Mathf.RoundToInt(CurrentHp * _damageRatio);
            result.Hits.Add(Deal(this, target, damage));
            return result;
        }
    }
}

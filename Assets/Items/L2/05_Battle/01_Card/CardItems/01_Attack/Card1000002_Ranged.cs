using System.Collections.Generic;
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

        public override Dictionary<string, int> TooltipArgs()
            => new Dictionary<string, int> { { "dmg", Mathf.RoundToInt(CurrentHp * _damageRatio) } };

        public override void InteractWith(CardBase target)
        {
            int damage = Mathf.RoundToInt(CurrentHp * _damageRatio);
            BattleManager.Instance.Deal(this, target, damage);
        }
    }
}

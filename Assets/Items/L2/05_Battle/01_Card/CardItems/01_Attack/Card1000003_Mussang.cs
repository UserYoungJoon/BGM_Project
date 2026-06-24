using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    // [무쌍] 선택한 적 카드에게 자신의 현재 HP의 100% 피해.
    // 추가로 인접한 적 카드 전부에게 자신의 현재 HP의 50% 피해. (스펙의 '무작위 1장'을 인접 전부로 변경한 변형)
    // 인접한 적 카드가 없으면 추가 피해는 발생하지 않음.
    public class Card1000003_Mussang : CardBase
    {
        public override CardType Type => CardType.Mussang;

        private float _damageRatio;
        private float _splashRatio;

        protected override void OnSpawn()
        {
            _damageRatio = Data.GetData("damageBasedHpRatio");
            _splashRatio = Data.GetData("splashBasedHpRatio");
        }

        public override Dictionary<string, int> TooltipArgs()
            => new Dictionary<string, int>
            {
                { "dmg", Mathf.RoundToInt(CurrentHp * _damageRatio) },
                { "splash", Mathf.RoundToInt(CurrentHp * _splashRatio) }
            };

        public override void InteractWith(CardBase target)
        {
            int main = Mathf.RoundToInt(CurrentHp * _damageRatio);
            int splash = Mathf.RoundToInt(CurrentHp * _splashRatio);

            BattleManager.Instance.Deal(this, target, main);

            foreach (var neighbor in target.Owner.AdjacentAliveCards(target.SlotIndex))
                BattleManager.Instance.Deal(this, neighbor, splash);
        }
    }
}

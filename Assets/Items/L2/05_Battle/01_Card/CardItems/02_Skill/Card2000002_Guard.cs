using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    // [수호] 선택한 아군에게 최대 HP에 비례한 방어도를 부여. 방어도는 받는 피해를 먼저 흡수.
    public class Card2000002_Guard : CardBase
    {
        public override CardType Type => CardType.Guard;
        public override CardInteractType InteractType => CardInteractType.AllyCard;

        private float _blockRatio;

        protected override void OnSpawn()
        {
            _blockRatio = Data.GetData("blockBasedMaxHpRatio");
        }

        public override Dictionary<string, int> TooltipArgs()
            => new Dictionary<string, int> { { "block", Mathf.RoundToInt(MaxHp * _blockRatio) } };

        public override InteractResult InteractWith(CardBase target)
        {
            var result = new InteractResult { Attacker = this, Target = target };
            int block = Mathf.RoundToInt(MaxHp * _blockRatio);
            target.AddBlock(block);
            return result;
        }
    }
}

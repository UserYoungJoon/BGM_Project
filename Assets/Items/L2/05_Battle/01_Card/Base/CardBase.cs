using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    public abstract class CardBase
    {
        public abstract CardType Type { get; }

        private CardDataSO _data;
        private int _currentHp;
        private int _maxHp;
        private int _block; // 방어도

        public CardPlayerBase Owner { get; set; }
        public int SlotIndex { get; set; }

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public int Block => _block;
        public bool IsDead => _currentHp <= 0;
        public CardDataSO Data => _data;
        public int Cost => _data != null ? _data.Cost : 0;

        public virtual CardInteractType InteractType => CardInteractType.EnemyCard;

        public void Spawn(CardDataSO data)
        {
            _data = data;
            _maxHp = data.Hp;
            _currentHp = data.Hp;
            _block = 0;
            OnSpawn();
        }

        protected virtual void OnSpawn() { }

        public virtual void OnTurnStart() { }
        public virtual void OnTurnEnd() { }
        public virtual void InteractWith(CardBase target) { }

        public void AddBlock(int amount)
        {
            if (amount > 0) _block += amount;
        }

        public void AttackedBy(AttackSource source)
        {
            if (source.Damage <= 0) return;
            int dmg = source.Damage;
            if (_block > 0)
            {
                int absorbed = Mathf.Min(_block, dmg);
                _block -= absorbed;
                dmg -= absorbed;
            }
            _currentHp -= dmg;
            if (_currentHp < 0) _currentHp = 0;
        }

        public void HealedBy(HealSource source)
        {
            if (source.HealAmount <= 0 || IsDead) return;
            _currentHp = Mathf.Min(_maxHp, _currentHp + source.HealAmount);
        }

        public virtual string GetLocalizedTooltip() => _data.Tooltip;

        // 툴팁의 {키} 자리에 들어갈 실 수치(현재 HP 기준 계산값). 카드별 override.
        public virtual Dictionary<string, int> TooltipArgs() => new Dictionary<string, int>();
    }
}

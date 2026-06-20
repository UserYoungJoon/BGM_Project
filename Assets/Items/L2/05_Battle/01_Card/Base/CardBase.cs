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

        public CardPlayerBase Owner { get; set; }
        public int SlotIndex { get; set; }

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public bool IsDead => _currentHp <= 0;
        public CardDataSO Data => _data;
        public int Cost => _data != null ? _data.Cost : 0;

        public void Spawn(CardDataSO data)
        {
            _data = data;
            _maxHp = data.Hp;
            _currentHp = data.Hp;
            OnSpawn();
        }

        protected virtual void OnSpawn() { }

        public virtual List<HealFact> OnTurnStart() => null;
        public virtual void OnTurnEnd() { }

        public virtual InteractResult InteractWith(CardBase target)
            => new InteractResult { Attacker = this, Target = target };

        public virtual void InteractWith(CardPlayerBase cardPlayer) { }

        public void AttackedBy(in AttackSource source)
        {
            if (source.Damage <= 0) return;
            _currentHp -= source.Damage;
            if (_currentHp < 0) _currentHp = 0;
        }

        public void HealedBy(in HealSource source)
        {
            if (source.HealAmount <= 0 || IsDead) return;
            _currentHp = Mathf.Min(_maxHp, _currentHp + source.HealAmount);
        }

        protected DamageFact Deal(CardBase from, CardBase victim, int amount)
        {
            victim.AttackedBy(new AttackSource(from, amount));
            return new DamageFact { Card = victim, Amount = amount, HpAfter = victim.CurrentHp, Died = victim.IsDead };
        }

        public virtual string GetLocalizedTooltip() => _data.Tooltip;
    }
}

using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    public abstract class CardBase
    {
        public abstract CardType Type { get; }

        private CardDataSO _data;

        private int _currentHp;
        private int _maxHp;
        private int _damage;
        private int _heal;
        private int _costForTurn;

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public bool IsDead => _currentHp <= 0;
        public CardDataSO Data => _data;

        public void Spawn(CardDataSO data)
        {
            _data = data;
            _maxHp = data.Hp;
            _currentHp = data.Hp;
            OnSpawn();
        }

        protected virtual void OnSpawn()
        {

        }

        public virtual void OnTurnStart()
        {

        }

        public virtual void OnTurnEnd()
        {

        }

        public virtual void InteractWith(CardBase target)
        {

        }

        public virtual void InteractWith(CardPlayerBase cardPlayer)
        {

        }

        public void AttackedBy(in AttackSource source)
        {
            if (source.Damage <= 0) return;

            _currentHp -= source.Damage;
            if (_currentHp <= 0)
            {
                _currentHp = 0;
                OnDead();
            }
        }

        public void HealedBy(in HealSource source)
        {
            if (source.HealAmount <= 0 || IsDead) return;

            _currentHp = Mathf.Min(_maxHp, _currentHp + source.HealAmount);
        }

        protected virtual void OnDead()
        {

        }

        public virtual string GetLocalizedTooltip()
        {
            return _data.Tooltip;
        }
    }
}

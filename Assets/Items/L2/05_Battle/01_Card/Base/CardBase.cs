using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    public abstract class CardBase
    {
        public abstract CardType Type { get; }

        private CardDataSO _data;

        private int _currentHp;
        private int _maxHp;

        public void Spawn()
        {
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

        public void InteractWith(CardBase otherCard)
        {

        }

        public void AttackedBy(CardBase attacker)
        {

        }

        public void HealedBy(CardBase healer)
        {

        }

        public virtual string GetLocalizedTooltip()
        {
            return _data.Tooltip;
        }
    }
}

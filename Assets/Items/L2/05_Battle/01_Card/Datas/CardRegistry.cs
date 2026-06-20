using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    [CreateAssetMenu(fileName = "CardRegistry", menuName = "BGM/Card/CardRegistry")]
    public class CardRegistry : ScriptableObject
    {
        [SerializeField] private List<CardDataSO> _cards = new();

        private Dictionary<CardType, CardDataSO> _dict;

        private void BuildDictionary()
        {
            _dict = new Dictionary<CardType, CardDataSO>(_cards.Count);
            foreach (var c in _cards)
                if (c != null && c.Type != CardType.None)
                    _dict[c.Type] = c;
        }

        public void BuildAllCaches()
        {
            BuildDictionary();
            foreach (var c in _cards)
                if (c != null)
                    c.BuildCache();
        }

        public CardDataSO GetCardData(CardType type)
        {
            if (_dict == null) BuildDictionary();
            if (_dict.TryGetValue(type, out var data)) return data;
            Debug.LogWarning($"[CardRegistry] CardDataSO not found for {type}");
            return null;
        }

        public IReadOnlyList<CardDataSO> All => _cards;
    }
}

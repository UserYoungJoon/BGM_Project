using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L2.Battle.Card
{
    public class CardDataSO : ScriptableObject
    {
        [SerializeField] private CardType _type;
        [SerializeField] private string _cardName;
        [SerializeField] private string _tooltip;
        [SerializeField] private Sprite _illust;
        [SerializeField] private int _hp;
        [SerializeField] private int _cost;
        [SerializeField] private List<CardEffectParam> _effectParams = new();

        public CardType Type => _type;
        public string CardName => _cardName;
        public string Tooltip => _tooltip;
        public Sprite Illust => _illust;
        public int Hp => _hp;
        public int Cost => _cost;

        private Dictionary<string, float> _effectCache;

        public void BuildCache()
        {
            _effectCache = new Dictionary<string, float>(_effectParams.Count);
            foreach (var p in _effectParams)
                _effectCache[p.Key] = p.Value;
        }

        public float GetData(string key)
        {
            if (_effectCache.TryGetValue(key, out var v)) return v;
            Debug.LogError($"[CardDataSO] effect key '{key}' not found in {_type}");
            return 0f;
        }

        public float this[string key] => GetData(key);

        public IReadOnlyDictionary<string, float> EffectMap => _effectCache;

#if UNITY_EDITOR
        public void ReadData(CardType type, string cardName, string tooltip, int hp, int cost, IDictionary<string, float> effects)
        {
            _type = type;
            _cardName = cardName;
            _tooltip = tooltip;
            _hp = hp;
            _cost = cost;

            _effectParams.Clear();
            if (effects != null)
                foreach (var kv in effects)
                    _effectParams.Add(new CardEffectParam(kv.Key, kv.Value));

            _effectCache = null;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

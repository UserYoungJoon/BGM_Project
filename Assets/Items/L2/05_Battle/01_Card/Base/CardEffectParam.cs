using System;

namespace YoungJoon.L2.Battle.Card
{
    [Serializable]
    public struct CardEffectParam
    {
        public string Key;
        public float Value;

        public CardEffectParam(string key, float value)
        {
            Key = key;
            Value = value;
        }
    }
}

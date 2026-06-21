using System;
using UnityEngine;

namespace YoungJoon.L1.Sound
{
    [Serializable]
    public class SoundAsset
    {
        public string Key;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
    }
}

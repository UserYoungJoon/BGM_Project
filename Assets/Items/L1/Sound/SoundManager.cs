using System.Collections.Generic;
using UnityEngine;
using YoungJoon.L0.Core;

namespace YoungJoon.L1.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : SingletonDDOL<SoundManager>
    {
        [SerializeField] private SoundAsset[] _sounds;

        private AudioSource _source;
        private Dictionary<string, SoundAsset> _map;

        public override void Awake()
        {
            base.Awake();
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;

            _map = new Dictionary<string, SoundAsset>(_sounds.Length);
            foreach (var s in _sounds)
                if (s != null && s.Clip != null && !string.IsNullOrEmpty(s.Key))
                    _map[s.Key] = s;
        }

        public void Play(string key)
        {
            if (_map.TryGetValue(key, out var s))
                _source.PlayOneShot(s.Clip, s.Volume);
        }
    }
}

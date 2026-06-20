using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L1.Text
{
    [Serializable]
    public class ColorTagEntry
    {
        public string tag;
        public Color color = Color.white;
    }

    [Serializable]
    public class IconTagEntry
    {
        public string key;
        public string spriteName;
    }

    [CreateAssetMenu(fileName = "RichTextStyleConfig", menuName = "BGM/Text/RichTextStyleConfig")]
    public class RichTextStyleConfig : ScriptableObject
    {
        private static RichTextStyleConfig _instance;

        public static RichTextStyleConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<RichTextStyleConfig>("RichTextStyleConfig");
                    if (_instance == null)
                        Debug.LogError("[RichTextStyleConfig] Resources/RichTextStyleConfig 로드 실패");
                }
                return _instance;
            }
        }

        [Header("<tag>text</tag> → <color=#HEX>text</color>")]
        public List<ColorTagEntry> colorTags = new();

        [Header("<icon=key> → <sprite name=\"spriteName\">")]
        public List<IconTagEntry> iconTags = new();

        private Dictionary<string, Color> _colorCache;
        private Dictionary<string, string> _iconCache;

        public Dictionary<string, Color> ColorCache => _colorCache;
        public Dictionary<string, string> IconCache => _iconCache;

        public void Init()
        {
            _colorCache = new Dictionary<string, Color>();
            foreach (var entry in colorTags)
                if (!string.IsNullOrEmpty(entry.tag))
                    _colorCache[entry.tag] = entry.color;

            _iconCache = new Dictionary<string, string>();
            foreach (var entry in iconTags)
                if (!string.IsNullOrEmpty(entry.key))
                    _iconCache[entry.key] = entry.spriteName;
        }
    }
}

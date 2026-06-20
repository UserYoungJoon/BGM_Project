using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YoungJoon.L1.Text
{
    public static class RichTextConverter
    {
        private static RichTextStyleConfig _config;

        private static readonly Regex IconTagRegex = new(@"<icon=(\w+)>", RegexOptions.Compiled);
        private static readonly Regex ValidateTagRegex = new(@"<(/?)(\w+)[^>]*>", RegexOptions.Compiled);
        private static Dictionary<string, (Regex regex, string replacement)> _colorTagCache;

        public static bool IsInitialized => _config != null;

        public static void Init(RichTextStyleConfig config)
        {
            _config = config;
            config.Init();

            _colorTagCache = new Dictionary<string, (Regex, string)>();
            foreach (var kvp in _config.ColorCache)
            {
                string tag = kvp.Key;
                string hex = ColorUtility.ToHtmlStringRGB(kvp.Value);
                var regex = new Regex($@"<{tag}>(.*?)</{tag}>", RegexOptions.Compiled | RegexOptions.Singleline);
                _colorTagCache[tag] = (regex, $"<color=#{hex}>$1</color>");
            }
        }

        public static string Convert(string text)
        {
            if (_config == null)
            {
                Debug.LogWarning("[RichTextConverter] Init() 먼저 호출 필요");
                return text;
            }
            if (string.IsNullOrEmpty(text))
                return text;

            text = ConvertIconTags(text);
            text = ConvertColorTags(text);
            return text;
        }

        private static string ConvertIconTags(string text)
        {
            return IconTagRegex.Replace(text, match =>
            {
                string key = match.Groups[1].Value;
                if (_config.IconCache.TryGetValue(key, out string spriteName))
                    return $"<sprite name=\"{spriteName}\" color=#FFFFFF>";

                Debug.LogWarning($"[RichTextConverter] icon key 없음: {key}");
                return match.Value;
            });
        }

        private static string ConvertColorTags(string text)
        {
            foreach (var kvp in _colorTagCache)
            {
                var (regex, replacement) = kvp.Value;
                text = regex.Replace(text, replacement);
            }
            return text;
        }

        public static bool Validate(string text, out string error)
        {
            var stack = new Stack<string>();
            foreach (Match match in ValidateTagRegex.Matches(text))
            {
                bool isClosing = match.Groups[1].Value == "/";
                string tagName = match.Groups[2].Value.ToLower();
                if (tagName == "icon" || tagName == "sprite")
                    continue;

                if (isClosing)
                {
                    if (stack.Count == 0 || stack.Peek() != tagName)
                    {
                        error = $"태그 순서 오류: </{tagName}>";
                        return false;
                    }
                    stack.Pop();
                }
                else stack.Push(tagName);
            }

            if (stack.Count > 0)
            {
                error = $"닫히지 않은 태그: <{stack.Peek()}>";
                return false;
            }
            error = null;
            return true;
        }
    }
}

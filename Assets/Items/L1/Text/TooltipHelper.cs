using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YoungJoon.L1.Text
{
    public static class TooltipHelper
    {
        private static readonly Regex BracePattern = new(@"\{(\w+)\}", RegexOptions.Compiled);

        private static void EnsureInit()
        {
            if (RichTextConverter.IsInitialized) return;
            var config = RichTextStyleConfig.Instance;
            if (config != null)
                RichTextConverter.Init(config);
        }

        public static string Build(string template, IReadOnlyDictionary<string, string> values = null)
        {
            EnsureInit();
            string replaced = ReplaceBraces(template, values);
            return RichTextConverter.Convert(replaced);
        }

        public static string Build(string template, IReadOnlyDictionary<string, float> values)
        {
            var dict = new Dictionary<string, string>(values.Count);
            foreach (var kv in values)
                dict[kv.Key] = kv.Value.ToString("0.###");
            return Build(template, dict);
        }

        public static string ReplaceBraces(string raw, IReadOnlyDictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(raw) || values == null)
                return raw;

            return BracePattern.Replace(raw, m =>
                values.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);
        }
    }
}

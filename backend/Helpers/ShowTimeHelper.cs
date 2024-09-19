using backend.Models;

namespace backend.Helpers
{
    public static class ShowTimeHelper
    {
        private static readonly Dictionary<ShowTimeDubType, string[]> _showTimeDubTypeMap = new()
        {
            { ShowTimeDubType.Regular, [""] },
            { ShowTimeDubType.OriginalVersion, ["OV", "OF", "Original Version", "Originalversion", "Originalfassung", "Original Fassung"] },
            { ShowTimeDubType.Subtitled, ["OmU", "OmeU", "OmdU", "Untertitel", "OmenglU"] },
        };

        private static readonly Dictionary<ShowTimeLanguage, string[]> _showTimeLanguageMap = new()
        {
            { ShowTimeLanguage.German, ["Deutsch","de","deu", "dt.", "DK"] },
            { ShowTimeLanguage.Danish, ["Dänisch","dän", "DK"] },
            { ShowTimeLanguage.English, ["Englisch", "English","eng","engl","EN.","GB","UK","US", "USA"] },
            { ShowTimeLanguage.French, ["Französisch","franz","frnz","frz","FR"] },
            { ShowTimeLanguage.Spanish, ["Spanisch","span","SP","ES"] },
            { ShowTimeLanguage.Italian, ["Italienisch","ital","IT"] },
            { ShowTimeLanguage.Turkish, ["Türkisch", "türk","trk", "TR"] },
            { ShowTimeLanguage.Russian, ["Russisch", "russ"] },
            { ShowTimeLanguage.Japanese, ["Japanisch", "jap", "JP", "JA"] },
            { ShowTimeLanguage.Korean, ["Koreanisch", "kor", "KO"] },
            { ShowTimeLanguage.Hindi, ["Hindi", "hind", "hin" ] },
            { ShowTimeLanguage.Polish, ["Polnisch", "pol", "PL"] },
            { ShowTimeLanguage.Other, ["Andere", "Verschiedene", "versch.", "div.", "Malayalam", "Filipino", "Georgisch", "georg." ]},
            { ShowTimeLanguage.Unknown, ["Unbekannt"] },
        };

        public static string GetLanguageName(ShowTimeLanguage language)
        {
            var values = _showTimeLanguageMap[language];
            return values[0];
        }

        public static string GetTypeName(ShowTimeDubType type)
        {
            var values = _showTimeDubTypeMap[type];
            return values[0];
        }

        public static ShowTimeLanguage? TryGetLanguage(string language, ShowTimeLanguage? def)
        {
            var result = FindMatchingDictionaryKey(language, _showTimeLanguageMap, ShowTimeLanguage.Unknown);
            if (result == ShowTimeLanguage.Unknown)
            {
                return def;
            }
            return result;
        }

        public static ShowTimeLanguage GetLanguage(string language)
        {
            return FindMatchingDictionaryKey(language, _showTimeLanguageMap, ShowTimeLanguage.German);
        }

        public static ShowTimeDubType GetDubType(string type)
        {
            type = type.Trim().Replace(".", null).Replace("(", null).Replace(")", null);

            var results = FindMatchingDictionaryKeys(type, _showTimeDubTypeMap);
            if (results.Count == 0)
            {
                return ShowTimeDubType.Regular;
            }
            // If both OV and OmU are found, return OmU, as it is more specific
            if (results.Contains(ShowTimeDubType.Subtitled) && results.Contains(ShowTimeDubType.OriginalVersion))
            {
                return ShowTimeDubType.Subtitled;
            }
            // Otherwise return the first resultq
            return results[0];
        }

        private static List<T> FindMatchingDictionaryKeys<T>(string haystack, Dictionary<T, string[]> dictionary) where T : notnull
        {

            var matches = new List<T>();

            foreach (var (key, needle) in dictionary)
            {
                if (Array.Exists(needle, n => !string.IsNullOrWhiteSpace(n) && haystack.Contains(n, StringComparison.OrdinalIgnoreCase)))
                {
                    matches.Add(key);
                }
            }

            return matches;
        }
        private static T FindMatchingDictionaryKey<T>(string needle, Dictionary<T, string[]> dictionary, T defaultValue) where T : notnull
        {
            foreach (var (key, value) in dictionary)
            {
                if (value.Any(v => !string.IsNullOrWhiteSpace(v) && needle.Contains(v, StringComparison.OrdinalIgnoreCase)))
                {
                    return key;
                }
            }

            return defaultValue;
        }
    }
}

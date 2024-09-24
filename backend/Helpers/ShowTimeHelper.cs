using backend.Models;

namespace backend.Helpers
{
    public static class ShowTimeHelper
    {
        private static readonly Dictionary<ShowTimeDubType, string[]> _showTimeDubTypeMap = new()
        {
            { ShowTimeDubType.OriginalVersion, ["OV", "OF", "Original Version", "Originalversion", "Originalfassung", "Original Fassung"] },
            { ShowTimeDubType.Subtitled, ["OmU", "OmeU", "OmdU", "Untertitel", "OmenglU"] },
            { ShowTimeDubType.Regular, [""] },
        };

        private static readonly Dictionary<ShowTimeLanguage, string[]> _showTimeLanguageMap = new()
        {
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
            { ShowTimeLanguage.German, ["Deutsch","de","deu", "dt.", "DK"] },
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
            type = type.Trim().Replace(".", " ").Replace("(", " ").Replace(")", " ");

            var results = FindMatchingDictionaryKeys(type, _showTimeDubTypeMap);

            // // If the reults contain OriginalVersion, check the position of OF in the string. 
            // // Some cinemas use OF as indicator for OriginalVersion,
            // // but in many English movie titles OF appears in the title itself.
            // // Checking if the position of OF is in the last third of the string
            // // is a simple heuristic to determine if it is used as OriginalVersion indicator. 
            // if (results.Contains(ShowTimeDubType.OriginalVersion) && type.Length > 16)
            // {
            //     var ofPosition = type.IndexOf("OF", StringComparison.OrdinalIgnoreCase);
            //     var lastSixth = type.Length * .85;
            //     if (ofPosition < lastSixth)
            //     {
            //         results.Remove(ShowTimeDubType.OriginalVersion);
            //     }
            // }

            // If both OV and OmU are found, return OmU, as it is more specific
            if (results.Contains(ShowTimeDubType.Subtitled) && results.Contains(ShowTimeDubType.OriginalVersion))
            {
                return ShowTimeDubType.Subtitled;
            }

            // If no specific type is found, return Regular
            if (results.Count == 0)
            {
                return ShowTimeDubType.Regular;
            }

            // Otherwise return the first result
            return results[0];
        }

        private static List<T> FindMatchingDictionaryKeys<T>(string haystack, Dictionary<T, string[]> dictionary) where T : notnull
        {
            var matches = new List<T>();

            foreach (var (key, needle) in dictionary)
            {
                // If the needle is an exact match for the haystack, return the key
                if (Array.Exists(needle, n => !string.IsNullOrWhiteSpace(n) && haystack.Equals(n, StringComparison.OrdinalIgnoreCase)))
                {
                    return [key];
                }
                // If the needle is contained in the haystack, add the key to the matches
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
                // If the needle is an exact match for the haystack, return the key
                if (Array.Exists(value, v => !string.IsNullOrWhiteSpace(v) && needle.Equals(v, StringComparison.OrdinalIgnoreCase)))
                {
                    return key;
                }
            }
            // If no exact match is found, check if the haystack contains one of the values
            foreach (var (key, value) in dictionary)
            {
                if (Array.Exists(value, v => !string.IsNullOrWhiteSpace(v) && needle.Contains(v, StringComparison.OrdinalIgnoreCase)))
                {
                    return key;
                }
            }

            return defaultValue;
        }
    }
}

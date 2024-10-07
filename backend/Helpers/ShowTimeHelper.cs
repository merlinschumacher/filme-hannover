using backend.Models;

namespace backend.Helpers
{
    public static class ShowTimeHelper
    {
        private static readonly Dictionary<ShowTimeDubType, string[]> _showTimeDubTypeMap = new()
        {
            { ShowTimeDubType.OriginalVersion, [" OV ", "Original Version", "Originalversion", "Originalfassung", "Original Fassung"] },
            { ShowTimeDubType.Subtitled, ["OmU","OmdU", "Untertitel"] },
            { ShowTimeDubType.SubtitledEnglish, ["OmeU", "OmenglU", "OmU engl. UT"] },
            { ShowTimeDubType.Regular, [""] },
        };

        private static readonly Dictionary<ShowTimeLanguage, string[]> _showTimeLanguageMap = new()
        {
            { ShowTimeLanguage.Danish, ["Dänisch", "dän", "DK"] },
            { ShowTimeLanguage.English, ["Englisch","English"," eng."," engl.","EN","GB","UK","US", "USA"] },
            { ShowTimeLanguage.French, ["Französisch","franz","frnz","frz","FR"] },
            { ShowTimeLanguage.Spanish, ["Spanisch"," span.","SP"," ES."] },
            { ShowTimeLanguage.Italian, ["Italienisch","ital","IT"] },
            { ShowTimeLanguage.Turkish, ["Türkisch", "türk","trk", " TR"] },
            { ShowTimeLanguage.Russian, ["Russisch", "russ"] },
            { ShowTimeLanguage.Japanese, ["Japanisch", "jap", "JP", "JA"] },
            { ShowTimeLanguage.Korean, ["Koreanisch", "kor", "KO"] },
            { ShowTimeLanguage.Hindi, ["Hindi", " hind.", " hin." ] },
            { ShowTimeLanguage.Polish, ["Polnisch", "poln.", "PL"] },
            { ShowTimeLanguage.German, ["Deutsch"," de."," deu.", "dt.", "DE"] },
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
            return type switch
            {
                ShowTimeDubType.OriginalVersion => "OV",
                ShowTimeDubType.Subtitled => "OmU",
                ShowTimeDubType.SubtitledEnglish => "OmeU",
                _ => string.Empty,
            };
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

            CheckForOF(type, results);

            // If both OV and OmU/OmeU are found, remove OV as it is less specific
            if ((results.Contains(ShowTimeDubType.Subtitled)
                || results.Contains(ShowTimeDubType.SubtitledEnglish)) && results.Contains(ShowTimeDubType.OriginalVersion))
            {
                results.Remove(ShowTimeDubType.OriginalVersion);
            }

            // If both OmU and OmeU are found, remove OmU as it is less specific
            if (results.Contains(ShowTimeDubType.Subtitled) && results.Contains(ShowTimeDubType.SubtitledEnglish))
            {
                results.Remove(ShowTimeDubType.Subtitled);
            }

            // If no type is found, add Regular as default
            if (results.Count == 0)
            {
                results.Add(ShowTimeDubType.Regular);
            }

            // Otherwise return the first result
            return results[0];
        }

        /// <summary>
        /// OF is a common abbreviation for OriginalFassung in German cinemas
        /// but in many English movie titles OF appears in the title itself.
        /// And sometimes the movie title is in English, uppercase and contains OF.
        /// We'll check if string is uppercase and contains OF, 
        /// This is a simple heuristic to determine if it is used as OriginalVersion indicator. 
        /// Check if the ratio of uppercase letters is higher than 70%.
        /// If so, we'll check if OF is in the last third of the string, as OF is usually at the end of the title
        /// </summary>
        private static void CheckForOF(string type, List<ShowTimeDubType> results)
        {
            var upperCaseRatio = type.Count(c => char.IsLetter(c) && char.IsUpper(c)) / (double)type.Length;
            if (upperCaseRatio > .7)
            {
                var ofPosition = type.IndexOf(" OF ", StringComparison.CurrentCulture);
                var lastSixth = type.Length * .85;
                if (ofPosition < lastSixth)
                {
                    results.Add(ShowTimeDubType.OriginalVersion);
                }
            }
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

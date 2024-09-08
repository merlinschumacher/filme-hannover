namespace backend.Models
{
    public enum ShowTimeType
    {
        Regular,
        OriginalVersion,
        Subtitled,
    }

    public enum ShowTimeLanguage
    {
        German,
        English,
        Danish,
        French,
        Spanish,
        Italian,
        Russian,
        Turkish,
        Japanese,
        Korean,
        Hindi,
        Other,
        Unknown,
    }

    public static class ShowTimeHelper
    {
        private static readonly Dictionary<ShowTimeType, string[]> _showTimeTypeMap = new()
        {
            { ShowTimeType.Regular, [""] },
            { ShowTimeType.OriginalVersion, ["OV", "OF", "Original Version", "Originalversion", "Originalfassung", "Original Fassung"] },
            { ShowTimeType.Subtitled, ["OmU", "OmeU", "OmdU", "Untertitel"] },
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
            { ShowTimeLanguage.Other, ["Andere", "Verschiedene", "versch.", "div.", "Malayalam", "Filipino", "Georgisch", "georg." ]},
            { ShowTimeLanguage.Unknown, ["Unbekannt"] }
        };

        public static string GetLanguageName(ShowTimeLanguage language)
        {
            var values = _showTimeLanguageMap[language];
            return values[0];
        }

        public static string GetTypeName(ShowTimeType type)
        {
            var values = _showTimeTypeMap[type];
            return values[0];
        }

        public static ShowTimeLanguage? TryGetLanguage(string language, ShowTimeLanguage? def)
        {
            var result = GetMatchingDictionaryKey(language, _showTimeLanguageMap, ShowTimeLanguage.Unknown);
            if (result == ShowTimeLanguage.Unknown)
            {
                return def;
            }
            return result;
        }

        public static ShowTimeLanguage GetLanguage(string language)
        {
            return GetMatchingDictionaryKey(language, _showTimeLanguageMap, ShowTimeLanguage.German);
        }

        public static ShowTimeType GetType(string type)
        {
            type = type.Trim().Replace(".", null).Replace("(", null).Replace(")", null);

            return GetMatchingDictionaryKey(type, _showTimeTypeMap, ShowTimeType.Regular);
        }

        private static T GetMatchingDictionaryKey<T>(string needle, Dictionary<T, string[]> dictionary, T defaultValue) where T : notnull
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

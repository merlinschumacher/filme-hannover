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
        Danish,
        German,
        English,
        French,
        Spanish,
        Italian,
        Georgian,
        Russian,
        Turkish,
        Mayalam,
        Japanese,
        Miscellaneous,
        Other,
        Filipino,
        Hindi,
    }

    public static class ShowTimeHelper
    {
        private static readonly Dictionary<string, ShowTimeLanguage?> _showTimeLanguageMap = new(StringComparer.OrdinalIgnoreCase)

        {
            { "Dänisch", ShowTimeLanguage.Danish },
            { "DK", ShowTimeLanguage.Danish },
            { "dän", ShowTimeLanguage.Danish },
            { "dt", ShowTimeLanguage.German },
            { "de", ShowTimeLanguage.German },
            { "deu", ShowTimeLanguage.German },
            { "eng", ShowTimeLanguage.English },
            { "EN", ShowTimeLanguage.English },
            { "GB", ShowTimeLanguage.English },
            { "UK", ShowTimeLanguage.English },
            { "US", ShowTimeLanguage.English },
            { "engl", ShowTimeLanguage.English },
            { "franz", ShowTimeLanguage.French },
            { "frnz", ShowTimeLanguage.French },
            { "frz", ShowTimeLanguage.French },
            { "FR", ShowTimeLanguage.French },
            { "span", ShowTimeLanguage.Spanish },
            { "SP", ShowTimeLanguage.Spanish },
            { "ES", ShowTimeLanguage.Spanish },
            { "ital", ShowTimeLanguage.Italian},
            { "IT", ShowTimeLanguage.Italian},
            { "jap", ShowTimeLanguage.Japanese},
            { "JP", ShowTimeLanguage.Japanese},
            { "JA", ShowTimeLanguage.Japanese},
            { "georg", ShowTimeLanguage.Georgian },
            { "russ", ShowTimeLanguage.Russian },
            { "türk", ShowTimeLanguage.Turkish },
            { "TR", ShowTimeLanguage.Turkish },
            { "div", ShowTimeLanguage.Miscellaneous },
            { "versch", ShowTimeLanguage.Miscellaneous },
            { "sonst", ShowTimeLanguage.Other },
            { "keine", ShowTimeLanguage.Other },
            { "Deutsch", ShowTimeLanguage.German },
            { "Englisch", ShowTimeLanguage.English },
            { "English", ShowTimeLanguage.English },
            { "Französisch", ShowTimeLanguage.French },
            { "Spanisch", ShowTimeLanguage.Spanish },
            { "Italienisch", ShowTimeLanguage.Italian },
            { "Georgisch", ShowTimeLanguage.Georgian },
            { "Russisch", ShowTimeLanguage.Russian },
            { "Türkisch", ShowTimeLanguage.Turkish },
            { "Mayalam", ShowTimeLanguage.Mayalam },
            { "Japanisch", ShowTimeLanguage.Japanese},
            { "Verschiedene", ShowTimeLanguage.Miscellaneous },
            { "Sonstige", ShowTimeLanguage.Other },
            { "Hindi", ShowTimeLanguage.Hindi },
            { "hin", ShowTimeLanguage.Hindi },
            { "hind", ShowTimeLanguage.Hindi },
            {"PH", ShowTimeLanguage.Filipino }
        };

        public static ShowTimeLanguage? TryGetLanguage(string language, ShowTimeLanguage? def = ShowTimeLanguage.German)
        {
            return GetMatchingDictionaryVaue(language, _showTimeLanguageMap, def);
        }

        public static ShowTimeLanguage GetLanguage(string language)
        {
            var result = GetMatchingDictionaryVaue(language, _showTimeLanguageMap, ShowTimeLanguage.German);
            return result ?? ShowTimeLanguage.German;
        }

        private static readonly Dictionary<ShowTimeType, string[]> _showTimeTypeMap = new()
        {
            { ShowTimeType.OriginalVersion, ["OV", "OF", "Original Version", "Originalversion", "Originalfassung", "Original Fassung"] },
            { ShowTimeType.Subtitled, ["OmU", "OmeU", "OmdU", "Untertitel"] },
        };

        public static ShowTimeType GetType(string type)
        {
            type = type.Trim().Replace(".", null).Replace("(", null).Replace(")", null);

            return GetMatchingDictionaryKey(type, _showTimeTypeMap, ShowTimeType.Regular);
        }

        private static T GetMatchingDictionaryKey<T>(string needle, Dictionary<T, string[]> dictionary, T defaultValue)
        {
            foreach (var (key, value) in dictionary)
            {
                if (value.Contains(needle, StringComparer.OrdinalIgnoreCase))
                {
                    return key;
                }
            }

            return defaultValue;
        }

        private static T GetMatchingDictionaryVaue<T>(string needle, Dictionary<string, T> dictionary, T defaultValue)
        {
            needle = needle.Trim().Replace(".", null).Replace("(", null).Replace(")", null);
            if (dictionary.TryGetValue(needle, out var result))
            {
                return result;
            }

            foreach (var (key, value) in dictionary)
            {
                if (needle.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
            return defaultValue;
        }
    }
}

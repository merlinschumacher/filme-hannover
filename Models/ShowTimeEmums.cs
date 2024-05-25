namespace kinohannover.Models
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
    }

    public static class ShowTimeHelper
    {
        private static readonly Dictionary<string, ShowTimeLanguage> ShowTimeLanguageMap = new(StringComparer.OrdinalIgnoreCase)

        {
            { "dt", ShowTimeLanguage.German },
            { "deu", ShowTimeLanguage.German },
            { "eng", ShowTimeLanguage.English },
            { "engl", ShowTimeLanguage.English },
            { "franz", ShowTimeLanguage.French },
            { "frnz", ShowTimeLanguage.French },
            { "frz", ShowTimeLanguage.French },
            { "span", ShowTimeLanguage.Spanish },
            { "ital", ShowTimeLanguage.Italian},
            { "jap", ShowTimeLanguage.Japanese},
            { "georg", ShowTimeLanguage.Georgian },
            { "russ", ShowTimeLanguage.Russian },
            { "türk", ShowTimeLanguage.Turkish },
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
        };

        public static ShowTimeLanguage GetLanguage(string language)
        {
            return GetMatchingDictionaryVaue(language, ShowTimeLanguageMap, ShowTimeLanguage.German);
        }

        private static readonly Dictionary<string, ShowTimeType> ShowTimeTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "OV", ShowTimeType.OriginalVersion },
            { "OmU", ShowTimeType.Subtitled },
            { "Original Version", ShowTimeType.OriginalVersion },
            { "Originalversion", ShowTimeType.OriginalVersion },
            { "Untertitel", ShowTimeType.Subtitled },
        };

        public static ShowTimeType GetType(string type)
        {
            return GetMatchingDictionaryVaue(type, ShowTimeTypeMap, ShowTimeType.Regular);
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

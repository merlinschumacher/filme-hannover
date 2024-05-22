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
            { "frz", ShowTimeLanguage.French },
            { "span", ShowTimeLanguage.Spanish },
            { "ital", ShowTimeLanguage.Italian},
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
            { "Verschiedene", ShowTimeLanguage.Miscellaneous },
            { "Sonstige", ShowTimeLanguage.Other },
        };

        public static ShowTimeLanguage GetLanguage(string language)
        {
            if (ShowTimeLanguageMap.TryGetValue(language, out var showTimeLanguage))
            {
                return showTimeLanguage;
            }
            return ShowTimeLanguage.German;
        }

        private static readonly Dictionary<string, ShowTimeType> ShowTimeTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "OV", ShowTimeType.OriginalVersion },
            { "(OV)", ShowTimeType.OriginalVersion },
            { "OmU", ShowTimeType.Subtitled },
            { "(OmU)", ShowTimeType.Subtitled },
            { "Original Version", ShowTimeType.OriginalVersion },
            { "Originalversion", ShowTimeType.OriginalVersion },
            { "Untertitel", ShowTimeType.Subtitled },
        };

        public static ShowTimeType GetType(string type)
        {
            if (ShowTimeTypeMap.TryGetValue(type, out var showTimeType))
            {
                return showTimeType;
            };
            return ShowTimeType.Regular;
        }
    }
}

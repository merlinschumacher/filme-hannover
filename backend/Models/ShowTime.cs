namespace backend.Models
{
    public class ShowTime
    {
        public int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public required Movie Movie { get; set; }
        public required Cinema Cinema { get; set; }
        public ShowTimeType Type { get; set; } = ShowTimeType.Regular;
        public ShowTimeLanguage Language { get; set; } = ShowTimeLanguage.German;
        public Uri? Url { get; set; }
        public string? SpecialEvent { get; set; }

        public string GetShowTimeSuffix()
        {
            if (Type == ShowTimeType.Regular && Language == ShowTimeLanguage.German)
            {
                return "";
            }

            var typeString = ShowTimeHelper.GetTypeName(Type);
            var languageString = ShowTimeHelper.GetLanguageName(Language);

            return $"({typeString}/{languageString})";
        }

        public override string ToString()
        {
            return $"{Movie} at {StartTime:u} at {Cinema}";
        }
    }
}

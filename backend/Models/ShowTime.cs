using backend.Helpers;

namespace backend.Models
{
    public class ShowTime
    {
        public int Id { get; set; }
        public required DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public required Movie Movie { get; set; }
        public required Cinema Cinema { get; set; }
        public ShowTimeDubType DubType { get; set; } = ShowTimeDubType.Regular;
        public ShowTimeLanguage Language { get; set; } = ShowTimeLanguage.German;
        public Uri? Url { get; set; }
        public string? SpecialEvent { get; set; }

        public string GetShowTimeSuffix()
        {
            if (DubType == ShowTimeDubType.Regular && Language == ShowTimeLanguage.German)
            {
                return string.Empty;
            }

            var typeString = ShowTimeHelper.GetTypeName(DubType);
            var languageString = ShowTimeHelper.GetLanguageName(Language);

            return $"({typeString}/{languageString})";
        }

        public override string ToString()
        {
            return $"{Movie} at {StartTime:u} at {Cinema}";
        }
    }
}

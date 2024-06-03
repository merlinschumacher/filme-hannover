namespace kinohannover.Scrapers.Cinemaxx
{
    public class EventInfo
    {
        public string event_id { get; set; }
        public string event_name { get; set; }
    }

    public class FilmParam
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }

    public class Name
    {
        public string name { get; set; }
        public string @class { get; set; }
        public string short_name { get; set; }
    }

    public class PromoLabels
    {
        public List<Name> names { get; set; }
        public string position { get; set; }
        public bool isborder { get; set; }
    }

    public class CinemaxxRoot
    {
        public List<WhatsOnAlphabeticFilm> WhatsOnAlphabeticFilms { get; set; }
        public object cdata { get; set; }
    }

    public class Tag
    {
        public string link { get; set; }
        public string name { get; set; }
        public string @class { get; set; }
        public bool target_blank { get; set; }
    }

    public class WhatsOnAlphabeticCinema
    {
        public List<WhatsOnAlphabeticCinema> WhatsOnAlphabeticCinemas { get; set; }
        public string DayTitle { get; set; }
        public List<WhatsOnAlphabeticShedule> WhatsOnAlphabeticShedules { get; set; }
        public string CinemaName { get; set; }
        public string CinemaId { get; set; }
        public List<object> PromoTypes { get; set; }
    }

    public class WhatsOnAlphabeticFilm
    {
        public List<WhatsOnAlphabeticCinema> WhatsOnAlphabeticCinemas { get; set; }
        public string Synopsis { get; set; }
        public string ShortSynopsis { get; set; }
        public string RankValue { get; set; }
        public string RankVotes { get; set; }
        public PromoLabels PromoLabels { get; set; }
        public bool IsEvent { get; set; }
        public string Title { get; set; }
        public string FilmId { get; set; }
        public string SortField { get; set; }
        public string SortFieldCommingSoon { get; set; }
        public string Poster { get; set; }
        public string TrailerUrl { get; set; }
        public object TrailerType { get; set; }
        public List<FilmParam> FilmParams { get; set; }
        public List<Tag> tags { get; set; }
        public string FilmUrl { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public object HeroMobileImage { get; set; }
        public string WantSee { get; set; }
        public bool ShowWantSee { get; set; }
        public bool RateFilmAvaiable { get; set; }
        public string PegiClass { get; set; }
        public string CertificateAge { get; set; }
        public string PegiHref { get; set; }
    }

    public class WhatsOnAlphabeticShedule
    {
        public string Time { get; set; }
        public string BookingLink { get; set; }
        public string VersionTitle { get; set; }
        public bool FirstClass { get; set; }
        public List<EventInfo> EventInfo { get; set; }
        public string ScreenNumber { get; set; }
        public List<object> PromoTypes { get; set; }
    }
}

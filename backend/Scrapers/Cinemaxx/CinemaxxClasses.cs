namespace kinohannover.Scrapers.Cinemaxx;

public class CinemaxxRoot
{
    public List<WhatsOnAlphabeticFilm> WhatsOnAlphabeticFilms { get; set; }
}

public class WhatsOnAlphabeticCinema
{
    public List<WhatsOnAlphabeticCinema> WhatsOnAlphabeticCinemas { get; set; }
    public List<WhatsOnAlphabeticShedule> WhatsOnAlphabeticShedules { get; set; }
}

public class WhatsOnAlphabeticFilm
{
    public List<WhatsOnAlphabeticCinema> WhatsOnAlphabeticCinemas { get; set; }
    public string Title { get; set; }
    public IEnumerable<FilmParam> FilmParams { get; set; } = [];
    public string FilmUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string CertificateAge { get; set; }
}

public class WhatsOnAlphabeticShedule
{
    public string Time { get; set; }
    public string BookingLink { get; set; }
    public string VersionTitle { get; set; }
}

public class FilmParam
{
    public string Title { get; set; }
    public string Link { get; set; } = string.Empty;
}

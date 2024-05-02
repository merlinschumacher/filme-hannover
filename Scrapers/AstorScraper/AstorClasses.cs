using Newtonsoft.Json;

namespace kinohannover.Scrapers.AstorScraper
{
    public class Award
    {
        public string award { get; set; }
        public string image { get; set; }
        public int sort { get; set; }
    }

    public class Descriptors
    {
        [JsonProperty("belastende Szenen")]
        public string belastendeSzenen { get; set; }

        public string Drogenkonsum { get; set; }
        public string Sexualitt { get; set; }
        public string Sprache { get; set; }
        public string Gewalt { get; set; }
        public string Bedrohung { get; set; }
        public string Verletzung { get; set; }

        [JsonProperty("belastende Themen")]
        public string belastendeThemen { get; set; }

        public string Selbstschdigung { get; set; }
    }

    public class EventInfo
    {
        public string crypt_id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string category { get; set; }
        public string color_decoration { get; set; }
        public string color_decoration_header { get; set; }
        public string color_decoration_performance { get; set; }
        public string color_decoration_performance_hover { get; set; }
        public int type { get; set; }
        public Poster poster { get; set; }
    }

    public class Events
    {
        public List<EventType1> type_1 { get; set; }
        public List<object> type_2 { get; set; }
    }

    public class Performance
    {
        public string crypt_id { get; set; }
        public string movie_crypt_id { get; set; }
        public EventInfo event_info { get; set; }
        public string oid { get; set; }
        public DateTime begin { get; set; }
        public DateTime end { get; set; }
        public string slug { get; set; }
        public string auditorium { get; set; }
        public string title { get; set; }
        public string release_type { get; set; }
        public string release_type_image { get; set; }
        public string release_type_crypt_id { get; set; }
        public string auditorium_crypt_id { get; set; }
        public int fsk { get; set; }
        public int time { get; set; }
        public bool bookable { get; set; }
        public bool reservable { get; set; }
        public bool is_assigned_seating { get; set; }
        public bool is_open_air_cinema { get; set; }
        public bool is_omu { get; set; }
        public string reservable_message { get; set; }
        public List<string> filters { get; set; }
        public string filter_type { get; set; }
        public Restriction restriction { get; set; }
        public int max_booking_time { get; set; }
        public bool needs_registration { get; set; }
        public int workload { get; set; }
        public bool hide_link_on_movie_detail { get; set; }
        public string link_label { get; set; }
        public string link_style { get; set; }
        public string headline { get; set; }
        public string teaser { get; set; }
        public string language { get; set; }
        public string lang_icon { get; set; }
        public int seating_area_usage_1 { get; set; }
        public int seating_area_usage_2 { get; set; }
        public int seating_area_usage_3 { get; set; }
        public bool is_cancelable { get; set; }
        public bool? is_ov { get; set; }
    }

    public class Poster
    {
        public string small { get; set; }
        public string medium { get; set; }
        public string large { get; set; }
        public string original { get; set; }
        public string title { get; set; }
        public string copyright { get; set; }
    }

    public class Restriction
    {
        public int max_seats_per_selection { get; set; }
        public bool? hide_in_program { get; set; }
    }

    public class AstorMovie
    {
        public string crypt_id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string description_short { get; set; }
        public string description_long { get; set; }
        public Poster poster { get; set; }
        public string genre { get; set; }
        public string country { get; set; }
        public List<string> filters { get; set; }
        public int minutes { get; set; }
        public int fsk { get; set; }
        public int year { get; set; }
        public DateTime date_insert { get; set; }
        public DateTime date_start { get; set; }
        public DateTime date_presale { get; set; }
        public DateTime date_sort { get; set; }
        public bool is_not_cancelable { get; set; }
        public List<Performance> performances { get; set; }
        public List<Scene> scenes { get; set; }
        public Events events { get; set; }
        public bool show { get; set; }
        public Descriptors descriptors { get; set; }
        public List<Trailer> trailers { get; set; }
        public List<Award> awards { get; set; }
        public bool? permit_display_presale_date { get; set; }
    }

    public class Scene
    {
        public string small { get; set; }
        public string medium { get; set; }
        public string large { get; set; }
        public string original { get; set; }
        public string title { get; set; }
        public string copyright { get; set; }
    }

    public class Trailer
    {
        public string crypt_id { get; set; }
        public string name { get; set; }
        public string url1080 { get; set; }
        public string url720 { get; set; }
        public string url640 { get; set; }
        public string url480 { get; set; }
        public int duration { get; set; }
        public int rating { get; set; }
        public DateTime publish { get; set; }
    }

    public class EventType1
    {
        public string crypt_id { get; set; }
        public string name { get; set; }
        public string teaser { get; set; }
        public string slug { get; set; }
        public string color_decoration { get; set; }
        public string color_list_box_background { get; set; }
        public string color_list_box_text { get; set; }
        public string file_signet { get; set; }
        public string file_partner { get; set; }
        public string file_list_box { get; set; }
        public int type { get; set; }
        public int sort { get; set; }
        public bool is_sneak { get; set; }
        public string category { get; set; }
        public List<object> movies { get; set; }
        public List<object> rawmovie_ids { get; set; }
        public string file_teaser_desktop { get; set; }
        public string file_teaser_mobile { get; set; }
    }
}

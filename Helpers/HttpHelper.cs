using HtmlAgilityPack;
using Newtonsoft.Json;

namespace kinohannover.Helpers
{
    public class HttpHelper
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<HtmlDocument> GetHtmlDocumentAsync(string url)
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        public static Uri? BuildAbsoluteUrl(string? url, string baseUrl = "")
        {
            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var relativeUri);
            Uri.TryCreate(url, UriKind.Absolute, out var baseUri);
            if (relativeUri == null)
            {
                return null;
            }

            if (relativeUri.IsAbsoluteUri)
            {
                return relativeUri;
            }
            if (baseUri == null)
            {
                baseUri = new Uri(baseUrl);
            }

            Uri.TryCreate(baseUri, relativeUri, out var result);

            return result;
        }

        public static async Task<T?> GetJsonAsync<T>(string url)
        {
            var jsonString = await _httpClient.GetStringAsync(url);

            T? jsonEntity = JsonConvert.DeserializeObject<T>(jsonString);
            return jsonEntity;
        }
    }
}

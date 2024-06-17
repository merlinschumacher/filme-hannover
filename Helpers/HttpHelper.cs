using HtmlAgilityPack;
using Newtonsoft.Json;

namespace kinohannover.Helpers
{
    public static class HttpHelper
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<HtmlDocument> GetHtmlDocumentAsync(Uri uri, StringContent? content = null)
        {
            string? html = await GetHttpContentAsync(uri, content);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        public static async Task<string?> GetHttpContentAsync(Uri uri, StringContent? content = null)
        {
            if (content is null)
            {
                return await _httpClient.GetStringAsync(uri);
            }
            else
            {
                ArgumentNullException.ThrowIfNull(content);
                var response = await _httpClient.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<T?> GetJsonAsync<T>(Uri uri)
        {
            var jsonString = await GetHttpContentAsync(uri);
            if (jsonString is null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
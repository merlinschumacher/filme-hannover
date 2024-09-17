using HtmlAgilityPack;
using Ical.Net;
using Newtonsoft.Json;

namespace backend.Helpers
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
            // If not in debug mode, add a delay to prevent spamming the server
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // Delay to prevent spamming the server
                var randomDelay = new Random().Next(100, 2000);
                Task.Delay(randomDelay).Wait();
            }

            // If content is null, send a GET request, otherwise send a POST request
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

            var result = JsonConvert.DeserializeObject<T>(jsonString);
            return result;
        }

        public static async Task<Calendar?> GetCalendarAsync(Uri icalUri)
        {
            try
            {
                var icalText = await GetHttpContentAsync(icalUri);
                return Calendar.Load(icalText);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

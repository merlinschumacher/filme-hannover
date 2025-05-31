using HtmlAgilityPack;
using Ical.Net;
using Newtonsoft.Json;

namespace backend.Helpers;

public static class HttpHelper
{
#pragma warning disable S1075 // URIs should not be hardcoded
	private const string _uaListUrl = "https://cdn.jsdelivr.net/gh/microlinkhq/top-user-agents@master/src/desktop.json";
#pragma warning restore S1075 // URIs should not be hardcoded
	private static readonly HttpClient _httpClient = new()
	{
		Timeout = TimeSpan.FromSeconds(30),
	};

	static HttpHelper()
	{
		// Set the default user agent for all HttpClient instances
		// Some cinemas block requests from unknown user agents or 
		// break if no user agent is set
		var userAgent = GetRandomUserAgentAsync().Result;
		_httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
	}

	private static async Task<string> GetRandomUserAgentAsync()
	{
		using var client = new HttpClient();
		var response = await client.GetAsync(_uaListUrl);

		var json = await response.Content.ReadAsStringAsync();
		var userAgents = JsonConvert.DeserializeObject<List<string>>(json);
		if (userAgents is null || userAgents.Count == 0)
		{
			throw new OperationCanceledException("Failed to load user agents");
		}

		var random = new Random();
		var randomIndex = random.Next(userAgents.Count);
		return userAgents[randomIndex];
	}

	public static async Task<HtmlDocument> PostFormAsync(Uri uri, Dictionary<string, string> formValues)
	{
		var content = new FormUrlEncodedContent(formValues);
		string? html = await LoadHttpContentAsync(uri, content);
		var doc = new HtmlDocument();
		doc.LoadHtml(html ?? "");
		return doc;
	}
	public static async Task<HtmlDocument> GetHtmlDocumentAsync(Uri uri, StringContent? content = null)
	{
		string? html = await GetHttpContentAsync(uri, content);
		var doc = new HtmlDocument();
		doc.LoadHtml(html ?? "");
		return doc;
	}

	public static async Task<string?> GetHttpContentAsync(Uri uri, StringContent? content = null)
	{
		return await LoadHttpContentAsync(uri, content);
	}

	private static async Task<string?> LoadHttpContentAsync(Uri uri, ByteArrayContent? content = null)
	{
		// If not in debug mode, add a delay to prevent spamming the server
		if (!System.Diagnostics.Debugger.IsAttached)
		{
			// Delay to prevent spamming the server, as this may trigger anti-spam measures
			var randomDelay = new Random().Next(50, 1000);
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

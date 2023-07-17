using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace RenderingRSS.Pages
{
    public class IndexModel : PageModel
    {
        private const int PageSize = 10;

        private readonly IHttpClientFactory _httpClientFactory;

        public List<RssItem> RssItems { get; set; } = new();
        public int CurrentPage { get; set; } = 0;
        public int TotalPages { get; set; } = 0;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var opmlResponse = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");

            if (opmlResponse.IsSuccessStatusCode)
            {
                var opmlContent = await opmlResponse.Content.ReadAsStringAsync();
                var feedUrls = ParseOpmlContent(opmlContent);

                var tasks = feedUrls.Select(url => FetchAndParseRssFeedAsync(httpClient, url));
                var rssResponses = await Task.WhenAll(tasks);
                RssItems = rssResponses.SelectMany(r => r).ToList();

                TotalPages = (int)Math.Ceiling((double)RssItems.Count / PageSize);
                CurrentPage = pageNumber;

                RssItems = RssItems
                    .Skip((pageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                return Page();
            }
            else
            {
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostToggleFavorite(string link, int pageNumber)
        {
            var favoriteFeeds = GetFavoriteFeedsFromCookie();

            if (!string.IsNullOrEmpty(link))
            {
                if (favoriteFeeds.Contains(link))
                {
                    favoriteFeeds.Remove(link);
                }
                else
                {
                    favoriteFeeds.Add(link);
                }

                SetFavoriteFeedsInCookie(favoriteFeeds);
            }

            return RedirectToPage("/Index", new { pageNumber });
        }

        private List<string> GetFavoriteFeedsFromCookie()
        {
            var favoriteFeedsCookie = HttpContext.Request.Cookies["FavoriteFeeds"];
            if (!string.IsNullOrEmpty(favoriteFeedsCookie))
            {
                return favoriteFeedsCookie.Split(',').ToList();
            }
            return new List<string>();
        }

        private void SetFavoriteFeedsInCookie(List<string> favoriteFeeds)
        {
            if (favoriteFeeds.Count > 0)
            {
                var favoriteFeedsCookieValue = string.Join(',', favoriteFeeds);
                HttpContext.Response.Cookies.Append("FavoriteFeeds", favoriteFeedsCookieValue);
            }
        }

        private List<string> ParseOpmlContent(string opmlContent)
        {
            var feedUrls = new List<string>();

            var doc = XDocument.Parse(opmlContent);
            var outlines = doc.Descendants("outline");

            foreach (var outline in outlines)
            {
                var xmlUrl = outline.Attribute("xmlUrl")?.Value;
                if (!string.IsNullOrEmpty(xmlUrl))
                {
                    feedUrls.Add(xmlUrl);
                }
            }

            return feedUrls;
        }

        private async Task<List<RssItem>> FetchAndParseRssFeedAsync(HttpClient httpClient, string url)
        {
            var rssItemList = new List<RssItem>();

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var xmlContent = await response.Content.ReadAsStringAsync();
                rssItemList = ParseXmlContent(xmlContent);
            }

            return rssItemList;
        }

        private List<RssItem> ParseXmlContent(string xmlContent)
        {
            var rssItemList = new List<RssItem>();
            var doc = XDocument.Parse(xmlContent);
            var items = doc.Descendants("item");

            foreach (var item in items)
            {
                var rssItem = new RssItem
                {
                    Description = item.Element("description")?.Value,
                    PubDate = item.Element("pubDate")?.Value,
                    Link = item.Element("link")?.Value,
                    IsFavorite = GetFavoriteFeedsFromCookie().Contains(item.Element("link")?.Value)
                };

                rssItemList.Add(rssItem);
            }

            return rssItemList;
        }
    }

    public class RssItem
    {
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
        public bool IsFavorite { get; set; }
    }
}

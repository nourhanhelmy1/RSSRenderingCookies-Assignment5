using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace RenderingRSS.Pages
{
    public class FavoriteFeedsModel : PageModel
    {
        private readonly IDistributedCache _cache;

        public List<RssItem> FavoriteRssItems { get; private set; } = new();
        public int PageSize { get; private set; } = 4;
        public int CurrentPage { get; private set; } = 0;
        public int TotalPages { get; private set; } = 0;

        public FavoriteFeedsModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task OnGetAsync(int currentPage = 1, int pageSize = 5)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;

            string userFavoriteFeeds = HttpContext.Request.Cookies["FavoriteFeeds"];

            if (!string.IsNullOrEmpty(userFavoriteFeeds) && userFavoriteFeeds != "[]")
            {
                List<string> favoriteFeeds = GetFavoriteFeedsFromCookie();

                string cachedRssItemsJson = await _cache.GetStringAsync("starredFeeds");

                if (!string.IsNullOrEmpty(cachedRssItemsJson))
                {
                    try
                    {
                        List<RssItem> cachedRssItems = JsonSerializer.Deserialize<List<RssItem>>(cachedRssItemsJson);
                        FavoriteRssItems = cachedRssItems
                            .Where(item => favoriteFeeds.Contains(item.Link) && item.IsFavorite)
                            .ToList();
                    }
                    catch (JsonException ex)
                    {
                        FavoriteRssItems = null;
                    }
                }
                else
                {
                    FavoriteRssItems = null;
                }
            }
            else
            {
                FavoriteRssItems = null;
            }

            if (FavoriteRssItems == null)
            {
                string starredFeedsJson = await _cache.GetStringAsync("starredFeeds");

                if (!string.IsNullOrEmpty(starredFeedsJson))
                {
                    try
                    {
                        List<RssItem> starredFeeds = JsonSerializer.Deserialize<List<RssItem>>(starredFeedsJson);
                        FavoriteRssItems = starredFeeds
                            .Where(item => item.IsFavorite)
                            .ToList();
                    }
                    catch (JsonException ex)
                    {
                        FavoriteRssItems = new List<RssItem>();
                    }
                }
                else
                {
                    FavoriteRssItems = new List<RssItem>();
                }
            }

            TotalPages = (int)Math.Ceiling(FavoriteRssItems.Count / (double)PageSize);
            int skip = (CurrentPage - 1) * PageSize;
            FavoriteRssItems = FavoriteRssItems.Skip(skip).Take(PageSize).ToList();
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
    }
}

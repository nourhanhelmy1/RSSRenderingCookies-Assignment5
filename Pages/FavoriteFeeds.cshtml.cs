using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RenderingRSS.Pages
{
    public class FavoriteFeedsModel : PageModel
    {
        public List<string> FavoriteFeeds { get; set; }

        public void OnGet()
        {
            FavoriteFeeds = GetFavoriteFeedsFromCookie();
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

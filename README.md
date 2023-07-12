# RSSRenderingCookies-Assignment5

Rendering RSS is a web application that allows users to view and manage RSS feeds. It provides features for fetching and displaying RSS feed items, as well as the ability to star and unstar favorite feeds. The application utilizes ASP.NET Core Razor Pages and integrates with an external RSS feed service.

## Features

- Fetches and displays RSS feed items from a remote feed source
- Supports pagination for navigating through feed items
- Allows users to star/unstar favorite feeds
- Provides a separate page to view favorite feeds

## Project Structure

The project consists of the following main files:

- `Index.cshtml.cs`: Represents the model for the index page, responsible for fetching and displaying the RSS feed items.
- `FavoriteFeeds.cshtml.cs`: Represents the model for the favorite feeds page, responsible for retrieving and rendering the user's favorite feeds.
- `Index.cshtml`: The Razor page view for the index page, responsible for rendering the fetched RSS feed items and providing pagination.
- `FavoriteFeeds.cshtml`: The Razor page view for the favorite feeds page, responsible for rendering the user's favorite feeds.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvement, please open an issue or submit a pull request.


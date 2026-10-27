namespace Template.WindowsApp.Views.Features;

using System.Globalization;
using System.Net;
using System.Net.Http;

using NSubstitute;

using Smart.Navigation;

using Template.WindowsApp.Mocks;

[Collection(ViewModelTestGroup.Name)]
public sealed class WebApiViewModelTests
{
    private static readonly FixedTimeProvider Time = new(new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task NavigatedLoadsFirstPage()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();

        // Act
        await viewModel.OnNavigatedToAsync(Substitute.For<INavigationContext>());

        // Assert
        Assert.Equal(10, viewModel.Posts.Count);
        Assert.Equal((1, 10, 100), (viewModel.RangeStart, viewModel.RangeEnd, viewModel.TotalCount));
        Assert.StartsWith("GET /posts?_page=1&_limit=10 ", viewModel.ListResult, StringComparison.Ordinal);
        Assert.Contains("200 OK", viewModel.ListResult, StringComparison.Ordinal);
        Assert.False(viewModel.PreviousCommand.CanExecute(null));
        Assert.True(viewModel.NextCommand.CanExecute(null));
    }

    [Fact]
    public async Task NextAndPreviousMovePages()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();
        await viewModel.OnNavigatedToAsync(Substitute.For<INavigationContext>());

        // Act
        viewModel.NextCommand.Execute(null);
        var second = (viewModel.RangeStart, viewModel.RangeEnd, viewModel.Posts[0].Id);
        viewModel.PreviousCommand.Execute(null);

        // Assert
        Assert.Equal((11, 20, 11), second);
        Assert.Equal((1, 10), (viewModel.RangeStart, viewModel.RangeEnd));
        Assert.Equal(["/posts?_page=1&_limit=10", "/posts?_page=2&_limit=10", "/posts?_page=1&_limit=10"], handler.Requests.Select(static x => x.Path));
    }

    [Fact]
    public async Task SelectingPostGetsIt()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();
        await viewModel.OnNavigatedToAsync(Substitute.For<INavigationContext>());

        // Act
        viewModel.SelectedPost = viewModel.Posts[2];

        // Assert
        Assert.Equal(3d, viewModel.Id);
        Assert.Equal(("Title 3", "Body 3"), (viewModel.Title, viewModel.Body));
        Assert.StartsWith("GET /posts/3 ", viewModel.PostResult, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingPostIsShownAsError()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();
        viewModel.Id = 0;

        // Act
        viewModel.GetCommand.Execute(null);

        // Assert
        Assert.Contains("404 Not Found", viewModel.PostResult, StringComparison.Ordinal);
        Assert.True(Assert.Single(viewModel.Requests).IsError);
        Assert.Equal((1, 1), (viewModel.SentCount, viewModel.FailedCount));
        Assert.Equal(string.Empty, viewModel.Title);
    }

    [Fact]
    public void CreateSendsTitleAndShowsNewId()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();
        viewModel.Title = "Hello";
        viewModel.Body = "World";

        // Act
        viewModel.CreateCommand.Execute(null);

        // Assert
        Assert.Contains("201 Created", viewModel.PostResult, StringComparison.Ordinal);
        Assert.EndsWith(", new id 101", viewModel.PostResult, StringComparison.Ordinal);
        var request = Assert.Single(handler.Requests);
        Assert.Equal((HttpMethod.Post, "/posts"), (request.Method, request.Path));
        Assert.Equal("""{"userId":1,"title":"Hello","body":"World"}""", request.Body);
    }

    [Fact]
    public void UpdatePatchAndDeleteUseTheirMethods()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();
        viewModel.Id = 5;
        viewModel.Title = "Changed";
        viewModel.Body = "Text";

        // Act
        viewModel.UpdateCommand.Execute(null);
        viewModel.PatchCommand.Execute(null);
        viewModel.DeleteCommand.Execute(null);

        // Assert
        Assert.Equal([HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete], handler.Requests.Select(static x => x.Method));
        Assert.All(handler.Requests, static x => Assert.Equal("/posts/5", x.Path));
        Assert.Equal("""{"title":"Changed"}""", handler.Requests[1].Body);
        Assert.Equal(["DELETE", "PATCH", "PUT"], viewModel.Requests.Select(static x => x.Method));
        Assert.StartsWith("DELETE /posts/5 ", viewModel.PostResult, StringComparison.Ordinal);
    }

    [Fact]
    public void RequestsKeepNewestEightAndClear()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(Respond);
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();

        // Act
        for (var id = 1; id <= 10; id++)
        {
            viewModel.Id = id;
            viewModel.GetCommand.Execute(null);
        }
        var kept = viewModel.Requests.Select(static x => x.Path).ToArray();
        var sent = viewModel.SentCount;
        viewModel.ClearCommand.Execute(null);

        // Assert
        Assert.Equal(8, kept.Length);
        Assert.Equal(("/posts/10", "/posts/3"), (kept[0], kept[^1]));
        Assert.Equal(10, sent);
        Assert.Empty(viewModel.Requests);
        Assert.Equal((0, 0), (viewModel.SentCount, viewModel.FailedCount));
    }

    [Fact]
    public void ConnectionFailureIsShown()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(static _ => throw new HttpRequestException("No connection"));
        using var viewModel = new WebApiViewModel(Time, handler.CreateService());
        viewModel.OnActivated();

        // Act
        viewModel.GetCommand.Execute(null);

        // Assert
        Assert.Contains("Request error", viewModel.PostResult, StringComparison.Ordinal);
        Assert.Equal(1, viewModel.FailedCount);
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request)
    {
        var uri = request.RequestUri!;
        var path = uri.AbsolutePath;
        if (path == "/posts")
        {
            if (request.Method != HttpMethod.Get)
            {
                return FakeHttpMessageHandler.Json(HttpStatusCode.Created, Post(101));
            }

            var page = Int32.Parse(uri.Query.Split('&')[0]["?_page=".Length..], CultureInfo.InvariantCulture);
            var first = ((page - 1) * 10) + 1;
            var response = FakeHttpMessageHandler.Json(HttpStatusCode.OK, $"[{String.Join(',', Enumerable.Range(first, 10).Select(Post))}]");
            response.Headers.Add("X-Total-Count", "100");
            return response;
        }

        var id = Int32.Parse(path["/posts/".Length..], CultureInfo.InvariantCulture);
        if ((id < 1) || (id > 100))
        {
            return FakeHttpMessageHandler.Json(HttpStatusCode.NotFound, "{}");
        }

        return FakeHttpMessageHandler.Json(HttpStatusCode.OK, request.Method == HttpMethod.Delete ? "{}" : Post(id));
    }

    private static string Post(int id) =>
        String.Create(CultureInfo.InvariantCulture, $$"""{"userId":1,"id":{{id}},"title":"Title {{id}}","body":"Body {{id}}"}""");
}

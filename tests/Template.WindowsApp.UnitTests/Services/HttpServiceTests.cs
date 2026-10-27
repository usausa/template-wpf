namespace Template.WindowsApp.Services;

using System.Net;
using System.Net.Http;

using Rester;

using Template.WindowsApp.Mocks;

public sealed class HttpServiceTests
{
    private const string PostJson = """{"userId":1,"id":7,"title":"Title 7","body":"Body 7"}""";

    [Fact]
    public async Task GetPostsSendsPageAndReadsList()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(static _ => FakeHttpMessageHandler.Json(HttpStatusCode.OK, $"[{PostJson}]"));
        var service = handler.CreateService();

        // Act
        var response = await service.GetPostsAsync(2, 10, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(RestResult.Success, response.RestResult);
        Assert.NotNull(response.Content);
        var post = Assert.Single(response.Content);
        Assert.Equal((1, 7, "Title 7", "Body 7"), (post.UserId, post.Id, post.Title, post.Body));
        Assert.Equal(new FakeRequest(HttpMethod.Get, "/posts?_page=2&_limit=10", null), Assert.Single(handler.Requests));
    }

    [Fact]
    public async Task WriteRequestsUseTheirMethodsAndCamelCase()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(static _ => FakeHttpMessageHandler.Json(HttpStatusCode.OK, PostJson));
        var service = handler.CreateService();
        var cancel = TestContext.Current.CancellationToken;

        // Act
        await service.PostPostAsync(new PostRequest { UserId = 1, Title = "New", Body = "Text" }, cancel);
        await service.PutPostAsync(7, new PostRequest { UserId = 1, Title = "Put", Body = "Text" }, cancel);
        await service.PatchPostAsync(7, new PostPatchRequest { Title = "Patch" }, cancel);
        await service.DeletePostAsync(7, cancel);

        // Assert
        Assert.Equal([HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete], handler.Requests.Select(static x => x.Method));
        Assert.Equal(["/posts", "/posts/7", "/posts/7", "/posts/7"], handler.Requests.Select(static x => x.Path));
        Assert.Equal("""{"userId":1,"title":"New","body":"Text"}""", handler.Requests[0].Body);
        Assert.Equal("""{"title":"Patch"}""", handler.Requests[2].Body);
        Assert.Null(handler.Requests[3].Body);
    }

    [Fact]
    public async Task MissingPostIsHttpError()
    {
        // Arrange
        using var handler = new FakeHttpMessageHandler(static _ => FakeHttpMessageHandler.Json(HttpStatusCode.NotFound, "{}"));
        var service = handler.CreateService();

        // Act
        var response = await service.GetPostAsync(0, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal((RestResult.HttpError, HttpStatusCode.NotFound), (response.RestResult, response.StatusCode));
    }
}

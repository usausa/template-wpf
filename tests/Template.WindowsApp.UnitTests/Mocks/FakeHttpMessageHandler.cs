namespace Template.WindowsApp.Mocks;

using System.Net;
using System.Net.Http;
using System.Text;

using NSubstitute;

using Template.WindowsApp.Services;

public sealed record FakeRequest(HttpMethod Method, string Path, string? Body);

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private static readonly Uri BaseAddress = new("https://example.test/");

    private readonly Func<HttpRequestMessage, HttpResponseMessage> responder;

    private readonly List<FakeRequest> requests = [];

    public IReadOnlyList<FakeRequest> Requests => requests;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        this.responder = responder;
    }

    public static HttpResponseMessage Json(HttpStatusCode statusCode, string json) =>
        new(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public HttpService CreateService()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(this, false) { BaseAddress = BaseAddress });
        return new HttpService(factory);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        requests.Add(new FakeRequest(request.Method, request.RequestUri!.PathAndQuery, body));
        return responder(request);
    }
}

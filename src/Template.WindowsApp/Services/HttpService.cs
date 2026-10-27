namespace Template.WindowsApp.Services;

using Rester;

//--------------------------------------------------------------------------------
// Models
//--------------------------------------------------------------------------------

public sealed class PostResponse
{
    public int UserId { get; set; }

    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;

    public override string ToString() => Title;
}

public sealed class PostRequest
{
    public int UserId { get; set; }

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;
}

public sealed class PostPatchRequest
{
    public string Title { get; set; } = default!;
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HttpService
{
    private readonly IHttpClientFactory httpClientFactory;

    public HttpService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public Uri? BaseAddress => httpClientFactory.CreateClient(ApiNames.Default).BaseAddress;

    //--------------------------------------------------------------------------------
    // Post
    //--------------------------------------------------------------------------------

    public ValueTask<IRestResponse<PostResponse[]>> GetPostsAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<PostResponse[]>($"posts?_page={page}&_limit={size}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<PostResponse>> GetPostAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.GetAsync<PostResponse>($"posts/{id}", cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<PostResponse>> PostPostAsync(PostRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PostAsync<PostResponse>("posts", request, cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<PostResponse>> PutPostAsync(int id, PostRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PutAsync<PostResponse>($"posts/{id}", request, cancel: cancellationToken);
    }

    public ValueTask<IRestResponse<PostResponse>> PatchPostAsync(int id, PostPatchRequest request, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.PatchAsync<PostResponse>($"posts/{id}", request, cancel: cancellationToken);
    }

    public ValueTask<IRestResponse> DeletePostAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiNames.Default);
        return client.SendAsync(HttpMethod.Delete, $"posts/{id}", cancel: cancellationToken);
    }
}

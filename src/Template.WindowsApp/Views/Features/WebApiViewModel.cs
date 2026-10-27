namespace Template.WindowsApp.Views.Features;

using System.Net.Http.Headers;

using Rester;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

public sealed record RequestEntry(
    DateTimeOffset Time,
    string Method,
    string Path,
    string Status,
    long Milliseconds,
    bool IsError)
{
    public override string ToString() => $"{Method} {Path} {Status}";
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class WebApiViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int PageSize = 10;

    private const int RequestLimit = 8;

    private const int UserId = 1;

    private const string TotalCountHeader = "X-Total-Count";

    private const string Arrow = "\u2192";

    private readonly TimeProvider timeProvider;

    private readonly HttpService httpService;

    public Uri? BaseAddress { get; }

    public ObservableCollection<PostResponse> Posts { get; } = [];

    public ObservableCollection<RequestEntry> Requests { get; } = [];

    [ObservableProperty]
    public partial PostResponse? SelectedPost { get; set; }

    [ObservableProperty]
    public partial int Page { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial int RangeStart { get; set; }

    [ObservableProperty]
    public partial int RangeEnd { get; set; }

    [ObservableProperty]
    public partial string? ListResult { get; set; }

    [ObservableProperty]
    public partial double? Id { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Body { get; set; }

    [ObservableProperty]
    public partial string? PostResult { get; set; }

    [ObservableProperty]
    public partial int SentCount { get; set; }

    [ObservableProperty]
    public partial int FailedCount { get; set; }

    public IObserveCommand ReloadCommand { get; }

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand GetCommand { get; }

    public IObserveCommand CreateCommand { get; }

    public IObserveCommand UpdateCommand { get; }

    public IObserveCommand PatchCommand { get; }

    public IObserveCommand DeleteCommand { get; }

    public IObserveCommand ClearCommand { get; }

    public WebApiViewModel(TimeProvider timeProvider, HttpService httpService)
    {
        this.timeProvider = timeProvider;
        this.httpService = httpService;

        BaseAddress = httpService.BaseAddress;
        Page = 1;
        Id = 1;
        Title = string.Empty;
        Body = string.Empty;

        ReloadCommand = MakeAsyncCommand(() => LoadAsync(Page));
        PreviousCommand = MakeAsyncCommand(() => LoadAsync(Page - 1), () => Page > 1);
        NextCommand = MakeAsyncCommand(() => LoadAsync(Page + 1), () => RangeEnd < TotalCount);
        GetCommand = MakeAsyncCommand(GetAsync, () => Id.HasValue);
        CreateCommand = MakeAsyncCommand(CreateAsync);
        UpdateCommand = MakeAsyncCommand(UpdateAsync, () => Id.HasValue);
        PatchCommand = MakeAsyncCommand(PatchAsync, () => Id.HasValue);
        DeleteCommand = MakeAsyncCommand(DeleteAsync, () => Id.HasValue);
        ClearCommand = MakeDelegateCommand(Clear);

        SubscribeSelectedPost(Select);
    }

    public override Task OnNavigatedToAsync(INavigationContext context) => LoadAsync(1);

    private static string PostPath(int id) =>
        String.Create(CultureInfo.InvariantCulture, $"/posts/{id}");

    private static int? FindTotalCount(HttpResponseHeaders? headers) =>
        (headers is not null) &&
        headers.TryGetValues(TotalCountHeader, out var values) &&
        Int32.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)
            ? count
            : null;

    private static string FormatStatus(IRestResponse response) =>
        response.RestResult switch
        {
            RestResult.Success or RestResult.HttpError => FormatStatusCode(response.StatusCode),
            RestResult.Timeout => "Timeout",
            RestResult.Cancel => "Canceled",
            RestResult.RequestError => "Request error",
            RestResult.SerializeError => "Invalid response",
            _ => "Failed"
        };

    private static string FormatStatusCode(HttpStatusCode code) =>
        Enum.IsDefined(code)
            ? String.Create(CultureInfo.InvariantCulture, $"{(int)code} {SplitWords(code.ToString())}")
            : ((int)code).ToString(CultureInfo.InvariantCulture);

    private static string SplitWords(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if ((i > 0) && Char.IsUpper(name[i]) && Char.IsLower(name[i - 1]))
            {
                builder.Append(' ');
            }
            builder.Append(name[i]);
        }
        return builder.ToString();
    }

    private int CurrentId() => (int)Id!.Value;

    private string Record(string method, string path, IRestResponse response, long start)
    {
        var elapsed = (long)timeProvider.GetElapsedTime(start).TotalMilliseconds;
        var status = FormatStatus(response);
        var isError = response.RestResult != RestResult.Success;

        Requests.Insert(0, new RequestEntry(timeProvider.GetLocalNow(), method, path, status, elapsed, isError));
        while (Requests.Count > RequestLimit)
        {
            Requests.RemoveAt(Requests.Count - 1);
        }

        SentCount++;
        if (isError)
        {
            FailedCount++;
        }

        return String.Create(CultureInfo.CurrentCulture, $"{method} {path} {Arrow} {status} ({elapsed:N0} ms)");
    }

    private void Select(PostResponse? post)
    {
        if (post is null)
        {
            return;
        }

        Id = post.Id;
        if (GetCommand.CanExecute(null))
        {
            GetCommand.Execute(null);
        }
    }

    private async Task LoadAsync(int page)
    {
        var start = timeProvider.GetTimestamp();
        var response = await httpService.GetPostsAsync(page, PageSize).ConfigureAwait(true);
        ListResult = Record("GET", String.Create(CultureInfo.InvariantCulture, $"/posts?_page={page}&_limit={PageSize}"), response, start);
        if ((response.RestResult != RestResult.Success) || (response.Content is not { } posts))
        {
            return;
        }

        Posts.Clear();
        foreach (var post in posts)
        {
            Posts.Add(post);
        }

        var offset = (page - 1) * PageSize;
        Page = page;
        TotalCount = FindTotalCount(response.Headers) ?? (offset + Posts.Count);
        RangeStart = Posts.Count > 0 ? offset + 1 : 0;
        RangeEnd = offset + Posts.Count;
    }

    private async Task GetAsync()
    {
        var id = CurrentId();
        var start = timeProvider.GetTimestamp();
        var response = await httpService.GetPostAsync(id).ConfigureAwait(true);
        PostResult = Record("GET", PostPath(id), response, start);
        if ((response.RestResult == RestResult.Success) && (response.Content is { } post))
        {
            Title = post.Title;
            Body = post.Body;
        }
    }

    private async Task CreateAsync()
    {
        var start = timeProvider.GetTimestamp();
        var response = await httpService.PostPostAsync(new PostRequest { UserId = UserId, Title = Title, Body = Body }).ConfigureAwait(true);
        var result = Record("POST", "/posts", response, start);
        PostResult = (response.RestResult == RestResult.Success) && (response.Content is { } post)
            ? String.Create(CultureInfo.CurrentCulture, $"{result}, new id {post.Id}")
            : result;
    }

    private async Task UpdateAsync()
    {
        var id = CurrentId();
        var start = timeProvider.GetTimestamp();
        var response = await httpService.PutPostAsync(id, new PostRequest { UserId = UserId, Title = Title, Body = Body }).ConfigureAwait(true);
        PostResult = Record("PUT", PostPath(id), response, start);
    }

    private async Task PatchAsync()
    {
        var id = CurrentId();
        var start = timeProvider.GetTimestamp();
        var response = await httpService.PatchPostAsync(id, new PostPatchRequest { Title = Title }).ConfigureAwait(true);
        PostResult = Record("PATCH", PostPath(id), response, start);
    }

    private async Task DeleteAsync()
    {
        var id = CurrentId();
        var start = timeProvider.GetTimestamp();
        var response = await httpService.DeletePostAsync(id).ConfigureAwait(true);
        PostResult = Record("DELETE", PostPath(id), response, start);
    }

    private void Clear()
    {
        Requests.Clear();
        SentCount = 0;
        FailedCount = 0;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("WebApi:Reload", "Reload posts", "Web API", "\uE72C", ReloadCommand),
        new("WebApi:Clear", "Clear requests", "Web API", "\uE894", ClearCommand)
    ];
}

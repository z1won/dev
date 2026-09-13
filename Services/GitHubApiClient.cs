using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace dev.Services;

public sealed class GitHubApiClient(HttpClient httpClient, ApiTelemetry telemetry)
{
    public Task<GitHubProfile?> GetProfileAsync(string username, CancellationToken cancellationToken = default)
        => GetJsonAsync<GitHubProfile>($"/users/{Uri.EscapeDataString(username)}", "GET /users/{user}", cancellationToken);

    public Task<IReadOnlyList<GitHubRepository>> GetRepositoriesAsync(string username, int perPage = 12, CancellationToken cancellationToken = default)
        => GetJsonAsync<IReadOnlyList<GitHubRepository>>($"/users/{Uri.EscapeDataString(username)}/repos?sort=updated&per_page={Math.Clamp(perPage, 1, 100)}", "GET /users/{user}/repos", cancellationToken);

    public Task<GitHubRepository?> GetRepositoryAsync(string owner, string repository, CancellationToken cancellationToken = default)
        => GetJsonAsync<GitHubRepository>($"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}", "GET /repos/{owner}/{repo}", cancellationToken);

    public async Task<GitHubReadme?> GetReadmeAsync(string owner, string repository, CancellationToken cancellationToken = default)
    {
        var readme = await GetJsonAsync<GitHubReadme>($"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/readme", "GET /repos/{owner}/{repo}/readme", cancellationToken);
        if (readme?.Content is null) return readme;
        try
        {
            var normalized = readme.Content.Replace("\n", "").Replace("\r", "");
            return readme with { DecodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(normalized)) };
        }
        catch (FormatException) { return readme; }
    }

    public Task<IReadOnlyList<GitHubCommit>> GetCommitsAsync(string owner, string repository, int perPage = 5, CancellationToken cancellationToken = default)
        => GetJsonAsync<IReadOnlyList<GitHubCommit>>($"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/commits?per_page={Math.Clamp(perPage, 1, 20)}", "GET /repos/{owner}/{repo}/commits", cancellationToken);

    public Task<GitHubCommitDetail?> GetCommitAsync(string owner, string repository, string sha, CancellationToken cancellationToken = default)
        => GetJsonAsync<GitHubCommitDetail>($"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/commits/{Uri.EscapeDataString(sha)}", "GET /repos/{owner}/{repo}/commits/{sha}", cancellationToken);

    public Task<IReadOnlyList<GitHubPullRequest>> GetPullRequestsAsync(string owner, string repository, int perPage = 5, CancellationToken cancellationToken = default)
        => GetJsonAsync<IReadOnlyList<GitHubPullRequest>>($"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/pulls?state=all&sort=updated&direction=desc&per_page={Math.Clamp(perPage, 1, 20)}", "GET /repos/{owner}/{repo}/pulls", cancellationToken);

    public Task<IReadOnlyList<GitHubIssue>> GetIssuesAsync(string owner, string repository, int perPage = 5, CancellationToken cancellationToken = default)
        => GetJsonAsync<IReadOnlyList<GitHubIssue>>($"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/issues?state=all&sort=updated&direction=desc&per_page={Math.Clamp(perPage, 1, 20)}", "GET /repos/{owner}/{repo}/issues", cancellationToken);

    private async Task<T?> GetJsonAsync<T>(string endpoint, string telemetryEndpoint, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var response = await httpClient.GetAsync(endpoint, cancellationToken);
            stopwatch.Stop();
            var status = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                var message = $"HTTP {status} {response.ReasonPhrase}";
                telemetry.Record("GET", telemetryEndpoint, status, stopwatch.ElapsedMilliseconds, message);
                throw new HttpRequestException(message);
            }
            var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            telemetry.Record("GET", telemetryEndpoint, status, stopwatch.ElapsedMilliseconds);
            return value;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            telemetry.Record("GET", telemetryEndpoint, 408, stopwatch.ElapsedMilliseconds, "Request timed out");
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException)
        {
            stopwatch.Stop();
            telemetry.Record("GET", telemetryEndpoint, 0, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}

public sealed record GitHubProfile(string Login, string? Name, string? AvatarUrl, string? HtmlUrl, string? Bio, int PublicRepos, int Followers, int Following);
public sealed record GitHubRepository(string Name, string? Description, string? HtmlUrl, string? Language, int StargazersCount, int ForksCount, bool Fork, DateTimeOffset? UpdatedAt);
public sealed record GitHubReadme(string? Name, string? Path, string? HtmlUrl, string? Content, string? Encoding)
{
    [JsonIgnore] public string? DecodedContent { get; init; }
}
public sealed record GitHubCommit(string Sha, GitHubCommitDetails? Commit, GitHubUser? Author, string? HtmlUrl);
public sealed record GitHubCommitDetails(GitHubCommitAuthor? Author, string? Message);
public sealed record GitHubCommitAuthor(string? Name, DateTimeOffset? Date);
public sealed record GitHubCommitDetail(string Sha, GitHubCommitDetails? Commit, GitHubUser? Author, string? HtmlUrl, GitHubCommitStats? Stats, IReadOnlyList<GitHubCommitFile>? Files);
public sealed record GitHubCommitStats(int Additions, int Deletions, int Total);
public sealed record GitHubCommitFile(string? Filename, string? Status, int Additions, int Deletions, int Changes, string? BlobUrl, string? RawUrl, string? Patch);
public sealed record GitHubUser(string? Login, string? AvatarUrl, string? HtmlUrl);
public sealed record GitHubPullRequest(int Number, string? Title, string? HtmlUrl, string? State, bool Draft, DateTimeOffset? UpdatedAt, GitHubUser? User);
public sealed record GitHubIssue(int Number, string? Title, string? HtmlUrl, string? State, DateTimeOffset? UpdatedAt, GitHubUser? User, GitHubPullRequestLink? PullRequest);
public sealed record GitHubPullRequestLink(string? Url);

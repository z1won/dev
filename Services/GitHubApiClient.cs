using System.Net.Http.Json;

namespace dev.Services;

public sealed class GitHubApiClient(HttpClient httpClient)
{
    public async Task<GitHubProfile?> GetProfileAsync(string username, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<GitHubProfile>($"https://api.github.com/users/{Uri.EscapeDataString(username)}", cancellationToken);

    public async Task<IReadOnlyList<GitHubRepository>> GetRepositoriesAsync(string username, int perPage = 12, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<GitHubRepository>>($"https://api.github.com/users/{Uri.EscapeDataString(username)}/repos?sort=updated&per_page={Math.Clamp(perPage, 1, 100)}", cancellationToken) ?? [];

    public async Task<IReadOnlyList<GitHubCommit>> GetCommitsAsync(string owner, string repository, int perPage = 5, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<GitHubCommit>>($"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/commits?per_page={Math.Clamp(perPage, 1, 20)}", cancellationToken) ?? [];

    public async Task<IReadOnlyList<GitHubPullRequest>> GetPullRequestsAsync(string owner, string repository, int perPage = 5, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<GitHubPullRequest>>($"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/pulls?state=all&sort=updated&direction=desc&per_page={Math.Clamp(perPage, 1, 20)}", cancellationToken) ?? [];

    public async Task<IReadOnlyList<GitHubIssue>> GetIssuesAsync(string owner, string repository, int perPage = 5, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<GitHubIssue>>($"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/issues?state=all&sort=updated&direction=desc&per_page={Math.Clamp(perPage, 1, 20)}", cancellationToken) ?? [];
}

public sealed record GitHubProfile(string Login, string? Name, string? AvatarUrl, string? HtmlUrl, string? Bio, int PublicRepos, int Followers, int Following);
public sealed record GitHubRepository(string Name, string? Description, string? HtmlUrl, string? Language, int StargazersCount, int ForksCount, bool Fork, DateTimeOffset? UpdatedAt);
public sealed record GitHubCommit(string Sha, GitHubCommitDetails? Commit, GitHubUser? Author, string? HtmlUrl);
public sealed record GitHubCommitDetails(GitHubCommitAuthor? Author, string? Message);
public sealed record GitHubCommitAuthor(string? Name, DateTimeOffset? Date);
public sealed record GitHubUser(string? Login, string? AvatarUrl, string? HtmlUrl);
public sealed record GitHubPullRequest(int Number, string? Title, string? HtmlUrl, string? State, bool Draft, DateTimeOffset? UpdatedAt, GitHubUser? User);
public sealed record GitHubIssue(int Number, string? Title, string? HtmlUrl, string? State, DateTimeOffset? UpdatedAt, GitHubUser? User, GitHubPullRequestLink? PullRequest);
public sealed record GitHubPullRequestLink(string? Url);

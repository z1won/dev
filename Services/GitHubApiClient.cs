using System.Net.Http.Json;

namespace dev.Services;

public sealed class GitHubApiClient(HttpClient httpClient)
{
    public async Task<GitHubProfile?> GetProfileAsync(string username, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<GitHubProfile>($"https://api.github.com/users/{Uri.EscapeDataString(username)}", cancellationToken);

    public async Task<IReadOnlyList<GitHubRepository>> GetRepositoriesAsync(string username, int perPage = 12, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<GitHubRepository>>($"https://api.github.com/users/{Uri.EscapeDataString(username)}/repos?sort=updated&per_page={Math.Clamp(perPage, 1, 100)}", cancellationToken) ?? [];
}

public sealed record GitHubProfile(
    string Login,
    string? Name,
    string? AvatarUrl,
    string? HtmlUrl,
    string? Bio,
    int PublicRepos,
    int Followers,
    int Following);

public sealed record GitHubRepository(
    string Name,
    string? Description,
    string? HtmlUrl,
    string? Language,
    int StargazersCount,
    int ForksCount,
    bool Fork,
    DateTimeOffset? UpdatedAt);

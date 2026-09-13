namespace dev.Services;

public sealed class ApiTelemetry
{
    private readonly List<ApiRequestRecord> requests = [];

    public IReadOnlyList<ApiRequestRecord> Requests => requests;
    public event Action? Changed;

    public void Record(string method, string endpoint, int statusCode, long elapsedMs, string? error = null)
    {
        requests.Insert(0, new ApiRequestRecord(DateTimeOffset.Now, method, endpoint, statusCode, elapsedMs, error));
        if (requests.Count > 12) requests.RemoveAt(requests.Count - 1);
        Changed?.Invoke();
    }
}

public sealed record ApiRequestRecord(
    DateTimeOffset Time,
    string Method,
    string Endpoint,
    int StatusCode,
    long ElapsedMs,
    string? Error)
{
    public bool Success => StatusCode is >= 200 and < 300 && Error is null;
}

using System.Net.Http.Json;
using System.Text.Json;

namespace CreadorDeRequerimientos.Mobile.Services;

public sealed class CreadorApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<T?> PostAsync<T>(string path, object payload, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, payload, JsonOptions, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<T?> PutAsync<T>(string path, object payload, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PutAsJsonAsync(path, payload, JsonOptions, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private static async Task<T?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body,
                null,
                response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }
}

// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nethermind.KZGCeremony;

public class Coordinator : ICoordinator
{
    private readonly HttpClient _httpClient;

    public Coordinator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CeremonyStatus> GetStatus()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("/info/status");
        HttpContent content = response.EnsureSuccessStatusCode().Content;
        CeremonyStatus? status = await content.ReadFromJsonAsync<CeremonyStatus>();
        return status ?? throw new Exception("Failed to deserialize status");
    }

    public async Task<CeremonyTranscript> GetTranscript()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("/info/current_state");
        HttpContent content = response.EnsureSuccessStatusCode().Content;
        CeremonyTranscript? transcript = await content.ReadFromJsonAsync<CeremonyTranscript>();
        return transcript ?? throw new Exception("Failed to deserialize transcript");
    }

    public async Task<IContributionBatch?> TryContribute(string sessionToken)
    {
        HttpRequestMessage message = new(HttpMethod.Post, "/lobby/try_contribute");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);

        HttpResponseMessage response = await _httpClient.SendAsync(message);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // We were rate limited by the coordinator api
            throw new RateLimitedException();
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Unauthorized call to the coordinator api
            throw new UnauthorizedException();
        }

        string payload = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
        // The coordinator api returns an object containing an error field if it's still not our turn
        return JsonDocument.Parse(payload).RootElement.TryGetProperty("error", out _)
            ? null : JsonSerializer.Deserialize<IContributionBatch>(payload);
    }

    public async Task<ContributionReceipt> Contribute(string sessionToken, IContributionBatch contributionBatch)
    {
        HttpRequestMessage message = new(HttpMethod.Post, "/contribute");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
        message.Content = JsonContent.Create(contributionBatch);

        HttpResponseMessage response = await _httpClient.SendAsync(message);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // There was an error with our contribution
            // TODO Decode error from response content
            throw new ContributionAbortException();
        }

        HttpContent content = response.EnsureSuccessStatusCode().Content;
        ContributionReceipt? receipt = await content.ReadFromJsonAsync<ContributionReceipt>();
        return receipt ?? throw new Exception("Failed to deserialize receipt");
    }

    public async Task Abort(string sessionToken)
    {
        HttpRequestMessage message = new(HttpMethod.Post, "/contribution/abort");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);

        HttpResponseMessage response = await _httpClient.SendAsync(message);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // We were rate limited by the coordinator api
            // TODO Decode error from response content
            throw new RateLimitedException();
        }

        _ = response.EnsureSuccessStatusCode();
    }
}

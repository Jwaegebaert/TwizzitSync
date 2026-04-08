using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwizzitSync.Config;
using TwizzitSync.Models.Twizzit;
using TwizzitSync.Utils;

namespace TwizzitSync.Services;

internal class TwizzitService
{
    private string _accessToken;

    /// <summary>
    /// Initialize Twizzit service.
    /// </summary>
    internal async Task InitializeTwizzitService()
    {
        var authToken = await AuthenticationUtil.GetTwizzitTokenAsync();
        _accessToken = authToken.Token;
    } 
    
    /// <summary>
    /// Factory method to create and initialize TwizzitService.
    /// </summary>
    public static async Task<TwizzitService> CreateAsync()
    {
        var service = new TwizzitService();
        await service.InitializeTwizzitService();
        return service;
    }

    /// <summary>
    /// Get contacts from Twizzit without membership information.
    /// </summary>
    /// <returns>Contacts.</returns>
    public async Task<Contact[]> GetContactsWithMembershipAsync()
    {
        var contacts = await CallTwizzitApiAsync<Contact>($"{EnvironmentConfig.TwizzitApiUrl}/contacts");
        var memberships = await GetMembershipsAsync();

        foreach (var contact in contacts)
        {
            contact.Membership = memberships.FirstOrDefault(m => m.ContactId == contact.Id);
        }

        return contacts;
    }

    /// <summary>
    /// Get memberships from Twizzit.
    /// </summary>
    /// <returns>Memberships.</returns>
    public async Task<Membership[]> GetMembershipsAsync()
    {
        return await CallTwizzitApiAsync<Membership>($"{EnvironmentConfig.TwizzitApiUrl}/memberships");
    }

    /// <summary>
    /// Execute Twizzit API call.
    /// Include pagination.
    /// </summary>
    /// <typeparam name="T">Type of response.</typeparam>
    /// <param name="endpoint">API endpoint.</param>
    /// <param name="page">Page number.</param>
    /// <returns>API response.</returns>
    private async Task<T[]> CallTwizzitApiAsync<T>(string endpoint, int page = 0)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        var response = await client.GetAsync($"{endpoint}?organization-ids[]={EnvironmentConfig.TwizzitOrganizationId}&limit=50&offset={page * 50}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error calling Twizzit API ({endpoint}): {response.ReasonPhrase}");

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<T[]>(jsonResponse);

        if (result != null && result.Length == 50)
        {
            var nextResult = await CallTwizzitApiAsync<T>(endpoint, page + 1);
            return [.. result, .. nextResult];
        }

        return result;
    }
}
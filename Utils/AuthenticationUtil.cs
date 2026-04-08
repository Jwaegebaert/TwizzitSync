using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Framework;
using TwizzitSync.Config;
using TwizzitSync.Models.Twizzit;

namespace TwizzitSync.Utils;

internal static class AuthenticationUtil
{
    /// <summary>
    /// Get a SharePoint client context.
    /// </summary>
    public static async Task<ClientContext> GetClientContextAsync(string siteUrl)
    {
        var certificate = await KeyVaultUtil.GetCertificateAsync(EnvironmentConfig.KeyVaultCertificateName);
        var ctx = await new AuthenticationManager(EnvironmentConfig.ClientId, certificate, EnvironmentConfig.TenantId).GetContextAsync(siteUrl);

        return ctx;
    }
    
    /// <summary>
    /// Get graph service client.
    /// </summary>
    /// <returns>Graph service client.</returns>
    public static async Task<GraphServiceClient> GetAppOnlyGraphServiceClientAsync()
    {
        var certificate = await KeyVaultUtil.GetCertificateAsync(EnvironmentConfig.KeyVaultCertificateName);
        var credential = new ClientCertificateCredential(EnvironmentConfig.TenantId, EnvironmentConfig.ClientId, certificate);
        var client = new GraphServiceClient(credential);

        return client;
    }

    /// <summary>
    /// Get a Twizzit access token.
    /// </summary>
    /// <returns>Twizzit access token.</returns>
    public static async Task<AuthToken> GetTwizzitTokenAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    
        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", EnvironmentConfig.TwizzitUsername),
            new KeyValuePair<string, string>("password", EnvironmentConfig.TwizzitUserCredentials)
        ]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
    
        var response = await client.PostAsync($"{EnvironmentConfig.TwizzitApiUrl}/authenticate", content);
        response.EnsureSuccessStatusCode();
    
        var responseBody = await response.Content.ReadAsStringAsync();        
        return JsonConvert.DeserializeObject<AuthToken>(responseBody);
    }
}
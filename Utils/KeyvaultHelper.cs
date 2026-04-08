using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TwizzitSync.Config;

namespace TwizzitSync.Utils;

internal class KeyVaultUtil
{
    private static readonly Uri VaultUri = new(EnvironmentConfig.KeyVaultUrl);

    private const string CacheKey = "TwizzitSyncCert-";

    /// <summary>
    /// Get secret from key vault.
    /// </summary>
    /// <param name="name">Secret name.</param>
    /// <param name="forceNew">When false, secret will be recovered from cache.</param>
    /// <returns>Secret.</returns>
    public static async Task<X509Certificate2> GetCertificateAsync(string name, bool forceNew = false)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
        {
            string Certificate = @"C:\Users\Jwaeg\Documents\Projects\_Personal\CertGenerator\Certificates\TwizzitSyncCertificate.pfx";
            return new X509Certificate2(Certificate, EnvironmentConfig.KeyVaultCertificateLocalPass, X509KeyStorageFlags.MachineKeySet);
        }

        if (!forceNew && CacheUtil.TryGetItem(CacheKey + name, out X509Certificate2 certificate))
            return certificate;

        var client = new SecretClient(VaultUri, new DefaultAzureCredential());
        var secretResponse = await client.GetSecretAsync(name);
        var secret = secretResponse.Value.Value;
        certificate = ConvertBase64ToCertificate(secret);

        // Add to cache
        CacheUtil.AddItem(CacheKey + name, certificate, TimeSpan.FromHours(1));

        return certificate;
    }

    /// <summary>
    /// Convert base64 string to X509 certificate.
    /// </summary>
    /// <param name="base64String">Certificate base64 string value.</param>
    /// <returns>Certificate object.</returns>
    private static X509Certificate2 ConvertBase64ToCertificate(string base64String)
    {
        var privateKeyBytes = Convert.FromBase64String(base64String);
        return new X509Certificate2(privateKeyBytes, (string)null
          , X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.Exportable);
    }
}
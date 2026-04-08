using System;

namespace TwizzitSync.Config;

public class EnvironmentConfig
{
    private static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name);

    #region Entra ID
    public static string ClientId => GetEnvironmentVariable("Entra:ClientId");
    public static string TenantId => GetEnvironmentVariable("Entra:TenantId");
    public static string KeyVaultUrl => GetEnvironmentVariable("Entra:KeyVaultUrl");
    public static string KeyVaultCertificateName => GetEnvironmentVariable("Entra:KeyVaultCertificateName");
    public static string KeyVaultCertificateLocalPass => GetEnvironmentVariable("Entra:KeyVaultCertificateLocalPass");
    public static string ServiceAccount => GetEnvironmentVariable("Entra:ServiceAccount");
    public static string ServiceAccountPassword => GetEnvironmentVariable("Entra:ServiceAccountPassword");
    #endregion

    #region Azure
    public static string AzureStorageConnection => GetEnvironmentVariable("AzureWebJobsStorage");
    #endregion

    #region Twizzit
    public static string TwizzitApiUrl => GetEnvironmentVariable("Twizzit:ApiUrl");
    public static string TwizzitUsername => GetEnvironmentVariable("Twizzit:Username");
    public static string TwizzitUserCredentials => GetEnvironmentVariable("Twizzit:UserCredentials");
    public static string TwizzitOrganizationId => GetEnvironmentVariable("Twizzit:OrganizationId");
    #endregion

    #region SharePoint
    public static string SharePointRootUrl => new Uri(SharePointDataSite).GetLeftPart(UriPartial.Authority);
    public static string SharePointAdminUrl => SharePointRootUrl.Replace(".sharepoint.com", "-admin.sharepoint.com");
    public static string SharePointDataSite => GetEnvironmentVariable("SP:DataSite");
    public static string SharePointDataSiteRelativeUrl => new Uri(SharePointDataSite).AbsolutePath;
    #endregion

}

using Newtonsoft.Json;

namespace TwizzitSync.Models.Twizzit;

public class AuthToken
{
    public string Token { get; set; }
    [JsonProperty("created-on")]
    public long CreatedOn { get; set; }
    [JsonProperty("valid-till")]
    public long ValidTill { get; set; }
}
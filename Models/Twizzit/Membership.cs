using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TwizzitSync.Models.Twizzit;

public class Membership
{
    public int Id { get; set; }
    [JsonProperty("contact-id")]
    public int ContactId { get; set; }
    [JsonProperty("membership-type-id")]
    public int MembershipTypeId { get; set; }
    [JsonProperty("season-id")]
    public int? SeasonId { get; set; }
    [JsonProperty("start-date")]
    public DateTime StartDate { get; set; }
    [JsonProperty("end-date")]
    public string EndDate { get; set; }
    [JsonProperty("extra-field-values")]
    public List<object> ExtraFieldValues { get; set; } 
}

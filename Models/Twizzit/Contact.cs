using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TwizzitSync.Models.Twizzit;

public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    [JsonProperty("date-of-birth")]
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string Nationality { get; set; }
    public string Language { get; set; }
    [JsonProperty("account-number")]
    public string AccountNumber { get; set; }
    [JsonProperty("registry-number")]
    public string RegistryNumber { get; set; }
    public string Number { get; set; }
    [JsonProperty("email-1")]
    public Email Email1 { get; set; }
    [JsonProperty("email-2")]
    public Email Email2 { get; set; }
    [JsonProperty("email-3")]
    public Email Email3 { get; set; }
    [JsonProperty("mobile-1")]
    public Mobile Mobile1 { get; set; }
    [JsonProperty("mobile-2")]
    public Mobile Mobile2 { get; set; }
    [JsonProperty("mobile-3")]
    public Mobile Mobile3 { get; set; }
    public Phone Phone { get; set; }
    public Address Address { get; set; }
    [JsonProperty("extra-field-values")]
    public List<object> ExtraFieldValues { get; set; }
    public Membership Membership { get; set; }
}

public class Email
{
    public object Target { get; set; }
    [JsonProperty("email")]
    public string EmailAddress { get; set; }
}

public class Mobile
{
    public object Target { get; set; }
    [JsonProperty("cc")]
    public string CountryCode { get; set; }
    [JsonProperty("number")]
    public string PhoneNumber { get; set; }
}

public class Phone
{
    public string Target { get; set; }
    [JsonProperty("cc")]
    public string CountryCode { get; set; }
    [JsonProperty("number")]
    public string PhoneNumber { get; set; }
}

public class Address
{
    public string Street { get; set; }
    [JsonProperty("number")]
    public string StreetNumber { get; set; }
    public string Box { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public Country Country { get; set; }
}

public class Country
{
    [JsonProperty("EN")]
    public string English { get; set; }
    [JsonProperty("NL")]
    public string Dutch { get; set; }
    [JsonProperty("FR")]
    public string French { get; set; }
}

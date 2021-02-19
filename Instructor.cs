using System;
using Newtonsoft.Json;

namespace CosmoSdk
{
    public class Instructor
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "address")]
        public Address Address { get; set; }
    }

    public class Address
    {
        [JsonProperty(PropertyName = "postal")]
        public string Postal { get; set; }
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
        [JsonProperty(PropertyName = "location")]
        public Location Location { get; set; }
    }

    public class Location
    {
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        [JsonProperty(PropertyName = "provience")]
        public string Provience { get; set; }
    }
}

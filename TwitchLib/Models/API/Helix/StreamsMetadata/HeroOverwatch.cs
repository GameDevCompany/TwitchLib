﻿using Newtonsoft.Json;

namespace TwitchLib.Models.API.Helix.StreamsMetadata
{
    public class HeroOverwatch
    {
        [JsonProperty(PropertyName = "ability")]
        public string Ability { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; protected set; }
        [JsonProperty(PropertyName = "role")]
        public string Role { get; protected set; }
    }
}

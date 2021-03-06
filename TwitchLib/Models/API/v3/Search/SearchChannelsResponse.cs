﻿using Newtonsoft.Json;

namespace TwitchLib.Models.API.v3.Search
{
    public class SearchChannelsResponse
    {
        [JsonProperty(PropertyName = "channels")]
        public Channels.Channel[] Channels { get; protected set; }
        [JsonProperty(PropertyName = "_total")]
        public int Total { get; protected set; }
    }
}

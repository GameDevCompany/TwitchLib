﻿using Newtonsoft.Json;

namespace TwitchLib.Models.API.v5.Ingests
{
    public class Ingests
    {
        #region Ingests
        [JsonProperty(PropertyName = "ingests")]
        public Ingest[] IngestServers { get; protected set; }
        #endregion
    }
}

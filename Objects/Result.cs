using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CasinoRaid.Objects
{
    class SocketUpdate
    {
        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    class Result
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    class Data
    {
        [JsonProperty("data")]
        public SubData SubData { get; set; }
    }

    class SubData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("h")]
        public string Hash { get; set; }

        [JsonProperty("i")]
        public int Id { get; set; }

        [JsonProperty("c")]
        public float Crash { get; set; }
    }
}

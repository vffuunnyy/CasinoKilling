using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CasinoRaid.Objects
{
    class Bet
    {
        [JsonProperty("userItemIds")]
        public int[] UserItemIds { get; set; }

        [JsonProperty("auto")]
        public string Auto { get; set; }
    }
}

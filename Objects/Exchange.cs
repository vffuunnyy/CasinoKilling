using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CasinoRaid.Objects
{
    class Exchange
    {
        [JsonProperty("userItemIds")]
        public int[] UserItemIds { get; set; }

        [JsonProperty("wishItemIds")]
        public int[] WishItemIds { get; set; }
    }
}

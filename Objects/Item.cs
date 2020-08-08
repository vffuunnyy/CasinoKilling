using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CasinoRaid.Objects
{
    class Item
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("itemId")]
        public long ItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public float Price { get; set; }
    }

    class ItemsResponse
    {
        [JsonProperty("data")]
        public JArray Data { get; set; }

        public IEnumerable<ListItem> Items => Data.Select(d => new ListItem(d.ToObject<JArray>()));
    }

    struct ListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public int Amount { get; set; }

        public ListItem(JArray array)
        {
            Id = array[0].ToObject<int>();
            Name = array[1].ToObject<string>();
            Amount = array[5].ToObject<int>();
            Price = array[6].ToObject<float>();
        }
    }
}

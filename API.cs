using CasinoRaid.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace CasinoRaid
{
    class API
    {
        public string Token { get; }
        public string SocketToken { get; set; }
        public int UserId { get; set; }
        public List<Item> Inventory { get; set; }
        public float TotalBalance => Balance + Inventory.Sum(i => i.Price);
        public float Balance { get; set; }

        public API(string token)
        {
            Token = token;

            UpdateInfo();
        }

        public IEnumerable<Item> ExchangeItems(Exchange exchange)
            => JsonConvert.DeserializeObject<JObject>(SendRequest("https://api.csgorun.org/marketplace/exchange-items", exchange, "POST"))["data"]["userItems"]["newItems"].ToObject<IEnumerable<Item>>();
        public IEnumerable<ListItem> BuyNearPrice(float price, int amount = 1)
        {
            var items = Items.Where(i => i.Amount > 0).Where(i => i.Price <= price).OrderByDescending(i => i.Price).Take(amount);

            ExchangeItems(new Exchange
            {
                UserItemIds = Inventory.Select(i => i.Id).ToArray(),
                WishItemIds = items.Select(i => i.Id).ToArray()
            });

            return items;
        }
        public void Bet(IEnumerable<Item> items, float crash)
        {
            SendRequest("https://api.csgorun.org/make-bet", new Bet
            {
                UserItemIds = items.Select(i => i.Id).ToArray(),
                Auto = crash.ToString("0.00").Replace(",", ".")
            }, "POST");
        }
        public void Bet(IEnumerable<ListItem> items, float crash)
        {
            SendRequest("https://api.csgorun.org/make-bet", new Bet
            {
                UserItemIds = items.Select(i => i.Id).ToArray(),
                Auto = crash.ToString("0.00").Replace(",", ".")
            }, "POST");
        }
        public void UpdateInfo()
        {
            var data = User;
            var user = data.User;

            Balance = user.Balance;
            Inventory = user.Items;
            UserId = user.Id;
            SocketToken = data.CentrifugeToken;
        }   
        public IEnumerable<ListItem> Items => JsonConvert.DeserializeObject<ItemsResponse>(SendRequest("https://cdn.csgorun.org/csgo/items.json")).Items;
        public StateData User => JsonConvert.DeserializeObject<State>(SendRequest("https://api.csgorun.org/current-state")).Data;
        public string SendRequest(string url, object param = null, string method = "GET")
        {
            var client = new WebClient();
            client.Headers.Add("authorization", $"JWT {Token}");

            if (param != null)
            {
                var data = JsonConvert.SerializeObject(param);
                client.Headers.Add("content-type", "application/json");
                return client.UploadString(url, method, data);
            }

            return client.DownloadString(url);
        }
    }

    public static class Utils
    {
        public static async Task Send(this WebSocket sock, string msg)
        {
            var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await sock.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> Read(this WebSocket sock)
        {
            var bytesReceived = new ArraySegment<byte>(new byte[1024]);
            var result = await sock.ReceiveAsync(bytesReceived, CancellationToken.None);

            return Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count);
        }
    }
}

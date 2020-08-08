using CasinoRaid.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CasinoRaid
{
    public class Player : SoundPlayer
    {
        public bool IsPlaying { get; set; } = false;

        public Player() : base() { }
        public Player(Stream stream) : base(stream) { }

        public void Start()
        {
            Play();
            IsPlaying = true;
        }
    }

    public class Settings
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("red_lenght")]
        public int RedLenght { get; set; }

        [JsonProperty("red_coeff")]
        public float RedCoeff { get; set; }

        [JsonProperty("coeff")]
        public float Coeff { get; set; }

        [JsonProperty("max_bet_percent")]
        public float MaxBetPercent { get; set; }

        [JsonProperty("normal_bet_percent")]
        public float NormalBetPercent { get; set; }

        [JsonProperty("max_bet_amount")]
        public float MaxBetAmount { get; set; }

        [JsonProperty("min_bet_amount")]
        public float MinBetAmount { get; set; }
    }

    class Program
    {
        public static Random Random = new Random();
        public static string[] LoseNames = new string[] { "CasinoRaid.lose.wav", "CasinoRaid.lose2.wav" };
        public static string[] WinNames = new string[] { "CasinoRaid.win.wav", "CasinoRaid.win2.wav" };
        public static Player WinPlayer = new Player();
        public static Player LosePlayer = new Player();
        public static Player StartPlayer = new Player(Assembly.GetExecutingAssembly().GetManifestResourceStream("CasinoRaid.start.wav"));
        public static Player FoundPlayer = new Player(Assembly.GetExecutingAssembly().GetManifestResourceStream("CasinoRaid.found.wav"));

        static void Handler(API api, Settings settings)
        {
            Start:

            StartPlayer.Play();

            var last = new List<float>();
            var redLenght = settings.RedLenght;
            var redCoeff = settings.RedCoeff;
            var coeff = settings.Coeff;
            var maxBetPercent = settings.MaxBetAmount;
            var normalBetPercent = settings.NormalBetPercent;
            var maxBetAmount = settings.MaxBetAmount;
            var minBetAmount = settings.MinBetAmount;
            var betted = false;
            var startBalance = api.TotalBalance;
            var maxBalance = startBalance;
            var minBalance = startBalance;
            var bet = startBalance * normalBetPercent;
            var losses = 0;
            var wins = 0;
            var makedBetCount = 0;

            var gameId = JsonConvert.DeserializeObject<JObject>(api.SendRequest("https://api.csgorun.org/current-state"))["data"]["game"]["history"][0]["id"].ToObject<int>();

            while (true)
            {
                try
                {
                    float crash;

                    try
                    {
                        crash = JsonConvert.DeserializeObject<JObject>(api.SendRequest($"https://api.csgorun.org/games/{++gameId}"))["data"]["crash"].ToObject<float>();
                        Thread.Sleep(1000);
                    } catch
                    {
                        gameId--;
                        continue;
                    }

                    LosePlayer.Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LoseNames.OrderBy(e => Random.NextDouble()).First());
                    WinPlayer.Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(WinNames.OrderBy(e => Random.NextDouble()).First());

                    last.Add(crash);
                    if (last.Count > redLenght)
                        last.RemoveAt(0);
                            
                    if (betted)
                    {
                        if (crash < coeff)
                        {
                            Console.WriteLine($"Lose -{bet}$");

                            losses++;
                            LosePlayer.Play();
                            // bet *= 1.2f;
                        }
                        else
                        {
                            Console.WriteLine($"Win +{bet * coeff} | {bet * (coeff- 1)}$");

                            wins++;
                            api.Balance += bet * coeff;
                            WinPlayer.Play();
                            // bet = api.TotalBalance * normalBetPercent;
                        }

                        Console.WriteLine($"Current Balance: {api.TotalBalance}$");
                        Console.WriteLine($"Wins: {wins}");
                        Console.WriteLine($"Losses: {losses}");

                        makedBetCount++;

                        if (makedBetCount == 2)
                            makedBetCount = 0;

                        betted = false;

                        bet = api.TotalBalance * normalBetPercent;
                    }

                    if (api.TotalBalance > maxBalance)
                        maxBalance = api.TotalBalance;

                    if (api.TotalBalance < minBalance)
                        minBalance = api.TotalBalance;

                    if (api.TotalBalance < maxBalance * 0.7f)
                        goto Out;

                    if (bet > maxBetAmount)
                        bet = maxBetAmount;

                    if (bet < minBetAmount)
                        bet = minBetAmount;

                    // if (bet > api.TotalBalance * maxBetPercent)
                    //     bet = api.TotalBalance * maxBetPercent;

                    if (last.Count == redLenght && last[0] <= redCoeff && last[1] <= redCoeff - 0.1f || makedBetCount > 0)
                    {
                        if (!LosePlayer.IsPlaying || !WinPlayer.IsPlaying)
                        {
                            FoundPlayer.Play();
                            LosePlayer.IsPlaying = false;
                            WinPlayer.IsPlaying = false;
                        }

                        Console.WriteLine($"\nMaking bet {bet}$");

                        api.BuyNearPrice(bet);
                        api.UpdateInfo();

                        Thread.Sleep(1000);

                        var betItems = api.Inventory.Where(i => i.Price <= bet).OrderBy(i => i.Price).Take(1);

                        Thread.Sleep(4000);

                        if (betItems.Any())
                            api.Bet(betItems, coeff);

                        betted = true;
                    }

                    Thread.Sleep(1000);

                    api.UpdateInfo();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n\nЖОПА ЖОПА ДЕД СДОХ: {e.Message}\n{e.StackTrace}\n\n");
                }
            }

            Out:
            Console.WriteLine("\n---------- Stopped ----------\n");
            Console.WriteLine($"Start Balance: {startBalance}$");
            Console.WriteLine($"Current Balance: {api.TotalBalance}$");
            Console.WriteLine($"Max Balance: {maxBalance}$");
            Console.WriteLine($"Min Balance: {minBalance}$");
            Console.WriteLine("\n---------- Stopped ----------\n");
            Console.WriteLine($"Казино зломано на {api.TotalBalance - startBalance}$, продолжить? (y\\n)");

            var ans = Console.ReadLine();

            if (ans == "y")
                goto Start;
        }

        static void Main(string[] args)
        {
            /* var data = File.ReadAllLines("datka.txt").Select(e => e.Split("|")[1].Replace(",", ".")).ToList();

            var balance = 65f;
            var balances = new List<string>();

            void updateBalance(float d, float mult = 1f) => balance += d >= 1.2f ? (balance * 0.05f * mult < 50f ? balance * 0.05f * mult : 50) * 0.2f : -(balance * 0.05f * mult < 50f ? balance * 0.05f * mult : 50);

            balances.Add(balance.ToString("0.00"));

            for (var i = 0; i < data.Count; i++)
            {
                try
                {
                    if (data[i] <= 1.2f && data[i + 1] <= 1.2f) // && data[i + 2] > 1.2f)
                    {
                        updateBalance(data[i + 2]);
                        balances.Add(balance.ToString("0.00"));

                        updateBalance(data[i + 3]);
                        balances.Add(balance.ToString("0.00"));

                        i++;
                    }
                } catch { }
            }

            File.WriteAllLines("dataout.txt", balances);

            Console.WriteLine(balances.Count); */
            Console.WriteLine("start processing..");

            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

            Handler(new API(settings.Token), settings);
        }
    }
}

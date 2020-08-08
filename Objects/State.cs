using System;
using System.Collections.Generic;
using System.Text;

namespace CasinoRaid.Objects
{
    class State
    {
        public bool Success { get; set; }
        public string Date { get; set; }
        public StateData Data { get; set; }
    }

    class StateData
    {
        public StateUser User { get; set; }
        public string CentrifugeToken { get; set; }
    }

    class StateUser
    {
        public float Balance { get; set; }
        public int Id { get; set; }
        public List<Item> Items { get; set; }
    }
}

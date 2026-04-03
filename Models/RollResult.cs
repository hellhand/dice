using System;
using System.Collections.Generic;

namespace Models
{
    public class RollResult
    {
        public int RollId { get; set; }
        public int DiceValue { get; set; }
        public DateTime RollDate { get; set; }
        public string User { get; set; }

        public RollResult(int rollId, int diceValue, string user)
        {
            RollId = rollId;
            DiceValue = diceValue;
            RollDate = DateTime.UtcNow;
            User = user;
        }
    }
}
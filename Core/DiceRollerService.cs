using System;
using System.Collections.Generic;
using System.Linq;

namespace DiceRoller.Core
{
    public class DieRoll
    {
        public int Value { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsCritical { get; set; }
        public bool WasRerolled { get; set; }
        public List<int> RerollValues { get; set; } = new List<int>();

        public DieRoll(int value, int threshold, bool isCritical)
        {
            Value = value;
            IsCritical = isCritical;
            EvaluateSuccess(threshold);
        }

        private void EvaluateSuccess(int threshold)
        {
            if (Value == 1)
            {
                IsSuccess = true;
            }
            else if (Value == 9 || Value == 10)
            {
                IsSuccess = false;
            }
            else
            {
                IsSuccess = Value <= threshold;
            }
        }

        public void AddReroll(int rerollValue, int threshold)
        {
            RerollValues.Add(rerollValue);
            WasRerolled = true;
            if (rerollValue <= threshold)
            {
                IsSuccess = true;
            }
        }

        public int GetTotalSuccesses()
        {
            int successCount = IsSuccess ? 1 : 0;
            successCount += RerollValues.Count(v => v <= 10 && v != 9);
            return successCount;
        }
    }

    public class DiceRollAction
    {
        public int NumDice { get; set; }
        public int Threshold { get; set; }
        public int Difficulty { get; set; }
        public List<DieRoll> DiceRolls { get; set; } = new List<DieRoll>();
        public int TotalSuccesses { get; set; }
        public int BonusSuccesses { get; set; }
        public bool IsCriticalFailure { get; set; }
        public bool IsSuccess { get; set; }
        public Random Rng { get; private set; }

        public DiceRollAction(int numDice, int threshold, int difficulty = 1)
        {
            NumDice = numDice;
            Threshold = threshold;
            Difficulty = difficulty;
            Rng = new Random();
        }

        public void Roll()
        {
            DiceRolls.Clear();
            TotalSuccesses = 0;
            BonusSuccesses = 0;
            IsCriticalFailure = false;
            IsSuccess = false;

            for (int i = 0; i < NumDice; i++)
            {
                int value = Rng.Next(1, 11);
                bool isCritical = (i == NumDice - 1);
                DieRoll roll = new DieRoll(value, Threshold, isCritical);

                while (roll.Value == 1 && roll.IsSuccess)
                {
                    int rerollValue = Rng.Next(1, 11);
                    roll.AddReroll(rerollValue, Threshold);
                    if (rerollValue != 1)
                    {
                        break;
                    }
                }

                DiceRolls.Add(roll);
            }

            TotalSuccesses = DiceRolls.Sum(d => d.GetTotalSuccesses());
            IsSuccess = TotalSuccesses >= Difficulty;
            if (IsSuccess)
            {
                BonusSuccesses = TotalSuccesses - Difficulty;
            }

            if (!IsSuccess)
            {
                var criticalDie = DiceRolls.FirstOrDefault(d => d.IsCritical);
                if (criticalDie != null && (criticalDie.Value == 9 || criticalDie.Value == 10))
                {
                    IsCriticalFailure = true;
                }
            }
        }

        public string GetSummary()
        {
            string summary = $"Rolled {NumDice}d10 with threshold {Threshold} (Difficulty: {Difficulty})\n";
            summary += $"Results: {string.Join(", ", DiceRolls.Select((d, i) => $"{(d.IsCritical ? "[CRIT] " : "")}{{d.Value}}"))}\n";
            summary += $"Total Successes: {TotalSuccesses}\n";
            
            if (IsSuccess)
            {
                summary += "✓ SUCCESS";
                if (BonusSuccesses > 0)
                {
                    summary += $" (+{{BonusSuccesses}} bonus success{{(BonusSuccesses > 1 ? "es" : "")}})";
                }
            }
            else
            {
                summary += "✗ FAILURE";
                if (IsCriticalFailure)
                {
                    summary += " (CRITICAL FAILURE)";
                }
            }

            return summary;
        }

        public string GetDetailedResults()
        {
            string details = GetSummary() + "\n\nDetailed Dice:\n";
            
            foreach (var roll in DiceRolls)
            {
                details += $"  Die: {{roll.Value}}";
                if (roll.IsCritical) details += " [CRITICAL DIE]";
                details += roll.IsSuccess ? " ✓" : " ✗";
                
                if (roll.RerollValues.Count > 0)
                {
                    details += $" (Rerolls: {{string.Join(", ", roll.RerollValues)}})";
                }
                details += "\n";
            }

            return details;
        }
    }

    public class DiceRollerService
    {
        private Random _random = new Random();

        public DiceRollAction PerformRoll(int numDice, int threshold, int difficulty = 1)
        {
            if (numDice <= 0)
                throw new ArgumentException("Number of dice must be positive", nameof(numDice));
            if (threshold < 0 || threshold > 10)
                throw new ArgumentException("Threshold must be between 0 and 10", nameof(threshold));
            if (difficulty <= 0)
                throw new ArgumentException("Difficulty must be positive", nameof(difficulty));

            var rollAction = new DiceRollAction(numDice, threshold, difficulty);
            rollAction.Roll();
            return rollAction;
        }

        public string EvaluateResultQuality(int successes)
        {
            if (successes <= 0) return "Critical Failure";
            if (successes == 1) return "Minor Success";
            if (successes == 2) return "Normal Success";
            if (successes >= 3 && successes < 5) return "Impressive Success";
            if (successes >= 5) return "Exceptional Success";
            return "Unknown";
        }
    }
}
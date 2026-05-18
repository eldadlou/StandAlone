using UnityEngine;

namespace MyGame.Game
{
    public enum Team
    {
        Player,
        AI,
        None
    }

    public class Player
    {
        public string Name { get; set; }
        public Team Team { get; set; }
        public Color TeamColor { get; set; }
        public bool IsAI { get; set; }
        public int AIDifficulty { get; set; } // 1-3 for AI difficulty levels
        
        // Team statistics
        public int UnitsCreated { get; set; }
        public int UnitsDestroyed { get; set; }
        public int UnitsLost { get; set; }
        
        // Resources (for future RTS features)
        public int Resources { get; set; }
        public int MaxResources { get; set; }

        public Player(string name, Team team, bool isAI = false)
        {
            Name = name;
            Team = team;
            IsAI = isAI;
            
            // Set team colors
            TeamColor = team == Team.Player ? Color.blue : Color.red;
            
            // Initialize AI difficulty if AI
            if (isAI)
            {
                AIDifficulty = 1; // Default difficulty
            }
            
            // Initialize resources
            Resources = 1000;
            MaxResources = 1000;
        }

        public void AddResources(int amount)
        {
            Resources = Mathf.Min(Resources + amount, MaxResources);
        }

        public bool SpendResources(int amount)
        {
            if (Resources >= amount)
            {
                Resources -= amount;
                return true;
            }
            return false;
        }

        public void UnitCreated()
        {
            UnitsCreated++;
        }

        public void UnitDestroyed()
        {
            UnitsDestroyed++;
        }

        public void UnitLost()
        {
            UnitsLost++;
        }

        public bool IsDefeated()
        {
            // Basic defeat condition - no units remaining
            // This will be enhanced by GameManager
            return false;
        }
    }
}

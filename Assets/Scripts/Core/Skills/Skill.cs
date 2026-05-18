using UnityEngine;
using MyGame.Core.Units;

namespace MyGame.Core.Skills
{
    public class Skill
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Cooldown { get; set; }
        public float LastUsedTime { get; set; }

        // Optionally, add logic for skill activation
        public virtual void Activate(IUnit user)
        {
            // Implement skill effect logic here
        }
    }
}

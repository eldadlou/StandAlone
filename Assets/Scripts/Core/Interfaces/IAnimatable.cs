using System;

namespace MyGame.Core.Interfaces
{
    public interface IAnimatable
    {
        void PlayAnimation(string animationName);
        event Action<string> OnAnimationEvent; // e.g., "Fire", "Reload"
    }
}

using MyGame.Core.Units;

namespace MyGame.Core.Services
{
    public interface ISelectionAudioFeedback
    {
        void PlaySelectionSound();
    }

    public interface ISelectionParticleFeedback
    {
        void SpawnSelectionEffect(IUnit unit);
    }
}

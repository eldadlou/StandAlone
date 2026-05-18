namespace MyGame.Core.Interfaces
{
    public interface IAttackable
    {
        void TakeDamage(float amount);
        float Health { get; }
    }
}
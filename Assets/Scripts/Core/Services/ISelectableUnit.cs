using MyGame.Core.Units;

namespace MyGame.Core.Services
{
    /// <summary>
    /// Selection feedback for units without referencing the concrete Unit type from higher layers.
    /// </summary>
    public interface ISelectableUnit : IUnit
    {
        void SetSelected(bool selected);
    }
}

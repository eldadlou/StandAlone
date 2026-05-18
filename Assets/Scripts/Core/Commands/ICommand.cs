namespace MyGame.Core.Commands
{
    /// <summary>
    /// Command pattern interface for undo/redo functionality
    /// </summary>
    public interface ICommand
    {
        void Execute();
        void Undo();
        bool CanExecute();
        string Description { get; }
    }
}

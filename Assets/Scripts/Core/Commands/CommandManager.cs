using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Core.Commands
{
    /// <summary>
    /// Manages command execution, undo, and redo functionality
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        
        [Header("Command Settings")]
        [SerializeField] private int maxUndoSteps = 50;
        [SerializeField] private bool enableUndoRedo = true;

        public bool CanUndo => _undoStack.Count > 0 && enableUndoRedo;
        public bool CanRedo => _redoStack.Count > 0 && enableUndoRedo;
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public void ExecuteCommand(ICommand command)
        {
            if (command == null || !command.CanExecute()) return;

            command.Execute();
            _undoStack.Push(command);
            
            // Clear redo stack when new command is executed
            _redoStack.Clear();
            
            // Limit undo stack size
            if (_undoStack.Count > maxUndoSteps)
            {
                var tempStack = new Stack<ICommand>();
                while (_undoStack.Count > maxUndoSteps)
                {
                    tempStack.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    _undoStack.Push(tempStack.Pop());
                }
            }
            
            Debug.Log($"Command executed: {command.Description}");
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            
            Debug.Log($"Command undone: {command.Description}");
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            
            Debug.Log($"Command redone: {command.Description}");
        }

        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            Debug.Log("Command history cleared");
        }

        public void SetMaxUndoSteps(int steps)
        {
            maxUndoSteps = Mathf.Max(1, steps);
        }

        public void SetUndoRedoEnabled(bool enabled)
        {
            enableUndoRedo = enabled;
        }
    }
}

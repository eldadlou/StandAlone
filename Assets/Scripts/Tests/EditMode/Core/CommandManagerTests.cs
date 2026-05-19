using NUnit.Framework;
using UnityEngine;
using MyGame.Core.Commands;

namespace MyGame.Tests.Core
{
    [TestFixture]
    [Category("Core")]
    public class CommandManagerTests
    {
        private CommandManager _commandManager;
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("CommandManagerTests");
            _commandManager = _host.AddComponent<CommandManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
                Object.DestroyImmediate(_host);
        }

        [Test]
        public void ExecuteCommand_WhenValid_PushesToUndoStack()
        {
            var command = new TestCommand("move");
            _commandManager.ExecuteCommand(command);

            Assert.AreEqual(1, _commandManager.UndoCount);
            Assert.IsTrue(command.WasExecuted);
        }

        [Test]
        public void ExecuteCommand_WhenNewCommandExecuted_ClearsRedoStack()
        {
            var first = new TestCommand("first");
            var second = new TestCommand("second");

            _commandManager.ExecuteCommand(first);
            _commandManager.Undo();
            _commandManager.ExecuteCommand(second);

            Assert.AreEqual(0, _commandManager.RedoCount);
            Assert.IsFalse(_commandManager.CanRedo);
        }

        [Test]
        public void Undo_WhenUndoStackNotEmpty_RestoresPreviousState()
        {
            var command = new TestCommand("attack");
            _commandManager.ExecuteCommand(command);
            _commandManager.Undo();

            Assert.IsTrue(command.WasUndone);
            Assert.AreEqual(1, _commandManager.RedoCount);
        }

        [Test]
        public void Redo_WhenRedoStackNotEmpty_ReappliesCommand()
        {
            var command = new TestCommand("attack");
            _commandManager.ExecuteCommand(command);
            _commandManager.Undo();
            _commandManager.Redo();

            Assert.AreEqual(2, command.ExecuteCount);
        }

        [Test]
        public void ClearHistory_WhenCalled_EmptiesUndoAndRedoStacks()
        {
            _commandManager.ExecuteCommand(new TestCommand("a"));
            _commandManager.ClearHistory();

            Assert.AreEqual(0, _commandManager.UndoCount);
            Assert.AreEqual(0, _commandManager.RedoCount);
        }

        private sealed class TestCommand : ICommand
        {
            private readonly string _description;

            public TestCommand(string description) => _description = description;

            public bool WasExecuted { get; private set; }
            public bool WasUndone { get; private set; }
            public int ExecuteCount { get; private set; }

            public string Description => _description;

            public bool CanExecute() => true;

            public void Execute()
            {
                ExecuteCount++;
                WasExecuted = true;
                WasUndone = false;
            }

            public void Undo()
            {
                WasUndone = true;
            }
        }
    }
}

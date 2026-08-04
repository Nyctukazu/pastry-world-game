using System.Collections.Generic;
using PastryWorld.Editor.Interfaces;

namespace PastryWorld.Editor.Commands;

public class CompositeCommand : ICommand
{
    private readonly List<ICommand> _commands = new();
    public string Name { get; }

    public CompositeCommand(string name, IEnumerable<ICommand> commands)
    {
        Name = name;
        _commands.AddRange(commands);
    }

    public void Execute()
    {
        foreach (var cmd in _commands)
        {
            cmd.Execute();
        }
    }

    public void Undo()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
    }
}
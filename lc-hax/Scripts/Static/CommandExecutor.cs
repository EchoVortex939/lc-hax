using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ZLinq;

static class CommandExecutor {
    static ValueEnumerable<ZLinq.Linq.ArrayWhere<Type>, Type> CommandTypes { get; } =
        Assembly.GetExecutingAssembly()
                .GetTypes()
                .AsValueEnumerable()
                .Where(type => typeof(ICommand).IsAssignableFrom(type));

    static Dictionary<string, ICommand> Commands { get; } =
        CommandExecutor.CommandTypes.Where(type => type.GetCustomAttribute<CommandAttribute>() is not null).ToDictionary(
            type => type.GetCustomAttribute<CommandAttribute>().Syntax,
            type => (ICommand)Activator.CreateInstance(type)
        );

    static Dictionary<string, ICommand> DebugCommands { get; } =
        CommandExecutor.CommandTypes.Where(type => type.GetCustomAttribute<DebugCommandAttribute>() is not null).ToDictionary(
            type => type.GetCustomAttribute<DebugCommandAttribute>().Syntax,
            type => (ICommand)new DebugCommand((ICommand)Activator.CreateInstance(type))
        );

    static Dictionary<string, ICommand> PrivilegeCommands { get; } =
        CommandExecutor.CommandTypes.Where(type => type.GetCustomAttribute<PrivilegedCommandAttribute>() is not null).ToDictionary(
            type => type.GetCustomAttribute<PrivilegedCommandAttribute>().Syntax,
            type => (ICommand)new PrivilegedCommand((ICommand)Activator.CreateInstance(type))
        );

    internal static async void Execute(string commandSyntax, string[]? args, Action<Result> onComplete) {
        try {
            Arguments arguments = args ?? [];

            ICommand? command =
                CommandExecutor.Commands.GetValue(commandSyntax) ??
                CommandExecutor.PrivilegeCommands.GetValue(commandSyntax) ??
                CommandExecutor.DebugCommands.GetValue(commandSyntax);

            if (command is null) {
                onComplete(new Result(false, $"Command '{commandSyntax}' not found"));
                return;
            }

            using CancellationTokenSource cancellationTokenSource = new();
            await command.Execute(arguments, cancellationTokenSource.Token);
            onComplete(new Result(true, $"Command '{commandSyntax}' executed successfully"));
        }

        catch (Exception exception) {
            Logger.Write(exception.ToString());
            onComplete(new Result(false, $"Error: {exception.Message}"));
        }
    }
}

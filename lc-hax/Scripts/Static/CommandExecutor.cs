using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

    internal static async Task<CommandResult> ExecuteAsync(string syntax, Arguments args, CommandInvocationSource source) {
        try {
            ICommand? command =
                CommandExecutor.Commands.GetValue(syntax) ??
                CommandExecutor.PrivilegeCommands.GetValue(syntax) ??
                CommandExecutor.DebugCommands.GetValue(syntax);

            if (command is null) {
                string message = "The command is not found!";
                Logger.Write($"Command not found: {syntax} (source: {source})");
                return new CommandResult(Success: false, Message: message);
            }

            using CancellationTokenSource cancellationTokenSource = new();
            await command.Execute(args, cancellationTokenSource.Token);

            return new CommandResult(Success: true);
        }
        catch (Exception exception) {
            Logger.Write(exception);
            return new CommandResult(Success: false, Message: exception.Message, Exception: exception);
        }
    }

    internal static async void ExecuteDirect(string syntax, params string[] args) {
        Arguments arguments = args;
        CommandResult result = await CommandExecutor.ExecuteAsync(syntax, arguments, CommandInvocationSource.Direct);

        if (!result.Success && result.Message is not null) {
            Logger.Write($"Command '{syntax}' failed: {result.Message}");
        }
    }
}

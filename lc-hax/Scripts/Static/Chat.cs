using System;
using UnityEngine.EventSystems;

static class Chat {
    internal static event Action<string>? OnExecuteCommandAttempt;

    internal static void Clear() {
        Helper.HUDManager?.AddTextToChatOnServer(
            $"</color>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<color=#FFFFFF00>"
        );
    }

    internal static void Print(string name, string? message, bool isSystem = false) {
        if (string.IsNullOrWhiteSpace(message)) return;
        if (Helper.HUDManager is not HUDManager hudManager) return;

        hudManager.AddChatMessage(message, name);

        if (!isSystem && hudManager.localPlayer.isTypingChat) {
            hudManager.localPlayer.isTypingChat = false;
            hudManager.typingIndicator.enabled = false;
            hudManager.chatTextField.text = "";
            hudManager.PingHUDElement(hudManager.Chat, 1.0f, 1.0f, 0.2f);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    internal static void Print(string? message) => Chat.Print("SYSTEM", message, true);

    internal static void Print(string? message, params string[] args) => Chat.Print($"{message}\n{string.Join('\n', args)}");

    internal static async void ExecuteCommand(string commandString) {
        try {
            Chat.Print("USER", commandString);
            Chat.OnExecuteCommandAttempt?.Invoke(commandString);

            Arguments args = commandString[1..].Split(' ');
            string? syntax = args[0];
            Arguments commandArgs = args[1..];

            if (syntax is null) return;

            CommandResult result = await CommandExecutor.ExecuteAsync(syntax, commandArgs, CommandInvocationSource.Chat);

            if (!result.Success && result.Message is not null) {
                Chat.Print(result.Message);
            }
        }
        catch (Exception exception) {
            Logger.Write(exception.ToString());
        }
    }
}

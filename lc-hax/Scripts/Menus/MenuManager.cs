using System.Collections.Generic;
using UnityEngine;

static class TeleportationMenuManager {
    static Stack<IMenu> MenuStack { get; } = new();
    internal static IMenu? CurrentMenu { get; private set; }
    internal static string? StatusMessage { get; set; }

    internal static void PushMenu(IMenu menu) {
        MenuStack.Push(menu);
        CurrentMenu = menu;
    }

    internal static void PopMenu() {
        if (MenuStack.Count > 0) {
            _ = MenuStack.Pop();
        }

        CurrentMenu = MenuStack.Count > 0 ? MenuStack.Peek() : null;
    }

    internal static void DrawCurrentMenu() {
        CurrentMenu?.OnGUI();
        DrawStatusMessage();
    }

    static void DrawStatusMessage() {
        if (string.IsNullOrEmpty(StatusMessage)) return;

        GUIStyle statusStyle = new(GUI.skin.label) {
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        statusStyle.normal.textColor = Color.yellow;
        Vector2 statusSize = statusStyle.CalcSize(new GUIContent(StatusMessage));
        Rect statusRect = new(
            (Screen.width * 0.5f) - (statusSize.x * 0.5f),
            Screen.height * 0.9f,
            statusSize.x,
            statusSize.y
        );

        GUI.Label(statusRect, StatusMessage, statusStyle);
    }
}

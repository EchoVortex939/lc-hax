using UnityEngine;

sealed class CommandMenuMod : MonoBehaviour {
    readonly struct CommandButton {
        internal string Label { get; init; }
        internal string CommandSyntax { get; init; }
        internal string[]? Args { get; init; }
    }

    bool InGame { get; set; }
    bool IsVisible { get; set; }
    string StatusMessage { get; set; } = string.Empty;
    float StatusMessageTime { get; set; }
    const float StatusMessageDuration = 3.0f;

    static CommandButton[] CommandButtons { get; } = [
        new() { Label = "God Mode", CommandSyntax = "god", Args = null },
        new() { Label = "NoClip", CommandSyntax = "noclip", Args = null },
        new() { Label = "Heal Self", CommandSyntax = "heal", Args = null },
        new() { Label = "Invisible", CommandSyntax = "invis", Args = null },
        new() { Label = "Unlimited Jump", CommandSyntax = "jump", Args = null }
    ];

    Rect WindowRect { get; set; }
    const float WindowWidth = 300f;
    const float WindowHeight = 280f;

    void OnEnable() {
        InputListener.OnInsertPress += this.ToggleMenu;
        GameListener.OnGameStart += this.OnGameStart;
        GameListener.OnGameEnd += this.OnGameEnd;
        ScreenListener.OnScreenSizeChange += this.UpdateWindowPosition;
        this.UpdateWindowPosition();
    }

    void OnDisable() {
        InputListener.OnInsertPress -= this.ToggleMenu;
        GameListener.OnGameStart -= this.OnGameStart;
        GameListener.OnGameEnd -= this.OnGameEnd;
        ScreenListener.OnScreenSizeChange -= this.UpdateWindowPosition;
    }

    void Update() {
        if (this.StatusMessageTime > 0f) {
            this.StatusMessageTime -= Time.deltaTime;
            if (this.StatusMessageTime <= 0f) {
                this.StatusMessage = string.Empty;
            }
        }
    }

    void OnGUI() {
        if (!this.IsVisible) return;
        if (!this.InGame) return;

        this.UpdateWindowPosition();
        this.WindowRect = GUI.Window(12345, this.WindowRect, this.DrawWindow, "Command Menu");
    }

    void DrawWindow(int windowID) {
        GUILayout.BeginVertical();

        GUILayout.Space(10);

        foreach (CommandButton button in CommandMenuMod.CommandButtons) {
            if (GUILayout.Button(button.Label, GUILayout.Height(30))) {
                this.ExecuteCommand(button);
            }
            GUILayout.Space(5);
        }

        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(this.StatusMessage)) {
            GUIStyle statusStyle = new(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            };

            GUILayout.Label(this.StatusMessage, statusStyle);
        }

        GUILayout.EndVertical();
    }

    void ExecuteCommand(CommandButton button) {
        CommandExecutor.Execute(button.CommandSyntax, button.Args, result => {
            this.StatusMessage = result.Success ? $"✓ {button.Label}" : $"✗ {result.Message ?? "Failed"}";
            this.StatusMessageTime = CommandMenuMod.StatusMessageDuration;
        });
    }

    void ToggleMenu() => this.IsVisible = !this.IsVisible;

    void OnGameStart() => this.InGame = true;

    void OnGameEnd() {
        this.InGame = false;
        this.IsVisible = false;
        this.StatusMessage = string.Empty;
        this.StatusMessageTime = 0f;
    }

    void UpdateWindowPosition() {
        float x = (Screen.width - CommandMenuMod.WindowWidth) * 0.5f;
        float y = (Screen.height - CommandMenuMod.WindowHeight) * 0.5f;
        this.WindowRect = new Rect(x, y, CommandMenuMod.WindowWidth, CommandMenuMod.WindowHeight);
    }
}

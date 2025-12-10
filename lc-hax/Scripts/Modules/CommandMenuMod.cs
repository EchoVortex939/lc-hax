using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

sealed class CommandMenuMod : MonoBehaviour {
    bool MenuVisible { get; set; }
    float ScrollPosition { get; set; }
    float TabScrollPosition { get; set; }

    int CurrentCategoryIndex { get; set; }
    int CurrentCommandIndex { get; set; }
    int SelectedPlayerIndex { get; set; } = -1;

    CommandCategory[] Categories { get; set; } = [];
    List<string> CurrentPlayers { get; set; } = [];

    bool IsWaitingForInput { get; set; }
    string InputBuffer { get; set; } = "";
    string SecondInputBuffer { get; set; } = "";
    string CurrentInputPrompt { get; set; } = "";
    string SecondInputPrompt { get; set; } = "";
    CommandInfo? PendingCommand { get; set; }
    bool IsWaitingForSecondInput { get; set; }

    struct CommandInfo {
        internal string name;
        internal string syntax;
        internal bool isPrivileged;
        internal string[] requiredParams;
        internal bool needsPlayerTarget;
    }

    struct CommandCategory {
        internal string name;
        internal CommandInfo[] commands;
    }

    void OnEnable() {
        InputListener.OnInsertPress += this.ToggleMenu;
        InputListener.OnMPress += this.ToggleMenu;
        InputListener.OnNumpad8Press += this.NavigateUp;
        InputListener.OnNumpad2Press += this.NavigateDown;
        InputListener.OnNumpad5Press += this.ExecuteSelected;
        InputListener.OnNumpad4Press += this.PreviousTab;
        InputListener.OnNumpad6Press += this.NextTab;
        this.InitializeCategories();
    }

    void OnDisable() {
        InputListener.OnInsertPress -= this.ToggleMenu;
        InputListener.OnMPress -= this.ToggleMenu;
        InputListener.OnNumpad8Press -= this.NavigateUp;
        InputListener.OnNumpad2Press -= this.NavigateDown;
        InputListener.OnNumpad5Press -= this.ExecuteSelected;
        InputListener.OnNumpad4Press -= this.PreviousTab;
        InputListener.OnNumpad6Press -= this.NextTab;
    }

    void InitializeCategories() {
        this.Categories = [
            new CommandCategory {
                name = "Teleportation",
                commands = [
                    new CommandInfo { name = "Exit", syntax = "exit", requiredParams = [] },
                    new CommandInfo { name = "Enter", syntax = "enter", requiredParams = [] },
                    new CommandInfo { name = "Teleport", syntax = "tp", requiredParams = ["x", "y", "z"] },
                    new CommandInfo { name = "Void", syntax = "void", requiredParams = [] },
                    new CommandInfo { name = "Home", syntax = "home", requiredParams = [] },
                    new CommandInfo { name = "Mob", syntax = "mob", requiredParams = [] },
                    new CommandInfo { name = "Random", syntax = "random", requiredParams = [] },
                ]
            },
            new CommandCategory {
                name = "Combat/Effects",
                commands = [
                    new CommandInfo { name = "Noise", syntax = "noise", requiredParams = ["duration"] },
                    new CommandInfo { name = "Bomb", syntax = "bomb", requiredParams = [] },
                    new CommandInfo { name = "Bombard", syntax = "bombard", requiredParams = [] },
                    new CommandInfo { name = "Hate", syntax = "hate", requiredParams = [] },
                    new CommandInfo { name = "Mask", syntax = "mask", requiredParams = ["amount"] },
                    new CommandInfo { name = "Fatality", syntax = "fatality", requiredParams = [] },
                    new CommandInfo { name = "Poison", syntax = "poison", requiredParams = ["damage", "duration", "delay"] },
                    new CommandInfo { name = "Stun", syntax = "stun", requiredParams = ["duration"] },
                    new CommandInfo { name = "Stun Click", syntax = "stunclick", requiredParams = [] },
                    new CommandInfo { name = "Kill Click", syntax = "killclick", requiredParams = [] },
                    new CommandInfo { name = "Kill", syntax = "kill", requiredParams = [] },
                ]
            },
            new CommandCategory {
                name = "Utilities",
                commands = [
                    new CommandInfo { name = "Say", syntax = "say", requiredParams = ["message"], needsPlayerTarget = true },
                    new CommandInfo { name = "Translate", syntax = "translate", requiredParams = ["language", "message"] },
                    new CommandInfo { name = "Signal", syntax = "signal", requiredParams = ["unlockable"] },
                    new CommandInfo { name = "Shovel", syntax = "shovel", requiredParams = ["force"] },
                    new CommandInfo { name = "Experience", syntax = "xp", requiredParams = ["amount"] },
                    new CommandInfo { name = "Buy", syntax = "buy", requiredParams = ["item", "quantity"] },
                    new CommandInfo { name = "Sell", syntax = "sell", requiredParams = ["item"] },
                    new CommandInfo { name = "Grab", syntax = "grab", requiredParams = ["item"] },
                    new CommandInfo { name = "Destroy", syntax = "destroy", requiredParams = [] },
                ]
            },
            new CommandCategory {
                name = "Building/World",
                commands = [
                    new CommandInfo { name = "Block", syntax = "block", requiredParams = [] },
                    new CommandInfo { name = "Build", syntax = "build", requiredParams = ["unlockable"] },
                    new CommandInfo { name = "Suit", syntax = "suit", requiredParams = ["suit"] },
                    new CommandInfo { name = "Visit", syntax = "visit", requiredParams = ["moon"] },
                    new CommandInfo { name = "Spin", syntax = "spin", requiredParams = ["duration"] },
                    new CommandInfo { name = "Upright", syntax = "upright", requiredParams = [] },
                    new CommandInfo { name = "Horn", syntax = "horn", requiredParams = ["duration"] },
                    new CommandInfo { name = "Unlock", syntax = "unlock", requiredParams = [] },
                    new CommandInfo { name = "Lock", syntax = "lock", requiredParams = [] },
                    new CommandInfo { name = "Open", syntax = "open", requiredParams = [] },
                    new CommandInfo { name = "Close", syntax = "close", requiredParams = [] },
                    new CommandInfo { name = "Garage", syntax = "garage", requiredParams = [] },
                    new CommandInfo { name = "Explode", syntax = "explode", requiredParams = [] },
                    new CommandInfo { name = "Berserk", syntax = "berserk", requiredParams = [] },
                    new CommandInfo { name = "Light", syntax = "light", requiredParams = [] },
                ]
            },
            new CommandCategory {
                name = "Player Toggles",
                commands = [
                    new CommandInfo { name = "God Mode", syntax = "god", requiredParams = [] },
                    new CommandInfo { name = "No Clip", syntax = "noclip", requiredParams = [] },
                    new CommandInfo { name = "Jump", syntax = "jump", requiredParams = [] },
                    new CommandInfo { name = "Rapid Fire", syntax = "rapid", requiredParams = [] },
                    new CommandInfo { name = "Hear", syntax = "hear", requiredParams = [] },
                    new CommandInfo { name = "Fake Death", syntax = "fakedeath", requiredParams = [] },
                    new CommandInfo { name = "Invisibility", syntax = "invis", requiredParams = [] },
                ]
            },
            new CommandCategory {
                name = "Game Controls",
                commands = [
                    new CommandInfo { name = "Start Game", syntax = "start", requiredParams = [] },
                    new CommandInfo { name = "End Game", syntax = "end", requiredParams = [] },
                    new CommandInfo { name = "Heal", syntax = "heal", requiredParams = [] },
                    new CommandInfo { name = "Players", syntax = "players", requiredParams = [] },
                    new CommandInfo { name = "Coordinates", syntax = "xyz", requiredParams = [] },
                    new CommandInfo { name = "Beta", syntax = "beta", requiredParams = [] },
                    new CommandInfo { name = "Clear", syntax = "clear", requiredParams = [] },
                    new CommandInfo { name = "Lobby", syntax = "lobby", requiredParams = [] },
                ]
            },
            new CommandCategory {
                name = "Players",
                commands = []
            },
            new CommandCategory {
                name = "Privileged (Host)",
                commands = [
                    new CommandInfo { name = "Time Scale", syntax = "timescale", requiredParams = ["scale"], isPrivileged = true },
                    new CommandInfo { name = "Quota", syntax = "quota", requiredParams = ["amount"], isPrivileged = true },
                    new CommandInfo { name = "Spawn Enemy", syntax = "spawn", requiredParams = ["enemy", "amount"], isPrivileged = true },
                    new CommandInfo { name = "Credit", syntax = "credit", requiredParams = ["amount"], isPrivileged = true },
                    new CommandInfo { name = "Land", syntax = "land", requiredParams = [], isPrivileged = true },
                    new CommandInfo { name = "Eject", syntax = "eject", requiredParams = [], isPrivileged = true },
                    new CommandInfo { name = "Revive", syntax = "revive", requiredParams = [], isPrivileged = true },
                    new CommandInfo { name = "Gods", syntax = "gods", requiredParams = [], isPrivileged = true },
                ]
            },
        ];
    }

    void ToggleMenu() {
        this.MenuVisible = !this.MenuVisible;
        this.CurrentCommandIndex = 0;
        this.SelectedPlayerIndex = -1;
    }

    void NavigateUp() {
        if (!this.MenuVisible) return;

        if (this.IsWaitingForInput) return;

        this.CurrentCommandIndex--;
        if (this.CurrentCommandIndex < 0) {
            CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];
            this.CurrentCommandIndex = currentCategory.commands.Length - 1;
        }
    }

    void NavigateDown() {
        if (!this.MenuVisible) return;

        if (this.IsWaitingForInput) return;

        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];
        this.CurrentCommandIndex++;
        if (this.CurrentCommandIndex >= currentCategory.commands.Length) {
            this.CurrentCommandIndex = 0;
        }
    }

    void PreviousTab() {
        if (!this.MenuVisible || this.IsWaitingForInput) return;

        this.CurrentCategoryIndex--;
        if (this.CurrentCategoryIndex < 0) {
            this.CurrentCategoryIndex = this.Categories.Length - 1;
        }
        this.CurrentCommandIndex = 0;
        this.SelectedPlayerIndex = -1;
    }

    void NextTab() {
        if (!this.MenuVisible || this.IsWaitingForInput) return;

        this.CurrentCategoryIndex++;
        if (this.CurrentCategoryIndex >= this.Categories.Length) {
            this.CurrentCategoryIndex = 0;
        }
        this.CurrentCommandIndex = 0;
        this.SelectedPlayerIndex = -1;
    }

    void ExecuteSelected() {
        if (!this.MenuVisible) return;

        if (this.IsWaitingForInput) {
            this.SubmitInput();
            return;
        }

        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];
        if (this.CurrentCommandIndex < 0 || this.CurrentCommandIndex >= currentCategory.commands.Length) return;

        CommandInfo command = currentCategory.commands[this.CurrentCommandIndex];

        bool isHost = Helper.LocalPlayer?.IsHost ?? false;
        if (command.isPrivileged && !isHost) {
            MenuManager.StatusMessage = "Host only command!";
            return;
        }

        if (command.requiredParams.Length > 0 || command.needsPlayerTarget) {
            this.StartParameterInput(command);
        }
        else {
            this.ExecuteCommand(command, []);
        }
    }

    void StartParameterInput(CommandInfo command) {
        this.IsWaitingForInput = true;
        this.PendingCommand = command;
        this.InputBuffer = "";
        this.SecondInputBuffer = "";

        if (command.needsPlayerTarget && this.SelectedPlayerIndex >= 0) {
            if (command.requiredParams.Length > 0) {
                this.CurrentInputPrompt = $"Enter {command.requiredParams[0]} for player '{this.CurrentPlayers[this.SelectedPlayerIndex]}':";
            }
            else {
                this.ExecuteCommand(command, [this.CurrentPlayers[this.SelectedPlayerIndex]]);
                this.CancelInput();
                return;
            }
        }
        else if (command.requiredParams.Length > 0) {
            this.CurrentInputPrompt = $"Enter {command.requiredParams[0]}:";
        }

        if (command.requiredParams.Length > 1) {
            this.SecondInputPrompt = $"Enter {command.requiredParams[1]}:";
            this.IsWaitingForSecondInput = false;
        }
    }

    void SubmitInput() {
        if (!this.IsWaitingForInput || this.PendingCommand is null) return;

        CommandInfo command = this.PendingCommand.Value;

        if (!this.IsWaitingForSecondInput && command.requiredParams.Length > 1) {
            if (string.IsNullOrEmpty(this.InputBuffer)) return;

            this.IsWaitingForSecondInput = true;
            return;
        }

        List<string> parameters = [];

        if (command.needsPlayerTarget && this.SelectedPlayerIndex >= 0) {
            parameters.Add(this.CurrentPlayers[this.SelectedPlayerIndex]);
        }

        if (!string.IsNullOrEmpty(this.InputBuffer)) {
            parameters.Add(this.InputBuffer);
        }

        if (this.IsWaitingForSecondInput && !string.IsNullOrEmpty(this.SecondInputBuffer)) {
            parameters.Add(this.SecondInputBuffer);
        }

        this.ExecuteCommand(command, [.. parameters]);
        this.CancelInput();
    }

    void CancelInput() {
        this.IsWaitingForInput = false;
        this.IsWaitingForSecondInput = false;
        this.PendingCommand = null;
        this.InputBuffer = "";
        this.SecondInputBuffer = "";
        this.CurrentInputPrompt = "";
        this.SecondInputPrompt = "";
    }

    void ExecuteCommand(CommandInfo command, string[] parameters) {
        _ = Task.Run(async () => {
            try {
                Arguments args = parameters.Length > 0 ? Arguments.FromCommand(string.Join(" ", parameters)) : new Arguments { Span = System.Array.Empty<string>() };
                CommandResult result = await CommandExecutor.ExecuteAsync(command.syntax, args, CommandInvocationSource.Direct);

                MenuManager.StatusMessage = result.Success ? $"Command '{command.syntax}' executed successfully!" : result.Message ?? $"Command '{command.syntax}' failed";
            }
            catch (Exception e) {
                MenuManager.StatusMessage = $"Error executing command: {e.Message}";
            }
        });
    }

    void Update() {
        if (!this.MenuVisible) return;

        this.UpdatePlayerList();
    }

    void UpdatePlayerList() {
        var players = Helper.Players?.Where(p => p is not null).Select(p => p!.PlayerUsername()).ToList();
        if (players is not null) {
            this.CurrentPlayers = players;
        }
    }

    void OnGUI() {
        if (!this.MenuVisible) return;

        int windowWidth = 800;
        int windowHeight = 700;
        Rect menuRect = new((Screen.width - windowWidth) / 2, (Screen.height - windowHeight) / 2, windowWidth, windowHeight);

        _ = GUILayout.Window(0, menuRect, this.DrawMenuWindow, "Command Menu", GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));
    }

    void DrawMenuWindow(int windowID) {
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        this.DrawTabs();

        GUILayout.Space(10);

        if (this.Categories[this.CurrentCategoryIndex].name == "Players") {
            this.DrawPlayersList();
        }
        else {
            this.DrawCommands();
        }

        GUILayout.Space(10);

        this.DrawInputPrompt();

        GUILayout.Space(10);

        if (GUILayout.Button("Close (M/Insert)", GUILayout.Height(30))) {
            this.ToggleMenu();
        }

        this.DrawHelpText();

        GUILayout.EndVertical();
    }

    void DrawTabs() {
        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(35));

        float tabWidth = 120;
        float totalTabWidth = this.Categories.Length * tabWidth;
        float availableWidth = 780;

        if (totalTabWidth > availableWidth) {
            this.TabScrollPosition = GUILayout.HorizontalScrollbar(this.TabScrollPosition, availableWidth, 0, totalTabWidth - availableWidth, GUILayout.Width(780));
        }

        GUILayout.BeginHorizontal(GUILayout.Width(availableWidth));
        _ = this.TabScrollPosition;
        for (int i = 0; i < this.Categories.Length; i++) {
            bool isSelected = i == this.CurrentCategoryIndex;
            GUIStyle tabStyle = new(GUI.skin.button) {
                fixedWidth = tabWidth,
                fixedHeight = 30
            };

            if (isSelected) {
                tabStyle.normal.background = tabStyle.active.background;
                tabStyle.fontStyle = FontStyle.Bold;
            }

            if (GUILayout.Button(this.Categories[i].name, tabStyle)) {
                this.CurrentCategoryIndex = i;
                this.CurrentCommandIndex = 0;
                this.SelectedPlayerIndex = -1;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
    }

    void DrawPlayersList() {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(300));

        GUILayout.Label("Select a player to target with commands:", GUI.skin.box);

        this.ScrollPosition = GUILayout.BeginScrollView(new Vector2(0, this.ScrollPosition), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)).y;

        for (int i = 0; i < this.CurrentPlayers.Count; i++) {
            bool isSelected = i == this.SelectedPlayerIndex;
            GUIStyle playerStyle = new(GUI.skin.button);

            if (isSelected) {
                playerStyle.normal.background = GUI.skin.button.active.background;
                playerStyle.fontStyle = FontStyle.Bold;
            }

            if (GUILayout.Button(this.CurrentPlayers[i], playerStyle, GUILayout.Height(30))) {
                this.SelectedPlayerIndex = i;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void DrawCommands() {
        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];

        GUILayout.Label(currentCategory.name, GUI.skin.box);

        bool isHost = Helper.LocalPlayer?.IsHost ?? false;

        this.ScrollPosition = GUILayout.BeginScrollView(new Vector2(0, this.ScrollPosition), GUILayout.Height(300), GUILayout.ExpandWidth(true)).y;

        for (int i = 0; i < currentCategory.commands.Length; i++) {
            CommandInfo cmd = currentCategory.commands[i];
            bool isSelected = i == this.CurrentCommandIndex;

            GUIStyle commandStyle = new(GUI.skin.button) {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 35
            };

            if (isSelected) {
                commandStyle.normal.background = GUI.skin.button.active.background;
                commandStyle.fontStyle = FontStyle.Bold;
            }

            string buttonText = cmd.name;
            if (cmd.isPrivileged && !isHost) {
                buttonText += " [HOST ONLY]";
                GUI.enabled = false;
                commandStyle.normal.textColor = Color.gray;
            }

            string paramInfo = cmd.requiredParams.Length > 0 ? $" ({string.Join(", ", cmd.requiredParams)})" : "";
            string fullText = buttonText + paramInfo;

            if (GUILayout.Button(fullText, commandStyle)) {
                this.CurrentCommandIndex = i;
                if (!cmd.isPrivileged || isHost) {
                    if (cmd.requiredParams.Length > 0 || cmd.needsPlayerTarget) {
                        this.StartParameterInput(cmd);
                    }
                    else {
                        this.ExecuteCommand(cmd, []);
                    }
                }
            }

            GUI.enabled = true;
        }

        GUILayout.EndScrollView();
    }

    void DrawInputPrompt() {
        if (!this.IsWaitingForInput) return;

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(80));

        if (!string.IsNullOrEmpty(this.CurrentInputPrompt)) {
            GUILayout.Label(this.CurrentInputPrompt, GUI.skin.box);
            this.InputBuffer = GUILayout.TextField(this.InputBuffer, GUI.skin.textField, GUILayout.Height(25));
        }

        if (this.IsWaitingForSecondInput && !string.IsNullOrEmpty(this.SecondInputPrompt)) {
            GUILayout.Label(this.SecondInputPrompt, GUI.skin.box);
            this.SecondInputBuffer = GUILayout.TextField(this.SecondInputBuffer, GUI.skin.textField, GUILayout.Height(25));
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Submit (Numpad 5)", GUILayout.Height(25))) {
            this.SubmitInput();
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(25))) {
            this.CancelInput();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void DrawHelpText() {
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Numpad: 4=Prev Tab, 6=Next Tab, 8=Up, 2=Down, 5=Select/Confirm", GUI.skin.label);
        GUILayout.Label("Mouse: Click tabs/commands, Enter parameters in input fields", GUI.skin.label);
        if (this.SelectedPlayerIndex >= 0) {
            GUILayout.Label($"Selected Player: {this.CurrentPlayers[this.SelectedPlayerIndex]}", GUI.skin.label);
        }
        GUILayout.EndVertical();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

sealed class AdvancedCommandMenuMod : MonoBehaviour {
    bool MenuVisible { get; set; }
    int CurrentCategoryIndex { get; set; }
    int CurrentItemIndex { get; set; }

    // Draggable window support
    bool IsDragging { get; set; }
    Vector2 DragOffset { get; set; }
    Vector2 windowPosition = new(50, 50);
    public Vector2 WindowPosition { get => this.windowPosition; set => this.windowPosition = value; }
    const int WindowWidth = 600;
    const int WindowHeight = 700;

    // Menu state
    public MenuState CurrentState { get; set; } = MenuState.CategorySelection;
    public PlayerControllerB? SelectedPlayer { get; set; }
    public BaseMenuScreen? CurrentScreen { get; set; } = null;

    // Screen management
    public readonly Stack<BaseMenuScreen> screenStack = new();

    public enum MenuState {
        CategorySelection,
        PlayerSelection,
        ParameterInput,
        PlayerCommandSelection
    }

    struct CommandInfo {
        internal string name;
        internal string syntax;
        internal bool isPrivileged;
        internal string[] parameters;
    }

    struct CommandCategory {
        internal string name;
        internal CommandInfo[] commands;
    }

    enum PlayerCommandType {
        Teleportation,
        EffectsAttacks,
        HealthState,
        BuildingItems,
        HostOnly
    }

    void OnEnable() {
        Logger.Write("AdvancedCommandMenuMod: OnEnable called");
        InputListener.OnInsertPress += this.ToggleMenu;
        InputListener.OnMPress += this.ToggleMenu;
        InputListener.OnNumpad8Press += this.NavigateUp;
        InputListener.OnNumpad2Press += this.NavigateDown;
        InputListener.OnNumpad5Press += this.ExecuteSelected;
        InputListener.OnNumpad4Press += this.NavigatePreviousTab;
        InputListener.OnNumpad6Press += this.NavigateNextTab;
        InputListener.OnBackspacePress += this.HandleBackNavigation;

        InitializeCategories();
        this.LoadWindowPosition();
        Logger.Write("AdvancedCommandMenuMod: Subscribed to all events");
    }

    void OnDisable() {
        Logger.Write("AdvancedCommandMenuMod: OnDisable called");
        InputListener.OnInsertPress -= this.ToggleMenu;
        InputListener.OnMPress -= this.ToggleMenu;
        InputListener.OnNumpad8Press -= this.NavigateUp;
        InputListener.OnNumpad2Press -= this.NavigateDown;
        InputListener.OnNumpad5Press -= this.ExecuteSelected;
        InputListener.OnNumpad4Press -= this.NavigatePreviousTab;
        InputListener.OnNumpad6Press -= this.NavigateNextTab;
        InputListener.OnBackspacePress -= this.HandleBackNavigation;

        this.SaveWindowPosition();
    }

    static void InitializeCategories() {
        // Initialize the existing categories from the original CommandMenuMod
        // plus the new Players category
    }

    void ToggleMenu() {
        this.MenuVisible = !this.MenuVisible;
        if (this.MenuVisible) {
            this.CurrentState = MenuState.CategorySelection;
            this.CurrentCategoryIndex = 0;
            this.CurrentItemIndex = 0;
            this.screenStack.Clear();
        }
        Logger.Write($"AdvancedCommandMenuMod: Menu toggled to {this.MenuVisible}");
    }

    void NavigatePreviousTab() {
        if (!this.MenuVisible) return;

        if (this.CurrentState == MenuState.CategorySelection) {
            this.CurrentCategoryIndex--;
            if (this.CurrentCategoryIndex < 0) {
                this.CurrentCategoryIndex = GetTotalCategories() - 1;
            }
            this.CurrentItemIndex = 0;
        }
    }

    void NavigateNextTab() {
        if (!this.MenuVisible) return;

        if (this.CurrentState == MenuState.CategorySelection) {
            this.CurrentCategoryIndex++;
            if (this.CurrentCategoryIndex >= GetTotalCategories()) {
                this.CurrentCategoryIndex = 0;
            }
            this.CurrentItemIndex = 0;
        }
    }

    void HandleBackNavigation() {
        if (!this.MenuVisible) return;

        if (this.screenStack.Count > 0) {
            this.CurrentScreen = this.screenStack.Pop();
        }
        else {
            this.CurrentState = MenuState.CategorySelection;
            this.CurrentScreen = null;
        }
    }

    void NavigateUp() {
        if (!this.MenuVisible) return;

        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.CurrentItemIndex--;
                break;
            case MenuState.PlayerSelection:
                this.CurrentScreen?.NavigateUp();
                break;
            case MenuState.ParameterInput:
                this.CurrentScreen?.NavigateUp();
                break;
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.NavigateUp();
                break;
            default:
                break;
        }
    }

    void NavigateDown() {
        if (!this.MenuVisible) return;

        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.CurrentItemIndex++;
                break;
            case MenuState.PlayerSelection:
                this.CurrentScreen?.NavigateDown();
                break;
            case MenuState.ParameterInput:
                this.CurrentScreen?.NavigateDown();
                break;
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.NavigateDown();
                break;
            default:
                break;
        }
    }

    void ExecuteSelected() {
        if (!this.MenuVisible) return;

        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.HandleCategorySelection();
                break;
            case MenuState.PlayerSelection:
            case MenuState.ParameterInput:
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.ExecuteSelected();
                break;
            default:
                break;
        }
    }

    void HandleCategorySelection() {
        if (this.CurrentCategoryIndex == GetTotalCategories() - 1) {
            // Players tab
            this.CurrentState = MenuState.PlayerSelection;
            this.CurrentScreen = new PlayersMenuScreen(this);
        }
        else {
            // Regular category - execute command directly
            this.ExecuteRegularCommand();
        }
    }

    void ExecuteRegularCommand() {
        CommandCategory category = GetCategory(this.CurrentCategoryIndex);
        if (this.CurrentItemIndex < 0 || this.CurrentItemIndex >= category.commands.Length) return;

        CommandInfo command = category.commands[this.CurrentItemIndex];

        bool isHost = Helper.LocalPlayer?.IsHost ?? false;
        if (command.isPrivileged && !isHost) {
            TeleportationMenuManager.StatusMessage = "Command requires host privileges!";
            return;
        }

        _ = Task.Run(async () => {
            CommandResult result = await CommandExecutor.ExecuteAsync(command.syntax, new Arguments { Span = [] }, CommandInvocationSource.Direct);
            if (!result.Success && result.Message is not null) {
                Chat.Print(result.Message);
            }
        });
    }

    static int GetTotalCategories() => GetCategories().Length;

    static CommandCategory[] GetCategories() {
        return [
            new CommandCategory {
                name = "Teleportation",
                commands = [
                    new CommandInfo { name = "Exit", syntax = "exit" },
                    new CommandInfo { name = "Enter", syntax = "enter" },
                    new CommandInfo { name = "Teleport", syntax = "tp" },
                    new CommandInfo { name = "Void", syntax = "void" },
                    new CommandInfo { name = "Home", syntax = "home" },
                    new CommandInfo { name = "Mob", syntax = "mob" },
                    new CommandInfo { name = "Random", syntax = "random" },
                ]
            },
            new CommandCategory {
                name = "Combat",
                commands = [
                    new CommandInfo { name = "Noise", syntax = "noise", parameters = ["player", "duration"] },
                    new CommandInfo { name = "Bomb", syntax = "bomb", parameters = ["player"] },
                    new CommandInfo { name = "Bombard", syntax = "bombard", parameters = ["player"] },
                    new CommandInfo { name = "Hate", syntax = "hate", parameters = ["player"] },
                    new CommandInfo { name = "Mask", syntax = "mask", parameters = ["player", "amount"] },
                    new CommandInfo { name = "Fatality", syntax = "fatality", parameters = ["player", "enemy"] },
                    new CommandInfo { name = "Poison", syntax = "poison", parameters = ["player", "damage", "duration", "delay"] },
                    new CommandInfo { name = "Stun", syntax = "stun", parameters = ["player"] },
                    new CommandInfo { name = "Kill", syntax = "kill", parameters = ["player"] },
                ]
            },
            new CommandCategory {
                name = "Utilities",
                commands = [
                    new CommandInfo { name = "Say", syntax = "say", parameters = ["player", "message"] },
                    new CommandInfo { name = "Translate", syntax = "translate" },
                    new CommandInfo { name = "Buy", syntax = "buy", parameters = ["item", "quantity"] },
                    new CommandInfo { name = "Grab", syntax = "grab", parameters = ["item"] },
                ]
            },
            new CommandCategory {
                name = "World",
                commands = [
                    new CommandInfo { name = "Block", syntax = "block" },
                    new CommandInfo { name = "Build", syntax = "build" },
                    new CommandInfo { name = "Suit", syntax = "suit", parameters = ["suit"] },
                    new CommandInfo { name = "Visit", syntax = "visit" },
                    new CommandInfo { name = "Spin", syntax = "spin" },
                    new CommandInfo { name = "Explode", syntax = "explode" },
                ]
            },
            new CommandCategory {
                name = "Toggles",
                commands = [
                    new CommandInfo { name = "God Mode", syntax = "god" },
                    new CommandInfo { name = "No Clip", syntax = "noclip" },
                    new CommandInfo { name = "Jump", syntax = "jump" },
                    new CommandInfo { name = "Rapid Fire", syntax = "rapid" },
                    new CommandInfo { name = "Fake Death", syntax = "fakedeath" },
                    new CommandInfo { name = "Invisibility", syntax = "invis" },
                ]
            },
            new CommandCategory {
                name = "Game",
                commands = [
                    new CommandInfo { name = "Start Game", syntax = "start" },
                    new CommandInfo { name = "End Game", syntax = "end" },
                    new CommandInfo { name = "Heal", syntax = "heal", parameters = ["player"] },
                    new CommandInfo { name = "Players", syntax = "players" },
                    new CommandInfo { name = "Clear", syntax = "clear" },
                ]
            },
            new CommandCategory {
                name = "Privileged",
                commands = [
                    new CommandInfo { name = "Spawn Enemy", syntax = "spawn", parameters = ["enemy", "player", "amount"], isPrivileged = true },
                    new CommandInfo { name = "Credit", syntax = "credit", parameters = ["amount"], isPrivileged = true },
                    new CommandInfo { name = "Land", syntax = "land", isPrivileged = true },
                    new CommandInfo { name = "Eject", syntax = "eject", isPrivileged = true },
                    new CommandInfo { name = "Revive", syntax = "revive", isPrivileged = true },
                ]
            },
            new CommandCategory {
                name = "Players",
                commands = [] // Special handling for Players tab
            }
        ];
    }

    static CommandCategory GetCategory(int index) => GetCategories()[index];

    void LoadWindowPosition() {
        Vector2 position = this.WindowPosition;
        position.x = PlayerPrefs.GetFloat("AdvancedMenu_X", 50);
        position.y = PlayerPrefs.GetFloat("AdvancedMenu_Y", 50);
        this.WindowPosition = position;
    }

    void SaveWindowPosition() {
        PlayerPrefs.SetFloat("AdvancedMenu_X", this.WindowPosition.x);
        PlayerPrefs.SetFloat("AdvancedMenu_Y", this.WindowPosition.y);
        PlayerPrefs.Save();
    }

    void KeepWindowInBounds() {
        Vector2 position = this.WindowPosition;
        position.x = Mathf.Clamp(position.x, 0, Screen.width - WindowWidth);
        position.y = Mathf.Clamp(position.y, 0, Screen.height - WindowHeight);
        this.WindowPosition = position;
    }

    void HandleDragging() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; // Flip Y coordinate

            Rect titleBarRect = new(this.WindowPosition.x, this.WindowPosition.y, WindowWidth, 30);
            if (titleBarRect.Contains(mousePos)) {
                this.IsDragging = true;
                this.DragOffset = mousePos - this.WindowPosition;
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            this.IsDragging = false;
        }

        if (this.IsDragging) {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; // Flip Y coordinate
            Vector2 position = mousePos - this.DragOffset;
            this.WindowPosition = position;
            this.KeepWindowInBounds();
        }
    }

    void OnGUI() {
        if (!this.MenuVisible) return;

        Logger.Write("AdvancedCommandMenuMod: Drawing menu");
        this.HandleDragging();

        GUI.Box(new Rect(this.WindowPosition.x, this.WindowPosition.y, WindowWidth, WindowHeight), GUIContent.none);

        GUILayout.BeginArea(new Rect(this.WindowPosition.x, this.WindowPosition.y, WindowWidth, WindowHeight));

        // Title bar for dragging
        GUILayout.BeginHorizontal();
        GUILayout.Label("Advanced Command Menu", GUILayout.Height(30));
        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20))) {
            this.ToggleMenu();
        }
        GUILayout.EndHorizontal();

        this.DrawContent();

        GUILayout.EndArea();
    }

    void DrawContent() {
        switch (this.CurrentState) {
            case MenuState.CategorySelection:
                this.DrawCategorySelection();
                break;
            case MenuState.PlayerSelection:
            case MenuState.ParameterInput:
            case MenuState.PlayerCommandSelection:
                this.CurrentScreen?.Draw();
                break;
            default:
                break;
        }
    }

    void DrawCategorySelection() {
        CommandCategory[] categories = GetCategories();

        // Draw category tabs
        GUILayout.BeginHorizontal();
        for (int i = 0; i < categories.Length; i++) {
            GUIStyle tabStyle = new(GUI.skin.button);
            if (i == this.CurrentCategoryIndex) {
                tabStyle.normal.background = GUI.skin.box.normal.background;
            }

            if (GUILayout.Button(categories[i].name, tabStyle, GUILayout.Height(30))) {
                this.CurrentCategoryIndex = i;
                this.CurrentItemIndex = 0;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Draw commands for current category
        if (this.CurrentCategoryIndex == categories.Length - 1) {
            // Players tab - show message to press numpad 5
            GUILayout.Label("Players Tab - Press Numpad 5 to scan lobby", GUI.skin.box, GUILayout.Height(50));
        }
        else {
            CommandCategory currentCategory = categories[this.CurrentCategoryIndex];
            this.DrawCommandsForCategory(currentCategory);
        }

        GUILayout.Space(10);
        GUILayout.Label("Numpad: 4=Prev Tab, 6=Next Tab, 8=Up, 2=Down, 5=Select, M=Close", GUI.skin.label);
    }

    void DrawCommandsForCategory(CommandCategory category) {
        GUILayout.Label(category.name, GUI.skin.box);

        bool isHost = Helper.LocalPlayer?.IsHost ?? false;

        int maxVisibleItems = 8;
        int startIndex = Mathf.Max(0, this.CurrentItemIndex - (maxVisibleItems / 2));
        int endIndex = Mathf.Min(category.commands.Length, startIndex + maxVisibleItems);

        if (endIndex - startIndex < maxVisibleItems && startIndex > 0) {
            startIndex = Mathf.Max(0, endIndex - maxVisibleItems);
        }

        for (int i = startIndex; i < endIndex; i++) {
            CommandInfo cmd = category.commands[i];

            if (i == this.CurrentItemIndex) {
                GUI.color = Color.green;
                GUI.backgroundColor = Color.green;
            }

            string buttonText = cmd.name;
            if (cmd.isPrivileged && !isHost) {
                buttonText += " [HOST ONLY]";
                GUI.color = Color.gray;
                GUI.enabled = false;
            }

            if (GUILayout.Button(buttonText, GUILayout.Height(35))) {
                this.CurrentItemIndex = i;
                if (!cmd.isPrivileged || isHost) {
                    this.ExecuteRegularCommand();
                }
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }
    }
}

using System.Threading.Tasks;
using UnityEngine;

sealed class CommandMenuMod : MonoBehaviour {
    bool MenuVisible { get; set; }
    int CurrentCategoryIndex { get; set; }
    int CurrentCommandIndex { get; set; }
    CommandCategory[] Categories { get; set; } = [];

    struct CommandInfo {
        internal string name;
        internal string syntax;
        internal bool isPrivileged;
    }

    struct CommandCategory {
        internal string name;
        internal CommandInfo[] commands;
    }

    void OnEnable() {
        Logger.Write("CommandMenuMod: OnEnable called");
        InputListener.OnInsertPress += this.ToggleMenu;
        InputListener.OnMPress += this.ToggleMenu;
        InputListener.OnNumpad8Press += this.NavigateUp;
        InputListener.OnNumpad2Press += this.NavigateDown;
        InputListener.OnNumpad5Press += this.ExecuteSelected;
        this.InitializeCategories();
        Logger.Write("CommandMenuMod: Subscribed to all events");
    }

    void OnDisable() {
        Logger.Write("CommandMenuMod: OnDisable called");
        InputListener.OnInsertPress -= this.ToggleMenu;
        InputListener.OnMPress -= this.ToggleMenu;
        InputListener.OnNumpad8Press -= this.NavigateUp;
        InputListener.OnNumpad2Press -= this.NavigateDown;
        InputListener.OnNumpad5Press -= this.ExecuteSelected;
    }

    void InitializeCategories() {
        this.Categories =
        [
            new CommandCategory {
                name = "Teleportation",
                commands =
                [
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
                name = "Combat/Effects",
                commands =
                [
                    new CommandInfo { name = "Noise", syntax = "noise" },
                    new CommandInfo { name = "Bomb", syntax = "bomb" },
                    new CommandInfo { name = "Bombard", syntax = "bombard" },
                    new CommandInfo { name = "Hate", syntax = "hate" },
                    new CommandInfo { name = "Mask", syntax = "mask" },
                    new CommandInfo { name = "Fatality", syntax = "fatality" },
                    new CommandInfo { name = "Poison", syntax = "poison" },
                    new CommandInfo { name = "Stun", syntax = "stun" },
                    new CommandInfo { name = "Stun Click", syntax = "stunclick" },
                    new CommandInfo { name = "Kill Click", syntax = "killclick" },
                    new CommandInfo { name = "Kill", syntax = "kill" },
                ]
            },
            new CommandCategory {
                name = "Utilities",
                commands =
                [
                    new CommandInfo { name = "Say", syntax = "say" },
                    new CommandInfo { name = "Translate", syntax = "translate" },
                    new CommandInfo { name = "Signal", syntax = "signal" },
                    new CommandInfo { name = "Shovel", syntax = "shovel" },
                    new CommandInfo { name = "Experience", syntax = "xp" },
                    new CommandInfo { name = "Buy", syntax = "buy" },
                    new CommandInfo { name = "Sell", syntax = "sell" },
                    new CommandInfo { name = "Grab", syntax = "grab" },
                    new CommandInfo { name = "Destroy", syntax = "destroy" },
                ]
            },
            new CommandCategory {
                name = "Building/World",
                commands =
                [
                    new CommandInfo { name = "Block", syntax = "block" },
                    new CommandInfo { name = "Build", syntax = "build" },
                    new CommandInfo { name = "Suit", syntax = "suit" },
                    new CommandInfo { name = "Visit", syntax = "visit" },
                    new CommandInfo { name = "Spin", syntax = "spin" },
                    new CommandInfo { name = "Upright", syntax = "upright" },
                    new CommandInfo { name = "Horn", syntax = "horn" },
                    new CommandInfo { name = "Unlock", syntax = "unlock" },
                    new CommandInfo { name = "Lock", syntax = "lock" },
                    new CommandInfo { name = "Open", syntax = "open" },
                    new CommandInfo { name = "Close", syntax = "close" },
                    new CommandInfo { name = "Garage", syntax = "garage" },
                    new CommandInfo { name = "Explode", syntax = "explode" },
                    new CommandInfo { name = "Berserk", syntax = "berserk" },
                    new CommandInfo { name = "Light", syntax = "light" },
                ]
            },
            new CommandCategory {
                name = "Player Toggles",
                commands =
                [
                    new CommandInfo { name = "God Mode", syntax = "god" },
                    new CommandInfo { name = "No Clip", syntax = "noclip" },
                    new CommandInfo { name = "Jump", syntax = "jump" },
                    new CommandInfo { name = "Rapid Fire", syntax = "rapid" },
                    new CommandInfo { name = "Hear", syntax = "hear" },
                    new CommandInfo { name = "Fake Death", syntax = "fakedeath" },
                    new CommandInfo { name = "Invisibility", syntax = "invis" },
                ]
            },
            new CommandCategory {
                name = "Game Controls",
                commands =
                [
                    new CommandInfo { name = "Start Game", syntax = "start" },
                    new CommandInfo { name = "End Game", syntax = "end" },
                    new CommandInfo { name = "Heal", syntax = "heal" },
                    new CommandInfo { name = "Players", syntax = "players" },
                    new CommandInfo { name = "Coordinates", syntax = "xyz" },
                    new CommandInfo { name = "Beta", syntax = "beta" },
                    new CommandInfo { name = "Clear", syntax = "clear" },
                    new CommandInfo { name = "Lobby", syntax = "lobby" },
                ]
            },
            new CommandCategory {
                name = "Privileged (Host)",
                commands =
                [
                    new CommandInfo { name = "Time Scale", syntax = "timescale", isPrivileged = true },
                    new CommandInfo { name = "Quota", syntax = "quota", isPrivileged = true },
                    new CommandInfo { name = "Spawn Enemy", syntax = "spawn", isPrivileged = true },
                    new CommandInfo { name = "Credit", syntax = "credit", isPrivileged = true },
                    new CommandInfo { name = "Land", syntax = "land", isPrivileged = true },
                    new CommandInfo { name = "Eject", syntax = "eject", isPrivileged = true },
                    new CommandInfo { name = "Revive", syntax = "revive", isPrivileged = true },
                    new CommandInfo { name = "Gods", syntax = "gods", isPrivileged = true },
                ]
            },
        ];
    }

    void ToggleMenu() {
        this.MenuVisible = !this.MenuVisible;
        this.CurrentCommandIndex = 0;
        Logger.Write($"CommandMenuMod: Menu toggled to {this.MenuVisible}");
    }

    void NavigateUp() {
        if (!this.MenuVisible || this.Categories.Length == 0) return;

        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];
        if (currentCategory.commands.Length == 0) return;

        this.CurrentCommandIndex--;
        if (this.CurrentCommandIndex < 0) {
            this.CurrentCommandIndex = currentCategory.commands.Length - 1;
        }
    }

    void NavigateDown() {
        if (!this.MenuVisible || this.Categories.Length == 0) return;

        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];
        if (currentCategory.commands.Length == 0) return;

        this.CurrentCommandIndex++;
        if (this.CurrentCommandIndex >= currentCategory.commands.Length) {
            this.CurrentCommandIndex = 0;
        }
    }

    void ExecuteSelected() {
        if (!this.MenuVisible || this.Categories.Length == 0) return;

        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];
        if (this.CurrentCommandIndex < 0 || this.CurrentCommandIndex >= currentCategory.commands.Length) {
            return;
        }

        CommandInfo command = currentCategory.commands[this.CurrentCommandIndex];
        CommandMenuMod.ExecuteCommand(command);
    }

    static void ExecuteCommand(CommandInfo command) {
        _ = Task.Run(async () => {
            CommandResult result = await CommandExecutor.ExecuteAsync(command.syntax, new Arguments { Span = [] }, CommandInvocationSource.Direct);
            if (!result.Success && result.Message is not null) {
                Chat.Print(result.Message);
            }
        });
    }

    void OnGUI() {
        if (!this.MenuVisible) return;

        Logger.Write("CommandMenuMod: Drawing menu");
        GUILayout.BeginArea(new Rect(50, 50, 500, 600));

        this.DrawMenu();

        GUILayout.EndArea();
    }

    void DrawMenu() {
        GUILayout.Label("Command Menu", GUI.skin.box, GUILayout.Height(40));

        if (this.Categories.Length == 0) {
            GUILayout.Label("No categories available");
            return;
        }

        GUILayout.BeginHorizontal();
        for (int i = 0; i < this.Categories.Length; i++) {
            if (i == this.CurrentCategoryIndex) {
                GUI.color = Color.cyan;
            }

            if (GUILayout.Button(this.Categories[i].name, GUILayout.Height(30))) {
                this.CurrentCategoryIndex = i;
                this.CurrentCommandIndex = 0;
            }

            GUI.color = Color.white;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        this.DrawCommands();

        GUILayout.Space(10);

        if (GUILayout.Button("Close (M)", GUILayout.Height(40))) {
            this.ToggleMenu();
        }

        GUILayout.Label("Numpad: 8=Up, 2=Down, 5=Select", GUI.skin.label);
    }

    void DrawCommands() {
        CommandCategory currentCategory = this.Categories[this.CurrentCategoryIndex];

        GUILayout.Label(currentCategory.name, GUI.skin.box);

        bool isHost = Helper.LocalPlayer?.IsHost ?? false;

        _ = GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(300));

        for (int i = 0; i < currentCategory.commands.Length; i++) {
            CommandInfo cmd = currentCategory.commands[i];

            if (i == this.CurrentCommandIndex) {
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
                this.CurrentCommandIndex = i;
                if (!cmd.isPrivileged || isHost) {
                    CommandMenuMod.ExecuteCommand(cmd);
                }
            }

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        GUILayout.EndScrollView();
    }
}

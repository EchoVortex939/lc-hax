using GameNetcodeStuff;
using UnityEngine;

sealed class TeleportPlayerToPlayerMenu : IMenu {
    int SourcePlayerIndex { get; set; } = -1;
    int DestinationPlayerIndex { get; set; } = -1;
    SelectionMode CurrentMode { get; set; } = SelectionMode.Source;

    enum SelectionMode {
        Source,
        Destination
    }

    public void OnGUI() {
        GUILayout.Label("Teleport Player to Player", GUI.skin.box);

        PlayerControllerB[] activePlayers = Helper.ActivePlayers;

        if (activePlayers.Length < 2) {
            GUILayout.Label("Need at least 2 active players");
        }
        else {
            GUILayout.Label(this.CurrentMode == SelectionMode.Source ? "Select Source Player:" : "Select Destination Player:");
            _ = GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(200));

            for (int i = 0; i < activePlayers.Length; i++) {
                PlayerControllerB player = activePlayers[i];

                bool isSourceSelected = this.SourcePlayerIndex == i;
                bool isDestSelected = this.DestinationPlayerIndex == i;

                if (this.CurrentMode == SelectionMode.Source) {
                    if (isSourceSelected) {
                        GUI.color = Color.green;
                    }

                    if (GUILayout.Button($"{player.playerUsername}", GUILayout.Height(30))) {
                        this.SourcePlayerIndex = isSourceSelected ? -1 : i;
                        if (!isSourceSelected) {
                            this.CurrentMode = SelectionMode.Destination;
                        }
                    }
                }
                else {
                    if (i == this.SourcePlayerIndex) {
                        GUI.color = Color.cyan;
                    }
                    else if (isDestSelected) {
                        GUI.color = Color.green;
                    }

                    if (GUILayout.Button($"{player.playerUsername}", GUILayout.Height(30))) {
                        if (i != this.SourcePlayerIndex) {
                            this.DestinationPlayerIndex = isDestSelected ? -1 : i;
                        }
                    }
                }

                GUI.color = Color.white;
            }

            GUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        GUILayout.Label($"Source: {(this.SourcePlayerIndex >= 0 && this.SourcePlayerIndex < activePlayers.Length ? activePlayers[this.SourcePlayerIndex].playerUsername : "None")}");
        GUILayout.Label($"Destination: {(this.DestinationPlayerIndex >= 0 && this.DestinationPlayerIndex < activePlayers.Length ? activePlayers[this.DestinationPlayerIndex].playerUsername : "None")}");

        GUILayout.Space(10);

        bool canTeleport = this.SourcePlayerIndex >= 0 && this.DestinationPlayerIndex >= 0 && this.SourcePlayerIndex != this.DestinationPlayerIndex && this.SourcePlayerIndex < activePlayers.Length && this.DestinationPlayerIndex < activePlayers.Length;

        GUI.enabled = canTeleport;
        if (GUILayout.Button("Teleport", GUILayout.Height(40))) {
            this.ExecuteTeleport();
        }
        GUI.enabled = true;

        GUILayout.Space(5);

        if (GUILayout.Button("Back", GUILayout.Height(40))) {
            this.OnBack();
        }
    }

    public void OnBack() => TeleportationMenuManager.PopMenu();

    void ExecuteTeleport() {
        PlayerControllerB[] activePlayers = Helper.ActivePlayers;

        if (this.SourcePlayerIndex < 0 || this.DestinationPlayerIndex < 0 || this.SourcePlayerIndex >= activePlayers.Length || this.DestinationPlayerIndex >= activePlayers.Length) {
            TeleportationMenuManager.StatusMessage = "Invalid player selection";
            return;
        }

        if (this.SourcePlayerIndex == this.DestinationPlayerIndex) {
            TeleportationMenuManager.StatusMessage = "Source and destination cannot be the same player";
            return;
        }

        PlayerControllerB sourcePlayer = activePlayers[this.SourcePlayerIndex];
        PlayerControllerB destPlayer = activePlayers[this.DestinationPlayerIndex];

        CommandResult result = CommandExecutor.ExecuteDirect("tp", sourcePlayer.playerUsername, destPlayer.playerUsername);
        TeleportationMenuManager.StatusMessage = result.Success ? "Teleported successfully" : result.Message ?? "Teleportation failed";
    }
}

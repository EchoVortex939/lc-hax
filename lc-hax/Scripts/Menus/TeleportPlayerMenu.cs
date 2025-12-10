using GameNetcodeStuff;
using System.Threading.Tasks;
using UnityEngine;

sealed class TeleportPlayerMenu : IMenu {
    int SelectedPlayerIndex { get; set; } = -1;

    public void OnGUI() {
        GUILayout.Label("Teleport to Player", GUI.skin.box);

        PlayerControllerB[] activePlayers = Helper.ActivePlayers;

        if (activePlayers.Length == 0) {
            GUILayout.Label("No active players available");
        }
        else {
            GUILayout.Label("Select a player:");
            _ = GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(200));

            for (int i = 0; i < activePlayers.Length; i++) {
                PlayerControllerB player = activePlayers[i];

                if (player.IsSelf()) {
                    continue;
                }

                bool isSelected = this.SelectedPlayerIndex == i;
                if (isSelected) {
                    GUI.color = Color.green;
                }

                if (GUILayout.Button($"{player.playerUsername}", GUILayout.Height(30))) {
                    this.SelectedPlayerIndex = isSelected ? -1 : i;
                }

                GUI.color = Color.white;
            }

            GUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        if (this.SelectedPlayerIndex >= 0 && this.SelectedPlayerIndex < activePlayers.Length) {
            if (GUILayout.Button("Teleport", GUILayout.Height(40))) {
                this.ExecuteTeleport();
            }
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Back", GUILayout.Height(40))) {
            this.OnBack();
        }
    }

    public void OnBack() => TeleportationMenuManager.PopMenu();

    void ExecuteTeleport() {
        PlayerControllerB[] activePlayers = Helper.ActivePlayers;

        if (this.SelectedPlayerIndex < 0 || this.SelectedPlayerIndex >= activePlayers.Length) {
            TeleportationMenuManager.StatusMessage = "Invalid player selection";
            return;
        }

        PlayerControllerB selectedPlayer = activePlayers[this.SelectedPlayerIndex];
        _ = Task.Run(async () => {
            CommandResult result = await CommandExecutor.ExecuteAsync("tp", new[] { selectedPlayer.playerUsername }, CommandInvocationSource.Direct);
            TeleportationMenuManager.StatusMessage = result.Success ? "Teleported successfully" : result.Message ?? "Teleportation failed";
        });
    }
}

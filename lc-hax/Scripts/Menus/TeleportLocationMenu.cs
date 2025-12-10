using System.Threading.Tasks;
using UnityEngine;

sealed class TeleportLocationMenu : IMenu {
    string XInput { get; set; } = "";
    string YInput { get; set; } = "";
    string ZInput { get; set; } = "";
    string XError { get; set; } = "";
    string YError { get; set; } = "";
    string ZError { get; set; } = "";

    public void OnGUI() {
        GUILayout.Label("Teleport to Location", GUI.skin.box);

        GUILayout.Label("X Coordinate:");
        this.XInput = GUILayout.TextField(this.XInput);
        if (!string.IsNullOrEmpty(this.XError)) {
            GUILayout.Label(this.XError, new GUIStyle(GUI.skin.label) {
                normal = { textColor = Color.red }
            });
        }

        GUILayout.Space(5);

        GUILayout.Label("Y Coordinate:");
        this.YInput = GUILayout.TextField(this.YInput);
        if (!string.IsNullOrEmpty(this.YError)) {
            GUILayout.Label(this.YError, new GUIStyle(GUI.skin.label) {
                normal = { textColor = Color.red }
            });
        }

        GUILayout.Space(5);

        GUILayout.Label("Z Coordinate:");
        this.ZInput = GUILayout.TextField(this.ZInput);
        if (!string.IsNullOrEmpty(this.ZError)) {
            GUILayout.Label(this.ZError, new GUIStyle(GUI.skin.label) {
                normal = { textColor = Color.red }
            });
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Teleport", GUILayout.Height(40))) {
            this.ExecuteTeleport();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Back", GUILayout.Height(40))) {
            this.OnBack();
        }
    }

    public void OnBack() => TeleportationMenuManager.PopMenu();

    void ExecuteTeleport() {
        this.XError = "";
        this.YError = "";
        this.ZError = "";

        bool isValidX = float.TryParse(this.XInput, out float x);
        bool isValidY = float.TryParse(this.YInput, out float y);
        bool isValidZ = float.TryParse(this.ZInput, out float z);

        if (!isValidX) this.XError = "Invalid X coordinate";
        if (!isValidY) this.YError = "Invalid Y coordinate";
        if (!isValidZ) this.ZError = "Invalid Z coordinate";

        if (!isValidX || !isValidY || !isValidZ) return;

        _ = Task.Run(async () => {
            CommandResult result = await CommandExecutor.ExecuteAsync("tp", new[] { x.ToString(), y.ToString(), z.ToString() }, CommandInvocationSource.Direct);
            TeleportationMenuManager.StatusMessage = result.Success ? "Teleported successfully" : result.Message ?? "Teleportation failed";
        });
    }
}

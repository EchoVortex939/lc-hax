using UnityEngine;

sealed class TeleportMenu : IMenu {
    public void OnGUI() {
        GUILayout.Label("Teleport", GUI.skin.box);

        if (GUILayout.Button("Location", GUILayout.Height(40))) {
            TeleportationMenuManager.PushMenu(new TeleportLocationMenu());
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Player", GUILayout.Height(40))) {
            TeleportationMenuManager.PushMenu(new TeleportPlayerMenu());
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Player → Player", GUILayout.Height(40))) {
            TeleportationMenuManager.PushMenu(new TeleportPlayerToPlayerMenu());
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Back", GUILayout.Height(40))) {
            this.OnBack();
        }
    }

    public void OnBack() => TeleportationMenuManager.PopMenu();
}

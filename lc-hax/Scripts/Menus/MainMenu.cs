using UnityEngine;

sealed class MainMenu : IMenu {
    public void OnGUI() {
        GUILayout.Label("Main Menu", GUI.skin.box);

        if (GUILayout.Button("Teleport", GUILayout.Height(40))) {
            TeleportationMenuManager.PushMenu(new TeleportMenu());
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Close Menu", GUILayout.Height(40))) {
            TeleportationMenuManager.PopMenu();
        }
    }

    public void OnBack() => TeleportationMenuManager.PopMenu();
}

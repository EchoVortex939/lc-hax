using UnityEngine;

sealed class MenuGUIMod : MonoBehaviour {
    bool MenuVisible { get; set; }

    void OnEnable() => InputListener.OnMPress += this.ToggleMenu;

    void OnDisable() => InputListener.OnMPress -= this.ToggleMenu;

    void OnGUI() {
        if (!this.MenuVisible) return;

        GUILayout.BeginArea(new Rect(50, 50, 400, 500));
        TeleportationMenuManager.DrawCurrentMenu();
        GUILayout.EndArea();
    }

    void ToggleMenu() {
        this.MenuVisible = !this.MenuVisible;

        if (this.MenuVisible && TeleportationMenuManager.CurrentMenu is null) {
            TeleportationMenuManager.PushMenu(new MainMenu());
        }
    }
}

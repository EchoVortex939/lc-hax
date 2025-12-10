using UnityEngine;

namespace Hax.GUI;

sealed class GUIMod : MonoBehaviour {
    internal MenuStack MenuStack { get; } = new();

    void OnGUI() {
        if (this.MenuStack.Current == null) return;

        this.MenuStack.Current.Draw();
    }

    internal void PushMenu(MenuScreen screen) {
        screen.Stack = this.MenuStack;
        this.MenuStack.Push(screen);
    }

    internal void PopMenu() => _ = this.MenuStack.Pop();

    internal void ClearMenus() => this.MenuStack.Clear();
}

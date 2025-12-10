using System.Collections.Generic;

namespace Hax.GUI;

sealed class MenuStack {
    Stack<MenuScreen> StackFrames { get; } = new();

    internal MenuScreen? Current { get; private set; }

    internal void Push(MenuScreen screen) {
        this.Current?.OnExit();
        this.StackFrames.Push(screen);
        this.Current = screen;
        this.Current.OnEnter();
    }

    internal MenuScreen? Pop() {
        if (this.StackFrames.Count == 0) return null;

        this.Current?.OnExit();
        _ = this.StackFrames.Pop();
        this.Current = this.StackFrames.Count > 0 ? this.StackFrames.Peek() : null;
        this.Current?.OnEnter();
        return this.Current;
    }

    internal void Clear() {
        while (this.StackFrames.Count > 0) {
            this.Current?.OnExit();
            _ = this.StackFrames.Pop();
        }

        this.Current = null;
    }
}

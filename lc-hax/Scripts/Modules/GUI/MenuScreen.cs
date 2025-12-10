namespace Hax.GUI;

internal abstract class MenuScreen {
    internal MenuStack? Stack { get; set; }

    internal abstract void Draw();

    internal virtual void OnEnter() {
    }

    internal virtual void OnExit() {
    }

    protected void Back() => this.Stack?.Pop();
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGUI = UnityEngine.GUI;
using UnityGUILayout = UnityEngine.GUILayout;

namespace Hax.GUI;

sealed class DualSelectionMenuScreen : MenuScreen {
    const float ButtonHeight = 30f;
    const float ButtonSpacing = 5f;
    const float ScrollSpeed = 50f;
    const float HeaderHeight = 40f;
    const float SidePadding = 10f;
    const float ColumnGap = 10f;

    Func<IEnumerable<MenuOption>> SourceProvider { get; }
    Func<IEnumerable<MenuOption>> DestinationProvider { get; }
    Action<MenuOption, MenuOption> OnConfirm { get; }
    string Title { get; }

    int SelectedSourceIndex { get; set; } = 0;
    int SelectedDestinationIndex { get; set; } = 0;
    float SourceScrollOffset { get; set; } = 0f;
    float DestinationScrollOffset { get; set; } = 0f;
    float ViewportHeight { get; set; } = 300f;

    SelectionSide FocusedSide { get; set; } = SelectionSide.Source;

    enum SelectionSide {
        Source,
        Destination
    }

    internal DualSelectionMenuScreen(
        string title,
        Func<IEnumerable<MenuOption>> sourceProvider,
        Func<IEnumerable<MenuOption>> destinationProvider,
        Action<MenuOption, MenuOption> onConfirm) {
        this.Title = title;
        this.SourceProvider = sourceProvider;
        this.DestinationProvider = destinationProvider;
        this.OnConfirm = onConfirm;
    }

    internal override void Draw() {
        List<MenuOption> sourceOptions = [.. this.SourceProvider()];
        List<MenuOption> destinationOptions = [.. this.DestinationProvider()];

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float menuWidth = screenWidth * 0.7f;
        float menuHeight = screenHeight * 0.7f;
        float menuX = (screenWidth - menuWidth) * 0.5f;
        float menuY = (screenHeight - menuHeight) * 0.5f;

        this.ViewportHeight = menuHeight - HeaderHeight - 80f;

        Helper.DrawBox(new Vector2(menuX + (menuWidth * 0.5f), menuY + (menuHeight * 0.5f)), new Size { Width = menuWidth, Height = menuHeight }, new Color(0.2f, 0.2f, 0.2f, 0.9f));

        this.DrawHeader(menuX, menuY, menuWidth);

        float columnWidth = (menuWidth - (SidePadding * 2) - ColumnGap) * 0.5f;
        float contentY = menuY + HeaderHeight + 10f;

        float sourceX = menuX + SidePadding;
        float destX = sourceX + columnWidth + ColumnGap;

        int selectedSourceIndex = this.SelectedSourceIndex;
        float sourceScrollOffset = this.SourceScrollOffset;
        int selectedDestinationIndex = this.SelectedDestinationIndex;
        float destinationScrollOffset = this.DestinationScrollOffset;

        this.DrawColumn(sourceOptions, sourceX, contentY, columnWidth, "Source", ref selectedSourceIndex, ref sourceScrollOffset, SelectionSide.Source);
        this.DrawColumn(destinationOptions, destX, contentY, columnWidth, "Destination", ref selectedDestinationIndex, ref destinationScrollOffset, SelectionSide.Destination);

        this.SelectedSourceIndex = selectedSourceIndex;
        this.SourceScrollOffset = sourceScrollOffset;
        this.SelectedDestinationIndex = selectedDestinationIndex;
        this.DestinationScrollOffset = destinationScrollOffset;

        this.DrawFooter(menuX, menuY + menuHeight - 30f, menuWidth, sourceOptions, destinationOptions);
    }

    void DrawHeader(float x, float y, float width) {
        Helper.DrawBox(new Vector2(x + (width * 0.5f), y + (HeaderHeight * 0.5f)), new Size { Width = width, Height = HeaderHeight }, new Color(0.1f, 0.1f, 0.1f, 0.95f));

        GUIStyle titleStyle = new(UnityGUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        UnityGUI.Label(new Rect(x + 5, y + 5, width - 35, HeaderHeight - 10), this.Title, titleStyle);

        if (UnityGUI.Button(new Rect(x + width - 30, y + 5, 25, HeaderHeight - 10), "◀", UnityGUI.skin.button)) {
            this.Back();
        }
    }

    void DrawColumn(List<MenuOption> options, float x, float y, float width, string columnTitle, ref int selectedIndex, ref float scrollOffset, SelectionSide side) {
        GUIStyle columnHeaderStyle = new(UnityGUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        UnityGUI.Label(new Rect(x, y - 25, width, 20), columnTitle, columnHeaderStyle);

        float totalContentHeight = options.Count * (ButtonHeight + ButtonSpacing);
        float maxScroll = Mathf.Max(0, totalContentHeight - this.ViewportHeight);

        UnityGUILayout.BeginArea(new Rect(x, y, width, this.ViewportHeight));

        int visibleStart = Mathf.FloorToInt(scrollOffset / (ButtonHeight + ButtonSpacing));
        int visibleEnd = Mathf.CeilToInt((scrollOffset + this.ViewportHeight) / (ButtonHeight + ButtonSpacing));
        visibleEnd = Mathf.Min(visibleEnd, options.Count);

        for (int i = 0; i < options.Count; i++) {
            if (i < visibleStart || i >= visibleEnd) {
                continue;
            }

            float currentY = (i * (ButtonHeight + ButtonSpacing)) - scrollOffset;
            bool isSelected = i == selectedIndex;
            bool isFocused = this.FocusedSide == side;
            Color buttonColor = isSelected && isFocused ? new Color(0.4f, 0.4f, 0.8f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);

            Helper.DrawBox(new Vector2(width * 0.5f, currentY + (ButtonHeight * 0.5f)), new Size { Width = width - 10, Height = ButtonHeight }, buttonColor);

            GUIStyle buttonStyle = new(UnityGUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            Rect buttonRect = new(5, currentY, width - 10, ButtonHeight);

            if (UnityGUI.Button(buttonRect, options[i].Label, buttonStyle)) {
                selectedIndex = i;
                this.FocusedSide = side;
            }

            if (isSelected && isFocused) {
                Helper.DrawOutlineBox(new Vector2(width * 0.5f, currentY + (ButtonHeight * 0.5f)), new Size { Width = width - 10, Height = ButtonHeight }, 2f, Color.yellow);
            }
        }

        if (Input.mouseScrollDelta.y != 0) {
            scrollOffset = Mathf.Clamp(scrollOffset - (Input.mouseScrollDelta.y * ScrollSpeed), 0, maxScroll);
        }

        UnityGUILayout.EndArea();
    }

    void DrawFooter(float x, float y, float width, List<MenuOption> sourceOptions, List<MenuOption> destinationOptions) {
        Helper.DrawBox(new Vector2(x + (width * 0.5f), y + 15f), new Size { Width = width, Height = 30f }, new Color(0.15f, 0.15f, 0.15f, 0.95f));

        float buttonWidth = 100f;
        float buttonHeight = 25f;
        float spacing = 10f;
        float centerX = x + (width * 0.5f);

        bool hasValidSource = this.SelectedSourceIndex >= 0 && this.SelectedSourceIndex < sourceOptions.Count;
        bool hasValidDestination = this.SelectedDestinationIndex >= 0 && this.SelectedDestinationIndex < destinationOptions.Count;

        UnityGUI.enabled = hasValidSource && hasValidDestination;
        if (UnityGUI.Button(new Rect(centerX - buttonWidth - (spacing * 0.5f), y + 2.5f, buttonWidth, buttonHeight), "Confirm", UnityGUI.skin.button)) {
            if (hasValidSource && hasValidDestination) {
                this.OnConfirm(sourceOptions[this.SelectedSourceIndex], destinationOptions[this.SelectedDestinationIndex]);
            }
        }

        UnityGUI.enabled = true;
    }

    internal override void OnEnter() {
        this.SelectedSourceIndex = 0;
        this.SelectedDestinationIndex = 0;
        this.SourceScrollOffset = 0f;
        this.DestinationScrollOffset = 0f;
        this.FocusedSide = SelectionSide.Source;
    }
}

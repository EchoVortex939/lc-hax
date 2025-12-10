using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGUI = UnityEngine.GUI;
using UnityGUILayout = UnityEngine.GUILayout;

namespace Hax.GUI;

sealed class SingleSelectionMenuScreen : MenuScreen {
    const float ButtonHeight = 30f;
    const float ButtonSpacing = 5f;
    const float ScrollSpeed = 50f;
    const float HeaderHeight = 40f;
    const float SidePadding = 10f;

    Func<IEnumerable<MenuOption>> OptionsProvider { get; }
    Action<MenuOption> OnSelect { get; }
    string Title { get; }

    int SelectedIndex { get; set; } = 0;
    float ScrollOffset { get; set; } = 0f;
    float ViewportHeight { get; set; } = 300f;

    internal SingleSelectionMenuScreen(string title, Func<IEnumerable<MenuOption>> optionsProvider, Action<MenuOption> onSelect) {
        this.Title = title;
        this.OptionsProvider = optionsProvider;
        this.OnSelect = onSelect;
    }

    internal override void Draw() {
        List<MenuOption> options = [.. this.OptionsProvider()];

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float menuWidth = screenWidth * 0.4f;
        float menuHeight = screenHeight * 0.6f;
        float menuX = (screenWidth - menuWidth) * 0.5f;
        float menuY = (screenHeight - menuHeight) * 0.5f;

        this.ViewportHeight = menuHeight - HeaderHeight - 20f;

        Helper.DrawBox(new Vector2(menuX, menuY), new Size { Width = menuWidth, Height = menuHeight }, new Color(0.2f, 0.2f, 0.2f, 0.9f));

        this.DrawHeader(menuX, menuY, menuWidth);

        float contentX = menuX + SidePadding;
        float contentY = menuY + HeaderHeight + 10f;
        float contentWidth = menuWidth - (SidePadding * 2);

        this.DrawScrollContent(options, contentX, contentY, contentWidth);
    }

    void DrawHeader(float x, float y, float width) {
        Helper.DrawBox(new Vector2(x, y), new Size { Width = width, Height = HeaderHeight }, new Color(0.1f, 0.1f, 0.1f, 0.95f));

        GUIStyle titleStyle = new(UnityGUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        UnityGUI.Label(new Rect(x + 5, y + 5, width - 35, HeaderHeight - 10), this.Title, titleStyle);

        if (UnityGUI.Button(new Rect(x + width - 30, y + 5, 25, HeaderHeight - 10), "◀", UnityGUI.skin.button)) {
            this.Back();
        }
    }

    void DrawScrollContent(List<MenuOption> options, float x, float y, float width) {
        float totalContentHeight = options.Count * (ButtonHeight + ButtonSpacing);
        float maxScroll = Mathf.Max(0, totalContentHeight - this.ViewportHeight);

        if (Input.mouseScrollDelta.y != 0) {
            this.ScrollOffset = Mathf.Clamp(this.ScrollOffset - (Input.mouseScrollDelta.y * ScrollSpeed), 0, maxScroll);
        }

        UnityGUILayout.BeginArea(new Rect(x, y, width, this.ViewportHeight));

        int visibleStart = Mathf.FloorToInt(this.ScrollOffset / (ButtonHeight + ButtonSpacing));
        int visibleEnd = Mathf.CeilToInt((this.ScrollOffset + this.ViewportHeight) / (ButtonHeight + ButtonSpacing));
        visibleEnd = Mathf.Min(visibleEnd, options.Count);
        _ = -this.ScrollOffset;

        for (int i = 0; i < options.Count; i++) {
            float currentY = y + (i * (ButtonHeight + ButtonSpacing)) - this.ScrollOffset;

            if (i < visibleStart || i >= visibleEnd) {
                continue;
            }

            bool isSelected = i == this.SelectedIndex;
            Color buttonColor = isSelected ? new Color(0.4f, 0.4f, 0.8f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);

            Helper.DrawBox(new Vector2(x + (width * 0.5f), currentY + (ButtonHeight * 0.5f)), new Size { Width = width - 10, Height = ButtonHeight }, buttonColor);

            GUIStyle buttonStyle = new(UnityGUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            Rect buttonRect = new(x + 5, currentY, width - 10, ButtonHeight);

            if (UnityGUI.Button(buttonRect, options[i].Label, buttonStyle)) {
                this.SelectedIndex = i;
                this.OnSelect(options[i]);
            }

            if (isSelected) {
                Helper.DrawOutlineBox(new Vector2(x + (width * 0.5f), currentY + (ButtonHeight * 0.5f)), new Size { Width = width - 10, Height = ButtonHeight }, 2f, Color.yellow);
            }
        }

        UnityGUILayout.EndArea();
    }

    internal override void OnEnter() {
        this.SelectedIndex = 0;
        this.ScrollOffset = 0f;
    }
}

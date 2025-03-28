using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EmCounter.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow() : base("Emote Counter Config###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 120);
        SizeCondition = ImGuiCond.Always;

        Configuration = Service.pluginConfig;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        /*if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }*/
    }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var showFlyText = Configuration.showFlyText;
        if (ImGui.Checkbox("Show flying text", ref showFlyText))
        {
            Configuration.showFlyText = showFlyText;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var showFlyTextNames = Configuration.showFlyTextNames;
        if (ImGui.Checkbox("Show name in flying text", ref showFlyTextNames))
        {
            Configuration.showFlyTextNames = showFlyTextNames;
            Configuration.Save();
        }

        var showSpanks = Configuration.showSpanks;
        if (ImGui.Checkbox("Show High Five", ref showSpanks))
        {
            Configuration.showSpanks = showSpanks;
            Configuration.Save();
        }
    }
}

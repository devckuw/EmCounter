using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace EmCounter.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private EmoteDataManager emoteDataManager;
    private Dictionary<ushort, string> emotesNames = new Dictionary<ushort, string>();

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, EmoteDataManager emoteDataManager)
        : base("Emote Counter##With a hidden ID")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(50, 50),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        this.emoteDataManager = emoteDataManager;
        foreach (var emote in Service.DataManager.GameData.GetExcelSheet<Emote>())
        {
            if (!emote.Name.IsEmpty)
            {
                emotesNames.Add((ushort)emote.RowId, emote.Name.ToString());
            }
        }
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (var tab = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, false))
        {
            if (tab.Success)
            {
                var emotes = emoteDataManager.GetEmotes();
                var names = emoteDataManager.GetNames();
                var counter = emoteDataManager.GetCounter();
                
                ImGuiTableFlags flag = ImGuiTableFlags.Hideable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp;
                ImGui.BeginTable("timelinetable", emotes.Count + 1, flag);

                ImGui.TableSetupColumn("Name");
                //Service.Log.Debug("avant emote use");
                foreach (var emote in emotes)
                {
                    if (emotesNames.ContainsKey(emote))
                        ImGui.TableSetupColumn($"{emotesNames[emote]}");
                    else
                        ImGui.TableSetupColumn($"{emote}?");
                }
                //Service.Log.Debug("après emote use");
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();

                foreach (var id in counter.Keys)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{names[id]}");
                    foreach (var emote in emotes)
                    {
                        ImGui.TableNextColumn();
                        if (counter[id].ContainsKey(emote))
                        {
                            ImGui.TextUnformatted($"{counter[id][emote]}");
                        }
                        else
                        {
                            ImGui.TextUnformatted("0");
                        }
                    }
                }
                ImGui.EndTable();
            }
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace EmCounter.Windows;

public class MainWindow : Window, IDisposable
{
    private EmoteDataManager emoteDataManager;
    private Dictionary<ushort, string> emotesNames = new Dictionary<ushort, string>();

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(EmoteDataManager emoteDataManager)
        : base("Emote Counter##With a hidden ID")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(50, 50),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

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

    public void DrawTabSorted()
    {
        using (var tab2 = ImRaii.Child("SomeChildWithAScrollbarButSorted", Vector2.Zero, false))
        {
            if (tab2.Success)
            {
                var emotes = emoteDataManager.GetEmotes();
                var names = emoteDataManager.GetNames();
                var counter = emoteDataManager.GetCounter();

                ImGuiTableFlags flag = ImGuiTableFlags.Hideable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable | 
                    ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Sortable | ImGuiTableFlags.BordersInnerH;
                ImGui.BeginTable("tabsorted", emotes.Count + 1, flag);

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
                int[] total = new int[emotes.Count];
                Array.Clear(total, 0, total.Length);

                ImGuiTableSortSpecsPtr sortInfo = ImGui.TableGetSortSpecs();
                if (sortInfo.SpecsCount != 0)
                {
                    Service.Log.Debug($"{sortInfo.Specs.ColumnIndex} / {sortInfo.Specs.SortDirection} / {sortInfo.SpecsCount} / {sortInfo.SpecsDirty}");
                }

                IOrderedEnumerable<KeyValuePair<ulong, Dictionary<ushort, int>>> sorted;

                if (sortInfo.Specs.ColumnIndex == 0)
                {
                    if(sortInfo.Specs.SortDirection == ImGuiSortDirection.Descending)
                        sorted = counter.OrderByDescending(x => names[x.Key]);
                    else
                        sorted = counter.OrderBy(x => names[x.Key]);
                }
                else
                {
                    if (sortInfo.Specs.SortDirection == ImGuiSortDirection.Ascending)
                        sorted = counter.OrderBy(x => counter[x.Key].ContainsKey(emotes[sortInfo.Specs.ColumnIndex - 1]) ? counter[x.Key][emotes[sortInfo.Specs.ColumnIndex - 1]] : 0);
                    else
                        sorted = counter.OrderByDescending(x => counter[x.Key].ContainsKey(emotes[sortInfo.Specs.ColumnIndex - 1]) ? counter[x.Key][emotes[sortInfo.Specs.ColumnIndex - 1]] : 0);
                }

                foreach (var id in sorted)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{names[id.Key]}");
                    int i = 0;
                    foreach (var emote in emotes)
                    {
                        ImGui.TableNextColumn();
                        if (counter[id.Key].ContainsKey(emote))
                        {
                            ImGui.TextUnformatted($"{counter[id.Key][emote]}");
                            //Service.Log.Debug($"{total[i]} {counter[id][emote]}");
                            total[i] += counter[id.Key][emote];
                            //Service.Log.Debug($"{total[i]}");

                        }
                        else
                        {
                            ImGui.TextUnformatted("0");
                        }
                        i += 1;
                    }
                }
                ImGui.TableNextRow();
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted("Total");
                for (int i = 0; i < emotes.Count; i++)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{total[i]}");
                }

                ImGui.EndTable();
                foreach (var name in names)
                {
                    ImGui.TextUnformatted($"{name.Key} {name.Value}");
                }
            }
            ImGui.TextUnformatted("");
            ImGuiComponents.HelpMarker("Right click to select/deselect emotes.");

        }
    }

    public override void Draw()
    {
        DrawTabSorted();
    }
}

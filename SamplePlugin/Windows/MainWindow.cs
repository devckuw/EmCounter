using System;
using System.Collections.Generic;
using System.Globalization;
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
                
                bool isDataSafe = names.Count == counter.Count;
                Service.Log.Debug($"{isDataSafe}");
                ImGuiTableFlags flag = ImGuiTableFlags.Hideable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable |
                    ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Sortable | ImGuiTableFlags.BordersInnerH;
                if (ImGui.BeginTable("tabsorted3", emotes.Count + 1, ImGuiTableFlags.Hideable | ImGuiTableFlags.Sortable | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchSame))
                //if (ImGui.BeginTable("tabsorted2", emotes.Count + 1, flag))
                {
                    ImGui.TableSetupColumn("Name");
                    foreach (var emote in emotes)
                    {
                        if (emotesNames.ContainsKey(emote))
                            ImGui.TableSetupColumn($"{emotesNames[emote]}");
                        else
                            ImGui.TableSetupColumn($"{emote}?");
                    }

                    ImGui.TableHeadersRow();

                    int[] total = new int[emotes.Count];
                    Array.Clear(total, 0, total.Length);

                    ImGuiTableSortSpecsPtr sortInfo = ImGui.TableGetSortSpecs();
                    if (sortInfo.SpecsCount != 0)
                    {
                        Service.Log.Debug($"{sortInfo.Specs.ColumnIndex} / {sortInfo.Specs.SortDirection} / {sortInfo.SpecsCount} / {sortInfo.SpecsDirty}");
                    }

                    IOrderedEnumerable<KeyValuePair<ulong, Dictionary<ushort, int>>> sorted;
                    if (isDataSafe)
                    {
                        if (sortInfo.Specs.ColumnIndex == 0)
                        {
                            if (sortInfo.Specs.SortDirection == ImGuiSortDirection.Descending)
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
                    }
                    else
                    {
                        sorted = (IOrderedEnumerable<KeyValuePair<ulong, Dictionary<ushort, int>>>)counter;
                    }

                    foreach (var id in sorted)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if (names.ContainsKey(id.Key))
                            ImGui.TextUnformatted($"{names[id.Key]}");
                        else
                            ImGui.TextUnformatted($"???");
                        int i = 0;
                        foreach (var emote in emotes)
                        {
                            ImGui.TableNextColumn();
                            if (counter[id.Key].ContainsKey(emote))
                            {
                                ImGui.TextUnformatted($"{counter[id.Key][emote]}");
                                total[i] += counter[id.Key][emote];
                            }
                            else
                            {
                                ImGui.TextUnformatted("0");
                            }
                            i += 1;
                        }
                    }

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
                }
#if DEBUG
                foreach (var name in names)
                {
                    ImGui.TextUnformatted($"{name.Key} {name.Value}");
                }
#endif
                    
            }
            ImGui.TextUnformatted("");
            ImGuiComponents.HelpMarker("Right click to select/deselect emotes.");
        }
    }

    public void DrawTabSorted2()
    {
        var emotes = emoteDataManager.GetEmotes();
        var names = emoteDataManager.GetNames();
        var counter = emoteDataManager.GetCounter();

        bool isDataSafe = names.Count == counter.Count;
        Service.Log.Debug($"{isDataSafe}");
        ImGuiTableFlags flag = ImGuiTableFlags.Hideable | ImGuiTableFlags.Sortable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp  | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner;
        //if (ImGui.BeginTable("tabsorted3", emotes.Count + 1, ImGuiTableFlags.Hideable | ImGuiTableFlags.Sortable | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchSame))
        if (ImGui.BeginTable("tabsorted", emotes.Count + 1, flag))
        {
            ImGui.TableSetupColumn("Name");
            foreach (var emote in emotes)
            {
                if (emotesNames.ContainsKey(emote))
                    ImGui.TableSetupColumn($"{emotesNames[emote]}");
                else
                    ImGui.TableSetupColumn($"{emote}?");
            }

            ImGui.TableHeadersRow();

            int[] total = new int[emotes.Count];
            Array.Clear(total, 0, total.Length);

            IOrderedEnumerable<KeyValuePair<ulong, Dictionary<ushort, int>>> sorted;

            ImGuiTableSortSpecsPtr sortInfo = ImGui.TableGetSortSpecs();
            if (sortInfo.SpecsCount != 0)
            {
                Service.Log.Debug($"{sortInfo.Specs.ColumnIndex} / {sortInfo.Specs.SortDirection} / {sortInfo.SpecsCount} / {sortInfo.SpecsDirty}");
            }

            if (isDataSafe)
            {
                if (sortInfo.Specs.ColumnIndex == 0)
                {
                    if (sortInfo.Specs.SortDirection == ImGuiSortDirection.Descending)
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
            }
            else
            {
                sorted = (IOrderedEnumerable<KeyValuePair<ulong, Dictionary<ushort, int>>>)counter;
            }
            sorted = (IOrderedEnumerable<KeyValuePair<ulong, Dictionary<ushort, int>>>)counter;
            foreach (var id in sorted)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (names.ContainsKey(id.Key))
                    ImGui.TextUnformatted($"{names[id.Key]}");
                else
                    ImGui.TextUnformatted($"???");
                int i = 0;
                foreach (var emote in emotes)
                {
                    ImGui.TableNextColumn();
                    if (counter[id.Key].ContainsKey(emote))
                    {
                        ImGui.TextUnformatted($"{counter[id.Key][emote]}");
                        total[i] += counter[id.Key][emote];
                    }
                    else
                    {
                        ImGui.TextUnformatted("0");
                    }
                    i += 1;
                }
            }

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
        }
    }

    public override void Draw()
    {
        //DrawTabSorted();
    }
}

using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
//using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmCounter;

public class EmoteDataManager : IDisposable
{

    private string ownerName = string.Empty;
    private ulong ownerCID;

    private int counterChanged = 0;
    private Dictionary<ulong, Dictionary<ushort, int>> counter;
    private Dictionary<ulong, string> names;
    private List<ushort> emotes;
    private Dictionary<ushort, string> emotesNames = new Dictionary<ushort, string>();

    private RewardFlyTextPat rewardPat = new RewardFlyTextPat();
    private List<ushort> allowedEmoteReward = new List<ushort> { 105, 112, 113 , 146, 147 , 213};

    //public DataFrame data;

    public EmoteDataManager()
    {
        Service.Log.Debug("create EmoteDataManager");
        
        UpdateOwner();

        foreach (var emote in Service.DataManager.GameData.GetExcelSheet<Emote>())
        {
            if (!emote.Name.IsEmpty)
            {
                emotesNames.Add((ushort)emote.RowId, emote.Name.ToString());
            }
        }
    }

    public void Dispose()
    {
        Save();
    }

    public void OnLogin()
    {
        Service.Log.Debug("on login");
        UpdateOwner();
    }

    public void OnLogout(int type, int code)
    {
        Service.Log.Debug("on logout");
        Save();
        ownerName = string.Empty;
        ownerCID = 0;
    }

    private unsafe ulong GetContentId(IPlayerCharacter player)
    {
        var chara = (Character*)player.Address;
        return chara == null ? 0 : chara->ContentId;
    }

    public void OnEmote(IPlayerCharacter instigator, ushort emoteId)
    {
        if (ownerCID == 0)
            UpdateOwner();

        if (!emotesNames.ContainsKey(emoteId))
            return;

        if (!emotes.Contains(emoteId))
            emotes.Add(emoteId);

        ulong contentId = GetContentId(instigator);
        if (contentId == 0)
        {
#if DEBUG
            Service.Log.Debug("contentid => 0");
#endif
            return;
        }
#if DEBUG
        Service.Log.Debug($"on emote => add count {contentId:X}/{contentId} {emoteId}  {instigator.Name} {instigator.HomeWorld.Value.Name}");
#endif
        counterChanged++;
        names[contentId] = instigator.Name.ToString();
        if (!counter.ContainsKey(contentId))
            counter[contentId] = new Dictionary<ushort, int>();
        if (!counter[contentId].ContainsKey(emoteId))
        {
            counter[contentId][emoteId] = 1;
        }
        else
        {
            counter[contentId][emoteId]++;
        }
        
        if (counterChanged > 50)
        {
            counterChanged = 0;
            Save();
        }

        switch (emoteId)
        {
            case 213: // High Five
                if (Service.pluginConfig.showSpanks)
                    rewardPat.OnPat(instigator, "Spank", (uint)counter[contentId][emoteId]);
                break;
            case 46: // Blow Kiss
                rewardPat.OnPat(instigator, "Dote", (uint)counter[contentId][emoteId]);
                break;
            case 105: // Pet
            case 112: // Hug
            case 113: // Embrace
            case 146: // Dote
            case 147: // Dote
                rewardPat.OnPat(instigator, emotesNames[emoteId], (uint)counter[contentId][emoteId]);
                break;
            default:
                break;
        }

        /*if (allowedEmoteReward.Contains(emoteId))
        {
            switch (emoteId)
            {
                case 213: // High Five
                    if (Service.pluginConfig.showSpanks)
                        rewardPat.OnPat(instigator, "Spank", (uint)counter[contentId][emoteId]);
                    break;
                case 46: // Blow Kiss
                    rewardPat.OnPat(instigator, "Dote", (uint)counter[contentId][emoteId]);
                    break;
                case 105: // Pet
                case 112: // Hug
                case 113: // Embrace
                case 146: // Dote
                case 147: // Dote
                    rewardPat.OnPat(instigator, emotesNames[emoteId], (uint)counter[contentId][emoteId]);
                    break;
                default:
                    break;
            }
        }*/
    }

    public void OnTerritoryChanged(ushort areaId)
    {
#if DEBUG
        Service.Log.Debug("on TerritoryChanged");
#endif
        if (counterChanged > 0)
        {
            counterChanged = 0;
            Save();
        }
    }

    private void UpdateOwner()
    {
        Service.Log.Debug("update char");
        if (Service.ClientState == null || Service.ClientState.LocalContentId == 0)
        {
            return;
        }

        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null || localPlayer.Name == null)
        {
            return;
        }

        var newCID = Service.ClientState.LocalContentId;
        var newName = localPlayer.Name.TextValue;

        if (newCID != ownerCID || newName != ownerName)
        {
            ownerName = newName;
            ownerCID = newCID;

            //OnOwnerChanged();
            Load();
        }
    }

    public void Save()
    {
        Service.Log.Debug("save");
        //p.Configuration.dataCount.Add(ownerCID, counter);
        Service.pluginConfig.dataCount[ownerCID] =  counter;
        Service.pluginConfig.dataNames = names;
        Service.pluginConfig.Save();
    }

    public void Load()
    {
        Service.Log.Debug("load");
        //counter = p.Configuration.dataCount[ownerCID];
        if (Service.pluginConfig.dataCount.ContainsKey(ownerCID))
            counter = Service.pluginConfig.dataCount[ownerCID];
        else
            counter = new Dictionary<ulong, Dictionary<ushort, int>>();
        names = Service.pluginConfig.dataNames;
        if (names == null)
            names = new Dictionary<ulong, string>();

        emotes = new List<ushort>();
        foreach (var id in counter.Keys)
        {
            foreach (var em in counter[id])
            {
                emotes.Add(em.Key);
            }
        }
        emotes = emotes.Distinct().ToList();
        Service.Log.Debug($"{counter.Count} {names.Count} {emotes.Count}");

    }

    public Dictionary<ulong, Dictionary<ushort, int>> GetCounter()
    {
        Service.Log.Debug($"{counter.Count} {names.Count} {emotes.Count}");
        return counter;
    }

    public Dictionary<ulong, string> GetNames()
    { 
        return names; 
    }

    public List<ushort> GetEmotes()
    { 
        return emotes; 
    }
}

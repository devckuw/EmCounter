using Dalamud.Game.ClientState.Objects.SubKinds;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmCounter;

public class EmoteDataManager : IDisposable
{

    private string ownerName = string.Empty;
    private ulong ownerCID;
    private Plugin p;

    private int counterChanged = 0;
    private Dictionary<ulong, Dictionary<ushort, int>> counter;
    private Dictionary<ulong, string> names;
    private List<ushort> emotes;
    private Dictionary<ushort, string> emotesNames = new Dictionary<ushort, string>();

    public EmoteDataManager(Plugin p)
    {
        Service.Log.Debug("create EmoteDataManager");
        this.p = p;
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

    public void OnEmote(IPlayerCharacter instigator, ushort emoteId)
    {
        if (ownerCID == 0)
            UpdateOwner();

        if (!emotesNames.ContainsKey(emoteId))
            return;

        if (!emotes.Contains(emoteId))
            emotes.Add(emoteId);

        Service.Log.Debug($"on emote => add count {instigator.EntityId:X} {emoteId}  {instigator.Name} {instigator.HomeWorld.Value.Name}");
        counterChanged++;
        names[instigator.EntityId] = instigator.Name.ToString();
        if (!counter.ContainsKey(instigator.EntityId))
            counter[instigator.EntityId] = new Dictionary<ushort, int>();
        if (!counter[instigator.EntityId].ContainsKey(emoteId))
        {
            counter[instigator.EntityId][emoteId] = 1;
        }
        else
        {
            counter[instigator.EntityId][emoteId]++;
        }
        
        if (counterChanged > 50)
        {
            counterChanged = 0;
            Save();
        }
    }

    public void OnTerritoryChanged(ushort areaId)
    {
        Service.Log.Debug("on TerritoryChanged");
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
        p.Configuration.dataCount[ownerCID] =  counter;
        p.Configuration.dataNames = names;
        p.Configuration.Save();
    }

    public void Load()
    {
        Service.Log.Debug("load");
        //counter = p.Configuration.dataCount[ownerCID];
        if (p.Configuration.dataCount.ContainsKey(ownerCID))
            counter = p.Configuration.dataCount[ownerCID];
        else
            counter = new Dictionary<ulong, Dictionary<ushort, int>>();
        names = p.Configuration.dataNames;
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
    }

    public Dictionary<ulong, Dictionary<ushort, int>> GetCounter()
    {
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

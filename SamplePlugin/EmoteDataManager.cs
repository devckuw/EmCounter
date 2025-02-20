using Dalamud.Game.ClientState.Objects.SubKinds;
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

    public EmoteDataManager()
    {
        Service.Log.Debug("load plugin => load char");
        //UpdateOwner();
    }

    public void Dispose()
    {
    }

    public void OnLogin()
    {
        Service.Log.Debug("on login => load char");
        //UpdateOwner();
    }

    public void OnLogout(int type, int code)
    {
        Service.Log.Debug("on logout => save char");
        //ownerName = string.Empty;
        //ownerCID = 0;

        //OnOwnerChanged();
    }

    public void OnEmote(IPlayerCharacter instigator, ushort emoteId)
    {
        Service.Log.Debug("on emote => add count");
        /*UpdateOwner();
        var needsSave = false;

        foreach (var counter in Service.emoteCounters)
        {
            var hasChanges = counter.OnEmote(instigator, emoteId);
            if (!hasChanges)
            {
                continue;
            }

            needsSave = true;
        }

        if (needsSave)
        {
            SaveOwnerDB();
        }*/
    }

    public void OnTerritoryChanged(ushort areaId)
    {
        Service.Log.Debug("on TerritoryChanged => save char");
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

            OnOwnerChanged();
        }
    }

    public void OnOwnerChanged()
    {
        //SaveOwnerDB();

        //LoadOrCreateOwnerDB();
        //CopyDBValuesToCounters();
    }

}

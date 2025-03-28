using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Hooking;

namespace EmCounter
{
    public class EmoteReaderHooks : IDisposable
    {
        public Action<IPlayerCharacter, ushort>? OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate>? hookEmote;

        public bool IsValid = false;

        public EmoteReaderHooks()
        {
            try
            {
                hookEmote = Service.InteropSigScanner.HookFromSignature<OnEmoteFuncDelegate>("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 4C 89 74 24", OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Service.Log.Error(ex, "failed to hook emotes!");
            }
        }

        public void Dispose()
        {
            hookEmote?.Dispose();
            IsValid = false;
        }

        private unsafe ulong GetContentId(IPlayerCharacter player)
        {
            var chara = (Character*)player.Address;
            return chara == null ? 0 : chara->ContentId;
        }

        void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2)
        {
            // unk - some field of event framework singleton? doesn't matter here anyway
#if DEBUG
            Service.Log.Info($"Emote >> unk:{unk:X}, instigatorAddr:{instigatorAddr:X}, emoteId:{emoteId}, targetId:{targetId:X}, unk2:{unk2:X}");
#endif

            if (Service.ClientState.LocalPlayer != null)
            {
                if (targetId == Service.ClientState.LocalPlayer.GameObjectId)
                {
                    var instigatorOb = Service.ObjectTable.FirstOrDefault(x => (ulong)x.Address == instigatorAddr) as IPlayerCharacter;
                    if (instigatorOb != null)
                    {
                        bool canCount = (instigatorOb.EntityId != targetId);

                        if (canCount)
                        {
#if DEBUG
                            Service.Log.Info($"on me {GetContentId(instigatorOb):X} {instigatorOb.Name} {instigatorOb.HomeWorld.Value.Name}");
#endif
                            OnEmote?.Invoke(instigatorOb, emoteId);
                        }
#if DEBUG
                        else
                        {
                            Service.Log.Info($"by me {GetContentId(instigatorOb):X} {instigatorOb.Name} {instigatorOb.HomeWorld.Value.Name}");
                        }
#endif
                    }
                }
            }

            hookEmote?.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }
    }
}

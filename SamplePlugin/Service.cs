using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Plugin;

namespace EmCounter;

public class Service
{
    public static Plugin plugin = null!;
    public static IDalamudPluginInterface pluginInterface = null!;
    public static Configuration pluginConfig = null!;

    //public static SplashScreenUI splashScreen = null!;

    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider InteropSigScanner { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] public static IFlyTextGui flyTextGui { get; private set; } = null!;
}

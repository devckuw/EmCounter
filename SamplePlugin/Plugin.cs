using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EmCounter.Windows;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;

namespace EmCounter;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    private const string CommandName = "/emotecounter";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Emote Counter");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    private EmoteReaderHooks emoteReader;
    private EmoteDataManager emoteDataManager;

    public Plugin()
    {

        PluginInterface.Create<Service>();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        emoteReader = new EmoteReaderHooks(this);
        emoteDataManager = new EmoteDataManager(this);

        emoteReader.OnEmote += emoteDataManager.OnEmote;
        Service.ClientState.Login += emoteDataManager.OnLogin;
        Service.ClientState.Logout += emoteDataManager.OnLogout;
        Service.ClientState.TerritoryChanged += emoteDataManager.OnTerritoryChanged;

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, emoteDataManager);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Service.Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        emoteReader.OnEmote -= emoteReader.OnEmote;
        Service.ClientState.Login -= emoteDataManager.OnLogin;
        Service.ClientState.Logout -= emoteDataManager.OnLogout;
        Service.ClientState.TerritoryChanged -= emoteDataManager.OnTerritoryChanged;
        emoteDataManager.Dispose();
        emoteReader.Dispose();

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        Service.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}

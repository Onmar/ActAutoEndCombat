using ActAutoEndCombat.Managers;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace ActAutoEndCombat;

internal class Services
{
    [PluginService] internal static DalamudPluginInterface DalamudPluginInterface { get; private set; }
    [PluginService] internal static CommandManager CommandManager { get; private set; }
    [PluginService] internal static ChatGui ChatGui { get; private set; }
    [PluginService] internal static ClientState ClientState { get; private set; }
    [PluginService] internal static Condition Condition { get; private set; }
    [PluginService] internal static DataManager DataManager { get; private set; }

    internal static Ui Ui { get; set; }
    internal static ActAutoEndManager ActAutoEndManager { get; set; }
    internal static SlashCommandManager SlashCommandManager { get; set; }
    internal static Config Config { get; set; }
}
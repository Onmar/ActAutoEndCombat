using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.IoC;

using ActAutoEndCombat.Managers;

namespace ActAutoEndCombat;

public class ActAutoEndCombat : IDalamudPlugin
{
    public string Name => "ACT Auto End Combat";

    public ActAutoEndCombat([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();

        // Config Initialization
        Services.Config = Services.DalamudPluginInterface.GetPluginConfig() as Config ?? new Config();
        Services.Config.Initialize(pluginInterface);

        // Internal Services
        Services.Ui = new Ui(this);
        Services.ActAutoEndManager = new ActAutoEndManager();
        Services.SlashCommandManager = new SlashCommandManager();
    }

    public void Dispose()
    {
        // Internal Services
        Services.SlashCommandManager?.Dispose();
        Services.ActAutoEndManager?.Dispose();
        Services.Ui?.Dispose();
    }
    
}
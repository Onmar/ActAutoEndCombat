using System;

using ActAutoEndCombat.Windows;

namespace ActAutoEndCombat;

public class Ui : IDisposable
{
    public ConfigurationWindow ConfigWindow { get; private set; }

    public Ui(ActAutoEndCombat plugin)
    {
        ConfigWindow = new ConfigurationWindow(plugin);

        Services.DalamudPluginInterface.UiBuilder.Draw += Draw;
        Services.DalamudPluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
    }

    public void Dispose()
    {
        Services.DalamudPluginInterface.UiBuilder.Draw -= Draw;
        Services.DalamudPluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
    }

    private void Draw()
    {
        ConfigWindow.Draw();
    }

    private void OpenConfigUi()
    {
        ConfigWindow.Visible = true;
    }
}
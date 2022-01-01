using System;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;

namespace ActAutoEndCombat.Managers;

public class SlashCommandManager : IDisposable
{
    private const ushort ColorRed = 534;
    private const ushort ColorGreen = 045;

    private const string MainCommand = "/actautoend";
    private const string ShorthandCommand = "/aae";

    private static readonly SeString ActiveMessage = new SeStringBuilder()
        .Append("ACT Auto End is ")
        .AddUiForeground(ColorGreen).Append("active").AddUiForegroundOff()
        .Append(" in your current zone.").Build();

    private static readonly SeString InactiveMessage = new SeStringBuilder()
        .Append("ACT Auto End is ")
        .AddUiForeground(ColorRed).Append("inactive").AddUiForegroundOff()
        .Append(" in your current zone.").Build();

    public SlashCommandManager()
    {
        RegisterCommand();
        Services.DalamudPluginInterface.LanguageChanged += ReregisterCommand;
    }

    public void Dispose()
    {
        UnregisterCommand();
    }

    private void RegisterCommand()
    {
        Services.CommandManager.AddHandler(MainCommand, new CommandInfo(SlashCommandCallback)
        {
            HelpMessage = "Main command for ACT Auto End Combat"
        });
        Services.CommandManager.AddHandler(ShorthandCommand, new CommandInfo(SlashCommandCallback)
        {
            HelpMessage = "Shorthand command for the plugin",
        });
    }

    private void UnregisterCommand()
    {
        Services.CommandManager.RemoveHandler(MainCommand);
        Services.CommandManager.RemoveHandler(ShorthandCommand);
    }

    private void ReregisterCommand(string langCode)
    {
        UnregisterCommand();
        RegisterCommand();
    }

    private void SlashCommandCallback(string _, string args)
    {
        var splitArgs = args.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var command = splitArgs.Length > 0 ? splitArgs[0] : null;
        switch (command)
        {
            case null:
                Services.Ui.ConfigWindow.Toggle();
                break;

            case "h":
            case "help":
                Services.ChatGui.Print("Valid Subcommands: check, activate, deactivate");
                break;

            case "check":
                Services.ChatGui.Print(Services.ActAutoEndManager.Active ? ActiveMessage : InactiveMessage);
                break;

            case "activate":
                Services.ActAutoEndManager.Activate();
                Services.ChatGui.Print(
                    "ACT Auto End is now active. This will revert to you automatic settings once you switch zone.");
                break;

            case "deactivate":
                Services.ActAutoEndManager.Deactivate();
                Services.ChatGui.Print(
                    "ACT Auto End is now inactive. This will revert to you automatic settings once you switch zone.");
                break;

            default:
                Services.ChatGui.PrintError($"Unknown command: {command}, type \"/aae help\" for a list of valid commands.");
                break;
        }
    }
}
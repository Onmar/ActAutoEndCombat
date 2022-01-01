using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

using Lumina.Excel.GeneratedSheets;

using Dalamud.Logging;

using ImGuiNET;

using ActAutoEndCombat.Windows.Components;

namespace ActAutoEndCombat.Windows;

public class ConfigurationWindow : Window<ActAutoEndCombat>
{

    private Config _config;

    // UI Components
    private DutyTypeTable _dutyTypeTable;
    private PatternTable _includePatternTable;
    private PatternTable _excludePatternTable;

    private readonly IReadOnlyList<TerritoryType> _territories;

    // Variables for Zone Debug
    private string _specificZoneDebugInfo = "";

    public ConfigurationWindow(ActAutoEndCombat plugin) : base(plugin)
    {
        _territories = Services.DataManager.GetExcelSheet<TerritoryType>()!.ToList();
    }

    protected override void OnOpen()
    {
        // Copy current Config
        _config = new Config(Services.Config);

        _dutyTypeTable = new DutyTypeTable(_config);

        _includePatternTable = new PatternTable("Include", _config.IncludePatterns, _territories.Select(Utils.GetDutyNameForTerritory).ToList());
        _excludePatternTable = new PatternTable("Exclude", _config.ExcludePatterns, GetIncludedTerritoryNames());

        // Update Exclude Values when Include Patterns change
        _dutyTypeTable!.TypesChanged += UpdateExcludePatternTableValues;
        _includePatternTable!.PatternsChanged += UpdateExcludePatternTableValues;
    }

    private void UpdateExcludePatternTableValues(object sender, EventArgs eventArgs)
    {
        _excludePatternTable.Values = GetIncludedTerritoryNames();
    }

    private List<string> GetIncludedTerritoryNames()
    {
        if (_config.IncludedDutyTypes == Config.DutyType.None && _config.IncludePatterns.Count == 0)
            return _territories.Select(Utils.GetDutyNameForTerritory).ToList();

        return _territories
            .Where(territory => _config.IncludedDutyTypes.HasFlag(Utils.GetDutyTypeForTerritory(territory)) || 
                                _config.IncludePatterns.Any(pattern => Regex.IsMatch(Utils.GetDutyNameForTerritory(territory), Utils.GlobToRegex(pattern))))
            .Select(Utils.GetDutyNameForTerritory)
            .ToList();
    }

    protected override void OnClose()
    {
        _dutyTypeTable = null;
        _includePatternTable = null;
        _excludePatternTable = null;
    }

    protected override void DrawUi()
    {
        ImGui.SetNextWindowSize(new Vector2(500, 100), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(500, 100), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin($"{Plugin.Name} Configuration", ref _WindowVisible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            // Delay
            ImGui.Text("Delay end of combat");
            ImGui.SameLine();
            HelpMarker.DrawComponent("How many seconds to wait after you exit combat before sending the end command to ACT.\n" + 
                                     "This is different from the ACT setting since this goes from the time you actually leave combat, rather than the last combat action. " + 
                                     "In fights with a cutscene (e.g. E1, E8S) or transition (e.g. E4S) you don't leave combat during the cutscene/transition, but no combat actions are taken during that time. " + 
                                     "This is what causes ACT to reset at such occasions.\n" + 
                                     "This delay is mostly for World or Dungeon type content where you frequently enter and leave combat.");
            if (ImGui.InputDouble("[seconds]", ref _config.Delay, 1.0, 1.0, "%.1f"))
            {
                if (_config.Delay < 0.0) _config.Delay = 0.0;
            }
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("Zone Filters");
            ImGui.SameLine();
            HelpMarker.DrawComponent("Here you can specify which zones you would like the plugin to be active in. " + 
                                     "If you have no filters set it will be active in all zones. " +
                                     "Otherwise it will be active in any zone which matches at least one include filter (if any exist), but no exclude filters. " + 
                                     "Filter types are additive, so you can use both a type and name filter at the same time.");
            
            // Include Filters
            if (ImGui.TreeNode("Included Zones"))
            {
                // Include Duty Types
                if (ImGui.TreeNode("By Type"))
                {
                    _dutyTypeTable?.DrawComponent();
                    ImGui.TreePop();
                }
                ImGui.Spacing();
                // Include Name Patterns
                var includeByNameOpen = ImGui.TreeNode("By Name");
                ImGui.SameLine();
                DrawPatternHelpMarker();
                if (includeByNameOpen)
                {
                    _includePatternTable?.DrawComponent();
                    ImGui.TreePop();   
                }
                ImGui.TreePop();
            }
            ImGui.Spacing();

            // Exclude Filters
            if (ImGui.TreeNode("Excluded Zones"))
            {
                // Exclude Name Patterns
                var excludeByNameOpen = ImGui.TreeNode("By Name"); 
                ImGui.SameLine();
                DrawPatternHelpMarker();
                if (excludeByNameOpen)
                {
                    _excludePatternTable?.DrawComponent();
                    ImGui.TreePop();   
                }
                ImGui.TreePop();
            }
            ImGui.Separator();
            ImGui.Spacing();

            // Save & Close Buttons
            if (ImGui.Button("Save")) SaveConfig();
            ImGui.SameLine();
            if (ImGui.Button("Close")) Visible = false;
            ImGui.SameLine();
            if (ImGui.Button("Save and Close")) { SaveConfig(); Visible = false; }

            if (_config.Debug)
            {
                ImGui.Separator();
                ImGui.Spacing();
                
                // Debug Info Tree
                if (ImGui.TreeNode("Debug Info"))
                {
                    var territory = _territories.First(territory => territory.RowId == Services.ClientState.TerritoryType)!;
                    ImGui.Text($"PlaceName.Name: {territory.PlaceName.Value?.Name}");
                    ImGui.Text($"ContentFinderCondition.Name: {territory.ContentFinderCondition.Value?.Name}");
                    ImGui.Text($"ContentFinderCondition.ContentType.Name: {territory.ContentFinderCondition.Value?.ContentType.Value?.Name}");
                    ImGui.Spacing();

                    ImGui.InputText("##SpecificZoneInfo", ref _specificZoneDebugInfo, 512);
                    var specificTerritory = _territories.FirstOrDefault(territory => Utils.GetDutyNameForTerritory(territory) == _specificZoneDebugInfo);
                    if (specificTerritory != null)
                    {
                        ImGui.Text($"IntendedUse: {specificTerritory.TerritoryIntendedUse}");
                        ImGui.Text($"PlaceName.Name: {specificTerritory.PlaceName.Value?.Name}");
                        ImGui.Text($"ContentFinderCondition.Name: {specificTerritory.ContentFinderCondition.Value?.Name}");
                        ImGui.Text($"ContentType.RowId: {specificTerritory.ContentFinderCondition.Value?.ContentType.Value?.RowId}");
                        ImGui.Text($"ContentType.Name: {specificTerritory.ContentFinderCondition.Value?.ContentType.Value?.Name}");
                    }

                    if (ImGui.Button("Test Zone Types"))
                    {
                        foreach (var territoryType in _territories)
                        {
                            Utils.GetDutyTypeForTerritory(territoryType);
                        }
                    }
                    
                    ImGui.TreePop();
                }   
            }
        }
        ImGui.End();
    }

    private void SaveConfig()
    {
        Services.Config.SetFromOther(_config);
        Services.Config.Save();
        PluginLog.Log("Settings saved.");
    }

    private static void DrawPatternHelpMarker()
    {
        HelpMarker.DrawComponent("Here you can specify name filters for the zones you would like to include (or exclude). " +
                                 "The pattern does not need to specify the whole zone, it is enough if it is just a part of the name. " +
                                 "You can use the wildcard * (asterisk) for any string and ? (question mark) for one character.\n" + 
                                 "Examples:\n" +
                                 "- All Extreme Trials: Include \"Extreme\" and \"The Minstrel's Ballad\" (2 filters)\n" +
                                 "- All Savage Raids: Include \"Savage\"\n" +
                                 "- All Savage Raids except DR: Include \"Savage\" and exclude \"Delubrum Reginae (Savage)\"\n" +
                                 "- All of Eden Savage: Include \"Eden*(Savage)\" (without spaces)");
    }

}
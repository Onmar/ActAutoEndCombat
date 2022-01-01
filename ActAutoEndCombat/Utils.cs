using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace ActAutoEndCombat;

internal static class Utils
{
    public static string GlobToRegex(string glob)
    {
        return Regex.Escape(glob).Replace("\\?", ".").Replace("\\*", ".*");
    }

    public static string GetDutyNameForTerritory(TerritoryType territory)
    {
        string territoryName = territory.ContentFinderCondition.Value?.Name;
        if (string.IsNullOrEmpty(territoryName)) territoryName = territory.PlaceName.Value?.Name;
        if (string.IsNullOrEmpty(territoryName)) return "Unknown";
        // Capitalization correction
        territoryName = territoryName.Replace("- the", "- The");
        territoryName = territoryName.Replace(": the", ": The");
        territoryName = Regex.Replace(territoryName, "^the", "The");
        return territoryName;
    }

    private static readonly Dictionary<string, Config.DutyType> ZoneOverrides = new()
        {
            { "Wolves' Den Pier", Config.DutyType.Pvp }
        };

    public static Config.DutyType GetDutyTypeForTerritory(TerritoryType territory)
    {
        if (territory.ContentFinderCondition.Value?.RowId == 0) return Config.DutyType.World;

        var contentFinderCondition = territory.ContentFinderCondition.Value!;
        var contentType = contentFinderCondition.ContentType.Value!;
        var contentName = GetDutyNameForTerritory(territory);

        if (ZoneOverrides.ContainsKey(contentName))
        {
            return ZoneOverrides[contentName];
        }
        
        switch (contentType.RowId)
        {
            case 0:
            case 7: // "Quest Battles"
            case 16: // "Disciples of the Land" / Ocean Fishing & Diadem
            case 19: // "Gold Saucer"
            case 22: // Event Duties (e.g. The Haunted Manor / The Calamity Retold)
            case 23: // Some weird Diadem Zones?
                return Config.DutyType.World;
            
            case 2: // "Dungeons":
            case 9: // "Treasure Hunt"
            case 20: // Hall of the Novice 
            case 21: // "Deep Dungeons"
            case 27: // Masked Carnivale
                return Config.DutyType.Dungeon;
            
            case 3: // "Guildhests"
                return Config.DutyType.Guildhest;
            
            case 4: // "Trials"
                if (contentName.Contains("Extreme") || contentName.Contains("Minstrel")) return Config.DutyType.TrialExtreme;
                else if (contentName.Contains("Unreal")) return Config.DutyType.TrialUnreal;
                else return Config.DutyType.Trial;
            
            case 5: // "Raids"
                if (territory.TerritoryIntendedUse == 8) return Config.DutyType.RaidAlliance;
                else if (contentName.Contains("Savage")) return Config.DutyType.RaidSavage;
                // else if (contentName.Contains("Ultimate")) return Config.DutyType.RaidUltimate; // Shouldn't exist
                else return Config.DutyType.Raid;
            
            case 28: // "Ultimate Raids"
                return Config.DutyType.RaidUltimate;
            
            case 6: // "PvP"
                return Config.DutyType.Pvp;
            
            case 26: // Eureka (incl. BA)
            case 29: // Bozja & Zadnor (incl. DR / DRS)
                return Config.DutyType.Special;
                
        }

        if (Services.Config?.Debug ?? false)
        {
            var msg = $"Unrecognized Content Type {contentType.RowId} for zone {contentName}";
            PluginLog.LogWarning(msg);
            Services.ChatGui.PrintError(msg);
        }
        
        return Config.DutyType.World;
    }

}
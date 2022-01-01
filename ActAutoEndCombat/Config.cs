using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace ActAutoEndCombat;

[Serializable]
public class Config : IPluginConfiguration
{
    [Flags]
    public enum DutyType : uint
    {
        None = 0, // Helper
        World = 1 << 0, // Non-Duties
        Dungeon = 1 << 1,
        Guildhest = 1 << 2,
        Trial = 1 << 3,
        TrialExtreme = 1 << 4,
        TrialUnreal = 1 << 5,
        Raid = 1 << 6,
        RaidAlliance = 1 << 7,
        RaidSavage = 1 << 8,
        RaidUltimate = 1 << 9,
        Pvp = 1 << 10,
        Special = 1 << 11, // Eureka, Bozja
    }
    
    public int Version { get; set; } = 0;

    public double Delay = 5.0;

    [NonSerialized] public DutyType IncludedDutyTypes = DutyType.None;

    public List<string> IncludePatterns = new();

    public List<string> ExcludePatterns = new();

    public bool Debug = false;

    /// <summary>
    /// Serialization property for IncludedDutyTypes as a List of strings
    /// </summary>
    // [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)] // This didn't work with List<string>, so it's an array now I guess...
    public string[] IncludedDutyTypesByName
    {
        // get
        // {
        //     var dutyTypesByName = new List<string>();
        //     foreach (var dutyType in Enum.GetValues<DutyType>())
        //     {
        //         if (IncludedDutyTypes.HasFlag(dutyType))
        //         {
        //             dutyTypesByName.Add(Enum.GetName(dutyType));
        //         }
        //     }
        //     return dutyTypesByName;
        // }
        get => Enum.GetValues<DutyType>()
            .Where(type => type != DutyType.None)
            .Where(type => IncludedDutyTypes.HasFlag(type))
            .Select(Enum.GetName)
            .Where(name => name != null)
            .ToArray();
        // set
        // {
        //     var dutyType = DutyType.None;
        //     foreach (var dutyTypeName in value)
        //     {
        //         dutyType |= Enum.Parse<DutyType>(dutyTypeName);
        //     }
        //     IncludedDutyTypes = dutyType;
        // }
        set => IncludedDutyTypes = value.Aggregate(DutyType.None, (current, dutyTypeName) => current | Enum.Parse<DutyType>(dutyTypeName));
    }

    [NonSerialized] private DalamudPluginInterface _pluginInterface;

    public event EventHandler<Config> ConfigChanged;

    public Config()
    {
    }

    public Config(Config other)
    {
        SetFromOther(other);
    }

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this._pluginInterface = pluginInterface;

        IncludePatterns ??= new List<string>();
        ExcludePatterns ??= new List<string>();
    }

    public void SetFromOther(Config other)
    {
        this.Version = other.Version;
        this.Delay = other.Delay;
        this.IncludedDutyTypes = other.IncludedDutyTypes;
        this.IncludePatterns = new List<string>(other.IncludePatterns);
        this.ExcludePatterns = new List<string>(other.ExcludePatterns);
        this.Debug = other.Debug;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
        ConfigChanged?.Invoke(this, this);
    }
}
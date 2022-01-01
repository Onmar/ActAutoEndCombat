using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace ActAutoEndCombat.Windows.Components;

public class DutyTypeTable
{

    public event EventHandler TypesChanged;

    private readonly Config _config;

    // UI state variables
    private readonly bool[] _ba;
    
    public DutyTypeTable(Config config)
    {
        _config = config;
        // Needs to be a bool[] since we can't ref elements of a BitArray (or BitVector32)
        _ba = new BitArray(new[] { (int)_config.IncludedDutyTypes }).Cast<bool>().ToArray();
    }
    
    public void DrawComponent()
    {

        if (ImGui.BeginTable("dutyTypeTable", 3, ImGuiTableFlags.None))
        {
            var changed = false;
            // Row 1
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("General World", ref _ba[BitOperations.Log2((uint)Config.DutyType.World)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Dungeons", ref _ba[BitOperations.Log2((uint)Config.DutyType.Dungeon)])) changed = true;
            ImGui.SameLine();
            HelpMarker.DrawComponent("incl. Treasure Dungeons / Deep Dungeons / Masked Carnivale");
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Guildhests", ref _ba[BitOperations.Log2((uint)Config.DutyType.Guildhest)])) changed = true;

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Trials", ref _ba[BitOperations.Log2((uint)Config.DutyType.Trial)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Extreme Trials", ref _ba[BitOperations.Log2((uint)Config.DutyType.TrialExtreme)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Unreal Trials", ref _ba[BitOperations.Log2((uint)Config.DutyType.TrialUnreal)])) changed = true;

            // Row 3
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Raids", ref _ba[BitOperations.Log2((uint)Config.DutyType.Raid)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Savage Raids", ref _ba[BitOperations.Log2((uint)Config.DutyType.RaidSavage)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Ultimate Raids", ref _ba[BitOperations.Log2((uint)Config.DutyType.RaidUltimate)])) changed = true;

            // Row 4
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Alliance Raids", ref _ba[BitOperations.Log2((uint)Config.DutyType.RaidAlliance)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Pvp", ref _ba[BitOperations.Log2((uint)Config.DutyType.Pvp)])) changed = true;
            ImGui.TableNextColumn();
            if (ImGui.Checkbox("Special", ref _ba[BitOperations.Log2((uint)Config.DutyType.Special)])) changed = true;
            ImGui.SameLine();
            HelpMarker.DrawComponent("Eureka & Bozja (incl. BA / DR / DRS)");

            if (changed)
            {
                var uintArray = new uint[1];
                new BitArray(_ba).CopyTo(uintArray, 0);
                _config.IncludedDutyTypes = (Config.DutyType)uintArray[0];
                TypesChanged?.Invoke(this, EventArgs.Empty);
            }
            
            ImGui.EndTable();
        }
        
    }
    
    private static void SetFlag(ref Config.DutyType value, Config.DutyType flag, bool state)
    {
        // if (state)
        // {
        //     value |= flag;
        // }
        // else
        // {
        //     value &= ~flag;
        // }
        value = state ? value | flag : value & ~flag;
    }
    
}
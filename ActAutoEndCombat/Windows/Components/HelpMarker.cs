using ImGuiNET;

namespace ActAutoEndCombat.Windows.Components;

public static class HelpMarker
{
    public static void DrawComponent(string desc)
    {
        ImGui.TextDisabled("(?)"); // ToDo: Replace with icon button
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
}
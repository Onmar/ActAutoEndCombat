using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ImGuiNET;

namespace ActAutoEndCombat.Windows.Components;

public class PatternTable
{
    // Minimum Length at which to display the matched values for a new pattern
    private const uint MinNewFilterLength = 3;

    public event EventHandler PatternsChanged;

    private readonly string _identifier;
    private readonly List<string> _patternList;

    // UI state variables for the new pattern
    private string _newPattern = "";
    private string _lastNewPattern = "";

    // Caches for the values of patterns
    private Dictionary<string, List<string>> _patternCache = new();
    private List<string> _newPatternCache = new();

    // Values which will be matched by the patterns.
    private List<string> _values = new();
    public List<string> Values { 
        private get => _values; 
        set
        {
            _values = value;
            _patternCache = _patternList.ToDictionary(pattern => pattern, FilterValues);
            _newPatternCache = _newPattern.Length < MinNewFilterLength ? new List<string>() : FilterValues(_newPattern);
        }
    }

    public PatternTable(string identifier, List<string> patternList, List<string> values)
    {
        _identifier = identifier;
        _patternList = patternList;
        Values = values; // Accessor also initializes pattern caches
    }

    public void DrawComponent()
    {
        if (ImGui.BeginTable($"{_identifier}PatternTable", 3))
        {
            ImGui.TableSetupColumn("Filter", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Nr. of Zones");
            ImGui.TableSetupColumn("");
            ImGui.TableHeadersRow();

            // Existing Filters
            string toRemove = null;
            foreach (var pattern in _patternList)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(pattern);
                ImGui.TableNextColumn();
                ImGui.Text($"{_patternCache[pattern].Count}");
                // Tooltip with matched values
                if (ImGui.IsItemHovered() && _patternCache[pattern].Count > 0)
                {
                    ImGui.BeginTooltip();
                    foreach (var value in _patternCache[pattern]) ImGui.Text(value);
                    ImGui.EndTooltip();
                }
                ImGui.TableNextColumn();
                if (ImGui.Button($"X##Remove{_identifier}{pattern}")) // ToDo:  Replace with icon button
                {
                    // Save which to remove for later. Should not be able to click two buttons on same frame, so this should work just fine.
                    toRemove = pattern;
                }
            }
            // Need to do this outside of loop since we are iterating over _patternList
            if (toRemove != null)
            {
                _patternList.Remove(toRemove);
                _patternCache.Remove(toRemove);
                PatternsChanged?.Invoke(this, EventArgs.Empty);
            }

            // New Filter
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.InputText($"##new{_identifier}Pattern", ref _newPattern, 512);
            // Update Zone List if input changed. Probably could do this with a callback but I don't know how to do that with ImGui.
            if (_newPattern != _lastNewPattern)
            {
                // Only do it if the filter is longer than 3 characters. This is to save resources with short patterns which will match mostly everything.
                if (_newPattern.Length < MinNewFilterLength) _newPatternCache.Clear();
                    
                // Disabled since it does not affect performance enough to be a problem
                //// If the new pattern is longer just remove the ones which do not match it anymore.
                //// There is an edge case to this if you replace a string with a longer string (Ctrl+v), but it will be fixed once you backspace.
                //// Also, the list is rebuilt fully if you add the filter, so it's not an issue there as well.
                //else if (newPattern.Length > lastNewPattern.Length && lastNewPattern.Length >= MinNewFilterLength)
                //{
                //    string regex = Utils.GlobToRegex(newPattern);
                //    newPatternCache.RemoveAll(zoneName => !Regex.IsMatch(zoneName, regex));
                //}
                    
                // Rebuild zone list from scratch
                else _newPatternCache = FilterValues(_newPattern);

                _lastNewPattern = _newPattern;
            }
            ImGui.TableNextColumn();
            ImGui.Text($"{(_newPattern.Length < 3 ? "" : _newPatternCache.Count)}");
            // Tooltip with matched values
            if (ImGui.IsItemHovered() && _newPatternCache.Count > 0)
            {
                ImGui.BeginTooltip();
                foreach (var value in _newPatternCache) ImGui.Text(value);
                ImGui.EndTooltip();
            }
            ImGui.TableNextColumn();
            if (ImGui.Button($"Add##addNew{_identifier}Pattern")) // ToDo: Replace with icon button
            {
                // Add only if it is not blank and does not exist already
                if (_newPattern != "" && !_patternList.Contains(_newPattern))
                {
                    _patternList.Add(_newPattern);
                    // Update Cache
                    _patternCache.Add(_newPattern, FilterValues(_newPattern));
                    _newPattern = "";
                    PatternsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            ImGui.EndTable();
        }
    }

    private List<string> FilterValues(string pattern)
    {
        Regex regex = new(Utils.GlobToRegex(pattern.ToLower()));
        // Distinct() cause there are multiple entries for certain names (e.g. housing subdivisions)
        return Values.Where(name => regex.IsMatch(name.ToLower())).Distinct().ToList();
    }

}
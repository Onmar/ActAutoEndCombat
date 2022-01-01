using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text;
using Lumina.Excel.GeneratedSheets;

namespace ActAutoEndCombat.Managers
{
    public class ActAutoEndManager : IDisposable
    {
        private static readonly XivChatEntry ActEndChatEntry = new()
        {
            Message = "end",
            Type = XivChatType.Echo
        };

        public bool Active { get; private set; } = false;

        private readonly Dictionary<ushort, TerritoryType> _territories;

        private readonly Timer _timer;

        public ActAutoEndManager()
        {
            // Game Data Initialization
            _territories = Services.DataManager.GetExcelSheet<TerritoryType>()!.ToDictionary(t => (ushort) t.RowId, t => t);

            // Timer for delayed end
            _timer = new Timer();
            _timer.AutoReset = false;
            _timer.Elapsed += (sender, args) => Services.ChatGui.PrintChat(ActEndChatEntry);

            Services.ClientState.TerritoryChanged += State_TerritoryChanged;
            Services.Config.ConfigChanged += Config_ConfigChanged;

            State_TerritoryChanged(null, Services.ClientState.TerritoryType);
        }

        public void Dispose()
        {
            Services.Config.ConfigChanged -= Config_ConfigChanged; 
            Services.ClientState.TerritoryChanged -= State_TerritoryChanged;
            Deactivate();
            _timer.Stop();
            _timer.Dispose();
        }

        private void Config_ConfigChanged(object sender, Config newConfig)
        {
            State_TerritoryChanged(null, Services.ClientState.TerritoryType);
        }
        
        private void State_TerritoryChanged(object sender, ushort territoryId)
        {
            if (territoryId == 0)
            {
                Deactivate();
                return;
            }

            var territoryIncluded = CheckTerritory(territoryId);
            if (territoryIncluded) Activate();
            else Deactivate();
        }

        private bool CheckTerritory(ushort territoryId)
        {
            var territory = _territories[territoryId];
            var territoryName = Utils.GetDutyNameForTerritory(territory);
            
            // Check Included Duty Types
            if (Services.Config.IncludedDutyTypes != Config.DutyType.None || Services.Config.IncludePatterns.Count != 0)
            {
                var includedByDutyType = false;
                if (Services.Config.IncludedDutyTypes != Config.DutyType.None)
                {
                    includedByDutyType = Services.Config.IncludedDutyTypes.HasFlag(Utils.GetDutyTypeForTerritory(territory));   
                }
                
                // Check Include Patterns
                var includedByIncludePattern = Services.Config.IncludePatterns.Any(includePattern => Regex.IsMatch(territoryName.ToLower(), Utils.GlobToRegex(includePattern.ToLower())));

                if (!includedByDutyType && !includedByIncludePattern) return false;
            }

            // Check Exclude Patterns
            if (Services.Config.ExcludePatterns.Count != 0)
            {
                var excluded = Services.Config.ExcludePatterns.Any(excludePattern => Regex.IsMatch(territoryName, Utils.GlobToRegex(excludePattern)));
                if (excluded) return false;
            }

            return true;
        }

        public void Activate()
        {
            if (Active) return;
            
            Active = true;
            Services.Condition.ConditionChange += Condition_OnConditionChange;
        }

        public void Deactivate()
        {
            if (!Active) return;
            
            Services.Condition.ConditionChange -= Condition_OnConditionChange;
            Active = false;
        }

        private void Condition_OnConditionChange(ConditionFlag flag, bool value)
        {
            if (flag != ConditionFlag.InCombat) return;

            if (!value)
            {
                if (Services.Config.Delay == 0.0)
                {
                    Services.ChatGui.PrintChat(ActEndChatEntry);
                }
                else
                {
                    _timer.Interval = Services.Config.Delay * 1000.0;
                    _timer.Start();
                }
            }
            else
            {
                // If we enter combat again before delay expired, cancel
                _timer.Stop();
            }
        }
    }
}

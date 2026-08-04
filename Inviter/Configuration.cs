using Dalamud.Configuration;
using Dalamud.Game.Text;
using System.Collections.Generic;

namespace Inviter
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool Enable = false;
        public bool ShowTooltips = true;
        public bool ShowDtrEntry = true;
        public string UILanguage = "en";
        public string TextPattern = "";
        public bool RegexMatch = false;
        public bool PrintMessage = false;
        public bool PrintError = true;
        public bool EnableQuestToast = true;
        public bool EnableNormalToast = false;
        public bool EnableErrorToast = false;
        /// <summary>
        /// Invite delay is randomized between these two (inclusive) each time, rather than
        /// fixed, mainly so a string of invites doesn't look obviously scripted/bot-timed.
        /// If DelayMax &lt; DelayMin the actual random draw (see Inviter.SendInviteAsync)
        /// swaps them defensively, so a hand-edited config can't end up with an invalid range.
        /// </summary>
        public int DelayMin = 200;
        public int DelayMax = 200;
        public int Ratelimit = 500;

        public List<XivChatType> FilteredChannels = [];
        public List<TerritoryRule> TerritoryRules = [];
        public List<XivChatType> HiddenChatType = [
            XivChatType.None,
            XivChatType.CustomEmote,
            XivChatType.StandardEmote,
            XivChatType.SystemMessage,
            XivChatType.SystemError,
            XivChatType.GatheringSystemMessage,
            XivChatType.ErrorMessage,
            XivChatType.RetainerSale
        ];

        public void Save()
        {
            Svc.PluginInterface.SavePluginConfig(this);
        }
    }

    /// <summary>
    /// Inherit (default): doesn't touch on/off at all, only carries a pattern/regex
    /// override if set, and otherwise leaves the zone following the global Enable switch.
    /// ForceOn: explicit opt-in to run here even if Enable is off globally.
    /// ForceOff: never run here, even if Enable is on globally.
    /// </summary>
    public enum TerritoryEnableMode
    {
        Inherit,
        ForceOn,
        ForceOff,
    }

    /// <summary>
    /// A per-zone override, keyed on TerritoryType rather than ContentFinderCondition
    /// (duty ID) so this also covers open-world zones like Eureka/Bozja that have no
    /// duty ID at all. This is the sole "where does Inviter behave differently"
    /// mechanism - see the resolution logic in Inviter.MsgHookDetour.
    /// </summary>
    public class TerritoryRule
    {
        public uint TerritoryId;
        public TerritoryEnableMode EnableMode = TerritoryEnableMode.Inherit;
        public string? TextPattern;

        /// <summary>
        /// Only meaningful alongside TextPattern - whether that override pattern is regex.
        /// Deliberately a plain bool, not bool?: "inherit the global regex setting" doesn't
        /// make sense for a pattern that isn't the global pattern in the first place.
        /// </summary>
        public bool RegexMatch;
    }
}

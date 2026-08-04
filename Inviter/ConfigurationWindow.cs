using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using System;
using System.Linq;
using System.Numerics;

namespace Inviter
{
    public class ConfigurationWindow : Window
    {
        private readonly string[] _languageList = ["en", "zh", "fr"];
        private int _selectedLanguage;
        private string _ruleTerritorySearch = "";

        public ConfigurationWindow() : base($"Inviter {Inviter.Plugin.localizer.Localize("Panel")}")
        {
            _selectedLanguage = Array.IndexOf(_languageList, Inviter.Plugin.Config.UILanguage);
            SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = new(460, 340),
                MaximumSize = new(int.MaxValue, int.MaxValue)
            };
        }

        public override void OnClose()
        {
            Inviter.Plugin.Config.Save();
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("InviterConfigTabs"))
            {
                if (ImGui.BeginTabItem(Inviter.Plugin.localizer.Localize("Features")))
                {
                    DrawFeatures();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem(Inviter.Plugin.localizer.Localize("Channels")))
                {
                    DrawChannels();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem(Inviter.Plugin.localizer.Localize("User Restriction")))
                {
                    DrawUserRestriction();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
        }

        // ---------------------------------------------------------------
        // Features: master switches and general behavior.
        // ---------------------------------------------------------------

        private void DrawFeatures()
        {
            if (ImGui.Checkbox("##enable", ref Inviter.Plugin.Config.Enable))
            {
                Inviter.Plugin.Config.Save();
                Inviter.Plugin.UpdateDtrBar();
            }
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Automatically invite people to your party (doesn't work for CWLS)."));
            ImGui.SameLine();
            ImGui.TextColored(
                Inviter.Plugin.Config.Enable ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed,
                Inviter.Plugin.Config.Enable
                    ? Inviter.Plugin.localizer.Localize("Inviter Enabled")
                    : Inviter.Plugin.localizer.Localize("Inviter Disabled"));

            ImGui.Separator();

            ImGui.TextUnformatted(Inviter.Plugin.localizer.Localize("Delay(ms):"));
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Delay the invitation after triggered, randomized between these two values each time."));
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##DelayMin", ref Inviter.Plugin.Config.DelayMin, 10, 100))
            {
                Inviter.Plugin.Config.DelayMin = Math.Max(0, Inviter.Plugin.Config.DelayMin);
                if (Inviter.Plugin.Config.DelayMax < Inviter.Plugin.Config.DelayMin)
                    Inviter.Plugin.Config.DelayMax = Inviter.Plugin.Config.DelayMin;
                Inviter.Plugin.Config.Save();
            }
            ImGui.SameLine();
            ImGui.TextUnformatted(Inviter.Plugin.localizer.Localize("to"));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("##DelayMax", ref Inviter.Plugin.Config.DelayMax, 10, 100))
            {
                Inviter.Plugin.Config.DelayMax = Math.Max(0, Inviter.Plugin.Config.DelayMax);
                if (Inviter.Plugin.Config.DelayMin > Inviter.Plugin.Config.DelayMax)
                    Inviter.Plugin.Config.DelayMin = Inviter.Plugin.Config.DelayMax;
                Inviter.Plugin.Config.Save();
            }

            ImGui.TextUnformatted(Inviter.Plugin.localizer.Localize("Rate limit (ms):"));
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("How much time must pass between invitations."));
            ImGui.SetNextItemWidth(150);
            if (ImGui.InputInt("##Ratelimit", ref Inviter.Plugin.Config.Ratelimit, 10, 100))
                Inviter.Plugin.Config.Save();

            ImGui.Separator();

            if (ImGui.Checkbox("##dtrEnabled", ref Inviter.Plugin.Config.ShowDtrEntry))
            {
                Inviter.Plugin.Config.Save();
                Inviter.Plugin.UpdateDtrBar();
            }
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Show an Inviter status entry in the server info bar. Left-click toggles it, right-click opens a quick menu."));
            ImGui.SameLine();
            ImGui.TextColored(
                Inviter.Plugin.Config.ShowDtrEntry ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed,
                Inviter.Plugin.Config.ShowDtrEntry
                    ? Inviter.Plugin.localizer.Localize("Server Info Bar Enabled")
                    : Inviter.Plugin.localizer.Localize("Server Info Bar Disabled"));

            ImGui.Separator();

            ImGui.TextUnformatted(Inviter.Plugin.localizer.Localize("Language:"));
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Change the UI Language."));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.Combo("##hideLangSetting", ref _selectedLanguage, _languageList, _languageList.Length))
            {
                Inviter.Plugin.Config.UILanguage = _languageList[_selectedLanguage];
                if (Inviter.Plugin.Config.TextPattern == "111" || Inviter.Plugin.Config.TextPattern == "inv")
                {
                    if (Inviter.Plugin.Config.UILanguage == "zh")
                        Inviter.Plugin.Config.TextPattern = "111";
                    else
                        Inviter.Plugin.Config.TextPattern = "inv";
                }
                Inviter.Plugin.localizer.Language = Inviter.Plugin.Config.UILanguage;
                Inviter.Plugin.Config.Save();
            }

            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Tooltips"), ref Inviter.Plugin.Config.ShowTooltips))
                Inviter.Plugin.Config.Save();

            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Print Debug Message"), ref Inviter.Plugin.Config.PrintMessage))
                Inviter.Plugin.Config.Save();
            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Print Error Message"), ref Inviter.Plugin.Config.PrintError))
                Inviter.Plugin.Config.Save();

            ImGui.Separator();

            ImGui.TextUnformatted(Inviter.Plugin.localizer.Localize("Display as Toasts:"));
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Show status changes as a pop-up toast, using the styles below. More than one can be enabled at once."));
            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Quest"), ref Inviter.Plugin.Config.EnableQuestToast))
                Inviter.Plugin.Config.Save();
            ImGui.SameLine();
            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Normal"), ref Inviter.Plugin.Config.EnableNormalToast))
                Inviter.Plugin.Config.Save();
            ImGui.SameLine();
            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Error"), ref Inviter.Plugin.Config.EnableErrorToast))
                Inviter.Plugin.Config.Save();
        }

        // ---------------------------------------------------------------
        // Channels: which chat channels are checked at all.
        // ---------------------------------------------------------------

        // Channels where the sender is already in a group with you, so inviting them is
        // meaningless (they're already partied/allianced/teamed) or outright not possible.
        private static readonly XivChatType[] AlreadyGroupedTypes =
        [
            XivChatType.Party,
            XivChatType.Alliance,
            XivChatType.CrossParty,
            XivChatType.PvPTeam,
        ];

        // Channels that carry a XivChatTypeInfoAttribute but whose messages don't come from
        // another player you could invite: system broadcasts, or Echo, which only you can see
        // (so matching it would just mean inviting yourself).
        private static readonly XivChatType[] NotAPlayerMessageTypes =
        [
            XivChatType.Urgent,
            XivChatType.Notice,
            XivChatType.Echo,
            XivChatType.CustomEmote,
            XivChatType.StandardEmote,
        ];

        // Derived from Dalamud's own XivChatType metadata instead of a hand-picked list, so it
        // stays correct as Dalamud adds/renumbers chat types: only types with real chat-log info
        // (GetDetails() != null, i.e. what shows up in the game's own channel config) are
        // "visible" player channels, GM broadcast variants are dropped via IsUsedByGm(), and the
        // remaining channels you could never actually invite someone from are excluded above.
        private static readonly XivChatType[] PublicChatTypes =
        [
            .. Enum.GetValues<XivChatType>()
                .Where(c => c.GetDetails() is not null)
                .Where(c => !c.IsUsedByGm())
                .Except(AlreadyGroupedTypes)
                .Except(NotAPlayerMessageTypes)
        ];

        private static void DrawChannels()
        {
            if (ImGui.Button(Inviter.Plugin.localizer.Localize("All") + "##filtersAll"))
            {
                Inviter.Plugin.Config.FilteredChannels = [];
                Inviter.Plugin.Config.Save();
            }
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Check every channel."));

            ImGui.SameLine();
            if (ImGui.Button(Inviter.Plugin.localizer.Localize("Clear") + "##filtersClear"))
            {
                Inviter.Plugin.Config.FilteredChannels = [.. PublicChatTypes];
                Inviter.Plugin.Config.Save();
            }
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Uncheck every channel."));

            ImGui.Separator();

            ImGui.Columns(4, "FiltersTable", true);
            foreach (XivChatType chatType in PublicChatTypes)
            {
                if (Inviter.Plugin.Config.HiddenChatType.Contains(chatType))
                    continue;
                string chatTypeName = Enum.GetName(chatType)!;
                bool checkboxClicked = Inviter.Plugin.Config.FilteredChannels.IndexOf(chatType) == -1;
                if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize(chatTypeName) + "##filter", ref checkboxClicked))
                {
                    Inviter.Plugin.Config.FilteredChannels = [.. Inviter.Plugin.Config.FilteredChannels.Distinct()];
                    if (checkboxClicked)
                    {
                        if (Inviter.Plugin.Config.FilteredChannels.IndexOf(chatType) != -1)
                            Inviter.Plugin.Config.FilteredChannels.Remove(chatType);
                    }
                    else if (Inviter.Plugin.Config.FilteredChannels.IndexOf(chatType) == -1)
                    {
                        Inviter.Plugin.Config.FilteredChannels.Add(chatType);
                    }
                    Inviter.Plugin.Config.FilteredChannels = [.. Inviter.Plugin.Config.FilteredChannels.Distinct()];
                    Inviter.Plugin.Config.FilteredChannels.Sort();
                    Inviter.Plugin.Config.Save();
                }
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
        }

        // ---------------------------------------------------------------
        // User Restriction: default matching rule ("Everywhere...") plus
        // per-zone exceptions ("...but for these Maps").
        // ---------------------------------------------------------------

        private void DrawUserRestriction()
        {
            if (ImGui.BeginTabBar("UserRestrictionTabs"))
            {
                if (ImGui.BeginTabItem(Inviter.Plugin.localizer.Localize("Everywhere...")))
                {
                    DrawEverywhere();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem(Inviter.Plugin.localizer.Localize("...but for these Maps")))
                {
                    DrawMaps();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
        }

        private void DrawEverywhere()
        {
            ImGui.TextWrapped(Inviter.Plugin.localizer.Localize(
                "The default pattern used everywhere, unless a zone under \"...but for these Maps\" overrides it."));
            ImGui.Separator();

            ImGui.TextUnformatted(Inviter.Plugin.localizer.Localize("Pattern:"));
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Pattern of the chat message to trigger the invitation."));
            if (ImGui.InputText("##textPattern", ref Inviter.Plugin.Config.TextPattern, 256))
                Inviter.Plugin.Config.Save();
            ImGui.SameLine();
            if (ImGui.Checkbox(Inviter.Plugin.localizer.Localize("Regex"), ref Inviter.Plugin.Config.RegexMatch))
                Inviter.Plugin.Config.Save();
            if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Use regex to match the pattern to chat messages."));

            if (string.IsNullOrWhiteSpace(Inviter.Plugin.Config.TextPattern))
            {
                ImGui.TextColored(new Vector4(1f, 0.55f, 0.2f, 1f),
                    Inviter.Plugin.localizer.Localize("Pattern is empty — Unless overridden for a zone, inviter will not match any messages until you enter one."));
            }
        }

        private static string GetTerritoryName(uint id)
        {
            var name = Svc.Data.GetExcelSheet<TerritoryType>().GetRowOrDefault(id)?.PlaceName.ValueNullable?.Name.ToString();
            return string.IsNullOrEmpty(name) ? $"#{id}" : name;
        }

        /// <summary>
        /// Per-zone rules - the sole "where does Inviter behave differently" mechanism.
        /// Inherit (default) leaves the zone following the global Enable switch and only
        /// optionally overrides Pattern/Regex; Force On/Off explicitly override Enable for
        /// that zone regardless of the global switch. See Inviter.MsgHookDetour.
        /// </summary>
        private void DrawMaps()
        {
            ImGui.TextWrapped(Inviter.Plugin.localizer.Localize(
                "Zones listed here override \"Everywhere...\". Inherit only overrides the Pattern (and whether it's " +
                "regex) if you check its box below; Force On/Off override the main Enable switch for that zone specifically."));
            ImGui.Separator();

            var rules = Inviter.Plugin.Config.TerritoryRules;
            string[] modeLabels =
            [
                Inviter.Plugin.localizer.Localize("Inherit"),
                Inviter.Plugin.localizer.Localize("Force On"),
                Inviter.Plugin.localizer.Localize("Force Off"),
            ];

            if (rules.Count == 0)
            {
                ImGui.TextDisabled(Inviter.Plugin.localizer.Localize("(No zone rules added.)"));
            }
            else if (ImGui.BeginTable("TerritoryRulesTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn(Inviter.Plugin.localizer.Localize("Mode"), ImGuiTableColumnFlags.WidthFixed, 110f);
                ImGui.TableSetupColumn(Inviter.Plugin.localizer.Localize("Zone"), ImGuiTableColumnFlags.WidthStretch, 2f);
                ImGui.TableSetupColumn(Inviter.Plugin.localizer.Localize("Pattern override"), ImGuiTableColumnFlags.WidthStretch, 2f);
                ImGui.TableSetupColumn(Inviter.Plugin.localizer.Localize("Regex"), ImGuiTableColumnFlags.WidthFixed, 55f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 60f);
                ImGui.TableHeadersRow();

                TerritoryRule? toRemove = null;
                foreach (var rule in rules)
                {
                    ImGui.PushID((int)rule.TerritoryId);
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    var modeIndex = (int)rule.EnableMode;
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.Combo("##ruleMode", ref modeIndex, modeLabels, modeLabels.Length))
                    {
                        rule.EnableMode = (TerritoryEnableMode)modeIndex;
                        Inviter.Plugin.Config.Save();
                        Inviter.Plugin.UpdateDtrBar();
                    }

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(GetTerritoryName(rule.TerritoryId));

                    ImGui.TableNextColumn();
                    var hasCustomPattern = rule.TextPattern != null;
                    if (ImGui.Checkbox("##ruleHasPattern", ref hasCustomPattern))
                    {
                        rule.TextPattern = hasCustomPattern ? Inviter.Plugin.Config.TextPattern : null;
                        Inviter.Plugin.Config.Save();
                    }
                    if (hasCustomPattern)
                    {
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(-1);
                        var pattern = rule.TextPattern ?? "";
                        if (ImGui.InputText("##rulePattern", ref pattern, 100))
                        {
                            rule.TextPattern = pattern;
                            Inviter.Plugin.Config.Save();
                        }
                    }

                    ImGui.TableNextColumn();
                    if (hasCustomPattern)
                    {
                        var regexValue = rule.RegexMatch;
                        if (ImGui.Checkbox("##ruleRegex", ref regexValue))
                        {
                            rule.RegexMatch = regexValue;
                            Inviter.Plugin.Config.Save();
                        }
                    }
                    else
                    {
                        ImGui.TextDisabled("-");
                        if (Inviter.Plugin.Config.ShowTooltips && ImGui.IsItemHovered())
                            ImGui.SetTooltip(Inviter.Plugin.localizer.Localize("Only applies when this zone has a pattern override."));
                    }

                    ImGui.TableNextColumn();
                    if (ImGui.SmallButton(Inviter.Plugin.localizer.Localize("Remove")))
                        toRemove = rule;

                    ImGui.PopID();
                }

                ImGui.EndTable();

                if (toRemove != null)
                {
                    rules.Remove(toRemove);
                    Inviter.Plugin.Config.Save();
                    Inviter.Plugin.UpdateDtrBar();
                }
            }

            ImGui.Dummy(new Vector2(0, 6));

            if (ImGui.Button(Inviter.Plugin.localizer.Localize("Add Current Zone") + "##addCurrentZoneRule"))
            {
                var currentId = Svc.ClientState.TerritoryType;
                if (currentId != 0 && !rules.Any(r => r.TerritoryId == currentId))
                {
                    rules.Add(new TerritoryRule { TerritoryId = currentId });
                    Inviter.Plugin.Config.Save();
                    Inviter.Plugin.UpdateDtrBar();
                }
            }

            ImGui.SetNextItemWidth(250);
            ImGui.InputTextWithHint("##ruleTerritorySearch", Inviter.Plugin.localizer.Localize("Search zone name..."), ref _ruleTerritorySearch, 100);

            if (!string.IsNullOrWhiteSpace(_ruleTerritorySearch))
            {
                ImGui.BeginChild("##ruleTerritorySearchResults", new Vector2(0, -1));
                foreach (var territory in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    var name = territory.PlaceName.ValueNullable?.Name.ToString();
                    if (string.IsNullOrEmpty(name) || !name.Contains(_ruleTerritorySearch, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (rules.Any(r => r.TerritoryId == territory.RowId))
                        continue;

                    if (ImGui.Selectable($"{name}##ruleTerritory{territory.RowId}"))
                    {
                        rules.Add(new TerritoryRule { TerritoryId = territory.RowId });
                        Inviter.Plugin.Config.Save();
                        Inviter.Plugin.UpdateDtrBar();
                        _ruleTerritorySearch = "";
                    }
                }
                ImGui.EndChild();
            }
        }
    }
}

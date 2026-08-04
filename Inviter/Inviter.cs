using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.Toast;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Inviter
{
    public class Inviter : IDalamudPlugin
    {
        public static Inviter Plugin = null!;
        public Configuration Config { get; private set; }
        public Localizer localizer;
        public TimedEnable timedRecruitment;
        public WindowSystem WindowSystem = new("Inviter");
        public ConfigurationWindow ConfigurationWindow;

        private long NextInviteAt = 0;
        private readonly Hook<RaptureLogModule.Delegates.AddMsgSourceEntry> MsgHook;

        private IDtrBarEntry? DtrEntry;
        private bool _openDtrMenu;
        private Vector2 _dtrMenuPos;
        private int _customMinutes = 15;
        private int _customAttempts = 0;

        public unsafe Inviter(IDalamudPluginInterface PluginInterface)
        {
            Plugin = this;
            Svc.Init(PluginInterface);
            Config = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            localizer = new Localizer(Config.UILanguage);
            timedRecruitment = new();

            MsgHook = Svc.Hook.HookFromAddress<RaptureLogModule.Delegates.AddMsgSourceEntry>(RaptureLogModule.MemberFunctionPointers.AddMsgSourceEntry, MsgHookDetour);
            MsgHook.Enable();

            Svc.Commands.AddHandler("/xinvite", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/xinvite - open the inviter panel.\n" +
                    "/xinvite <on/off/toggle> - turn the auto invite on/off.\n" +
                    "/xinvite <minutes> - enable temporary auto invite for certain amount of time in minutes.\n" +
                    "/xinvite <minutes> <attempts> - enable temporary auto invite for certain amount of time in minutes and finish it after certain amount of invite attempts."
            });
            ConfigurationWindow = new();
            WindowSystem.AddWindow(ConfigurationWindow);
            Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
            Svc.PluginInterface.UiBuilder.Draw += DrawDtrMenu;
            Svc.PluginInterface.UiBuilder.OpenMainUi += ConfigurationWindow.Toggle;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += ConfigurationWindow.Toggle;

            DtrEntry = Svc.DtrBar.Get("Inviter");
            DtrEntry.OnClick = OnDtrClick;
            Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
            UpdateDtrBar();
        }

        public void Dispose()
        {
            timedRecruitment.FinishTimer();
            MsgHook.Dispose();
            DtrEntry?.Remove();
            Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
            Svc.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
            Svc.PluginInterface.UiBuilder.Draw -= DrawDtrMenu;
            Svc.PluginInterface.UiBuilder.OpenMainUi -= ConfigurationWindow.Toggle;
            Svc.PluginInterface.UiBuilder.OpenConfigUi -= ConfigurationWindow.Toggle;
            WindowSystem.RemoveAllWindows();
            Svc.Commands.RemoveHandler("/xinvite");
        }

        private void OnTerritoryChanged(uint territoryId) => UpdateDtrBar(territoryId);

        /// <summary>
        /// Resolves whether Inviter is effectively enabled for a given zone: an explicit
        /// Force On/Off rule overrides the global switch outright; Inherit (or no rule at
        /// all) just follows it. Shared by MsgHookDetour (per-message) and UpdateDtrBar
        /// (per-zone-change/state-change) so the two can't disagree with each other.
        /// </summary>
        private static bool ResolveEffectiveEnable(bool globalEnable, TerritoryRule? rule)
        {
            if (rule?.EnableMode == TerritoryEnableMode.ForceOn)
                return true;
            if (rule?.EnableMode == TerritoryEnableMode.ForceOff)
                return false;
            return globalEnable;
        }

        /// <summary>
        /// Resolves the effective text pattern for a zone: the zone's own override if it has
        /// one, otherwise the global pattern. Shared by MsgHookDetour (matching) and
        /// CommandHandler (the "turned on for {0}" toast), so the toast can't claim a
        /// different pattern than what's actually being matched against in that zone.
        /// </summary>
        private static string ResolveEffectiveTextPattern(Configuration config, TerritoryRule? rule) =>
            rule != null && !string.IsNullOrEmpty(rule.TextPattern) ? rule.TextPattern : config.TextPattern;

        /// <summary>Looks up the territory rule, if any, for whatever zone the player is currently in.</summary>
        private TerritoryRule? GetCurrentZoneRule() =>
            Config.TerritoryRules.Find(r => r.TerritoryId == Svc.ClientState.TerritoryType);

        /// <summary>
        /// Fires a status notification in whichever toast style(s) are enabled (Quest/
        /// Normal/Error can be on simultaneously, mirroring LazyLoot's toast checkboxes) -
        /// for actual on/off/timed-session status changes, not validation errors, which
        /// stay as a plain Svc.Toasts.ShowError regardless of this setting.
        /// </summary>
        public void ShowStatusToast(string message, bool checkmark = true, bool sound = true)
        {
            if (Config.EnableQuestToast)
                Svc.Toasts.ShowQuest(message, new QuestToastOptions { DisplayCheckmark = checkmark, PlaySound = sound });
            if (Config.EnableNormalToast)
                Svc.Toasts.ShowNormal(message);
            if (Config.EnableErrorToast)
                Svc.Toasts.ShowError(message);
        }

        /// <summary>
        /// Left-click toggles auto-invite; right-click opens the quick menu. Which "toggle"
        /// happens on the left depends on the current zone's rule: Forced zones flip the
        /// force state directly (staying Forced, just swapping On/Off) since the whole point
        /// of Forcing is to ignore the global switch; unlisted zones and Inherit both just
        /// flip the global switch, since that's what they're following anyway.
        /// </summary>
        private void OnDtrClick(DtrInteractionEvent ev)
        {
            if (ev.ClickType == MouseClickType.Right)
            {
                _dtrMenuPos = ev.Position;
                _openDtrMenu = true;
                return;
            }

            var rule = GetCurrentZoneRule();
            if (rule != null && rule.EnableMode != TerritoryEnableMode.Inherit)
            {
                rule.EnableMode = rule.EnableMode == TerritoryEnableMode.ForceOn
                    ? TerritoryEnableMode.ForceOff
                    : TerritoryEnableMode.ForceOn;
                Config.Save();
                ShowStatusToast(string.Format(
                    localizer.Localize("Zone forced {0}"),
                    rule.EnableMode == TerritoryEnableMode.ForceOn ? localizer.Localize("On") : localizer.Localize("Off")));
                UpdateDtrBar();
                return;
            }

            Svc.Commands.ProcessCommand("/xinvite toggle");
        }

        /// <summary>
        /// Renders the DTR right-click popup. DTR click callbacks aren't guaranteed
        /// to run inside the current ImGui frame, so OnDtrClick only sets a flag;
        /// the actual ImGui.OpenPopup/BeginPopup pair happens here, on the Draw tick.
        /// </summary>
        private void DrawDtrMenu()
        {
            if (_openDtrMenu)
            {
                ImGui.OpenPopup("##InviterDtrMenu");
                _openDtrMenu = false;
            }

            ImGui.SetNextWindowPos(_dtrMenuPos, ImGuiCond.Appearing);
            if (!ImGui.BeginPopup("##InviterDtrMenu"))
                return;

            if (ImGui.MenuItem(Config.Enable ? localizer.Localize("Turn Off") : localizer.Localize("Turn On")))
                Svc.Commands.ProcessCommand(Config.Enable ? "/xinvite off" : "/xinvite on");

            ImGui.Separator();

            foreach (var minutes in new[] { 15, 30, 60 })
            {
                if (ImGui.MenuItem(string.Format(localizer.Localize("Enable for {0} minutes"), minutes)))
                    Svc.Commands.ProcessCommand($"/xinvite {minutes}");
            }

            ImGui.Separator();
            ImGui.TextUnformatted(localizer.Localize("Custom:"));
            ImGui.SetNextItemWidth(70);
            ImGui.InputInt(localizer.Localize("min") + "##dtrCustomMinutes", ref _customMinutes, 0, 0);
            ImGui.SetNextItemWidth(70);
            ImGui.InputInt(localizer.Localize("attempts") + "##dtrCustomAttempts", ref _customAttempts, 0, 0);
            if (ImGui.Button(localizer.Localize("Start") + "##dtrCustomStart"))
                Svc.Commands.ProcessCommand($"/xinvite {Math.Max(0, _customMinutes)} {Math.Max(0, _customAttempts)}");

            if (timedRecruitment.isRunning)
            {
                ImGui.Separator();
                if (ImGui.MenuItem(localizer.Localize("Cancel timed recruitment")))
                    Svc.Commands.ProcessCommand("/xinvite 0");
            }

            ImGui.Separator();
            if (ImGui.MenuItem(localizer.Localize("Open Settings...")))
                ConfigurationWindow.Toggle();

            ImGui.EndPopup();
        }

        /// <summary>
        /// Refreshes the DTR bar's visibility and status text. Called explicitly at every
        /// point that could change the effective state - Config.Enable/ShowDtrEntry
        /// changes, timed-session start/tick/finish, zone rule edits, and zone changes -
        /// rather than on a per-frame poll, since the plugin has no other reason to hook
        /// Framework.Update.
        ///
        /// territoryIdOverride exists because OnTerritoryChanged fires with the new zone's ID
        /// before Svc.ClientState.TerritoryType necessarily reflects it - reading the stale
        /// property there could show the previous zone's status for a tick. Every other
        /// caller just wants "the zone I'm in right now" and can omit it.
        /// </summary>
        public void UpdateDtrBar(uint? territoryIdOverride = null)
        {
            if (DtrEntry == null)
                return;

            DtrEntry.Shown = Config.ShowDtrEntry;

            var territoryId = territoryIdOverride ?? Svc.ClientState.TerritoryType;
            var rule = Config.TerritoryRules.Find(r => r.TerritoryId == territoryId);
            var effectiveEnable = ResolveEffectiveEnable(Config.Enable, rule);

            string status;
            if (timedRecruitment.isRunning && effectiveEnable)
            {
                status = string.Format(localizer.Localize("Timed ({0}m)"), timedRecruitment.MinutesRemaining);
            }
            else if (rule == null || rule.EnableMode == TerritoryEnableMode.Inherit)
            {
                var onOff = Config.Enable ? localizer.Localize("On") : localizer.Localize("Off");
                status = rule == null ? onOff : string.Format(localizer.Localize("{0} (Inherited)"), onOff);
            }
            else
            {
                var onOff = rule.EnableMode == TerritoryEnableMode.ForceOn ? localizer.Localize("On") : localizer.Localize("Off");
                status = string.Format(localizer.Localize("{0} (Forced)"), onOff);
            }

            DtrEntry.Text = new SeString(new TextPayload($"Inviter: {status}"));
            DtrEntry.Tooltip = new SeString(new TextPayload(localizer.Localize("Left-click: toggle. Right-click: menu.")));
        }

        private unsafe void MsgHookDetour(RaptureLogModule* thisPtr, ulong contentId, ulong accountId, int messageIndex, ushort worldId, ushort chatType)
        {
            MsgHook.Original(thisPtr, contentId, accountId, messageIndex, worldId, chatType);

            var rule = GetCurrentZoneRule();

            // A rule's TextPattern/RegexMatch override only where actually set. Its on/off
            // mode only takes effect when explicitly ForceOn/ForceOff - the default Inherit
            // mode leaves the global Enable switch untouched, so adding a rule just for a
            // pattern override can't silently keep Inviter alive somewhere Enable is off.
            bool hasPatternOverride = rule != null && !string.IsNullOrEmpty(rule.TextPattern);
            bool effectiveEnable = ResolveEffectiveEnable(Config.Enable, rule);
            string effectiveTextPattern = ResolveEffectiveTextPattern(Config, rule);
            bool effectiveRegexMatch = hasPatternOverride ? rule!.RegexMatch : Config.RegexMatch;

            if (!effectiveEnable)
                return;
            if (Config.FilteredChannels.Contains((XivChatType)chatType))
                return;
            if (Config.HiddenChatType.Contains((XivChatType)chatType))
                return;
            if (Svc.Objects.LocalPlayer == null || Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
                return;
            if (string.IsNullOrWhiteSpace(effectiveTextPattern))
                return;

            if (!RaptureLogModule.Instance()->GetLogMessageDetail(messageIndex, out var sender, out var rawMessage, out _, out _, out _, out _))
            {
                Log("Skipping invite: unable to get message detail.");
                return;
            }

            var message = SeString.Parse(rawMessage.AsSpan()).TextValue;
            var matched = false;
            if (!effectiveRegexMatch)
            {
                matched = message.Contains(effectiveTextPattern, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                try
                {
                    // Explicit timeout: without one, a message crafted to trigger catastrophic
                    // backtracking against the user's own pattern can hang the match indefinitely,
                    // and that message can come from any other player on a watched channel.
                    matched = Regex.IsMatch(message, effectiveTextPattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
                }
                catch (RegexMatchTimeoutException)
                {
                    LogError("Skipping invite: pattern took too long to evaluate (possible catastrophic backtracking).");
                    return;
                }
                catch (Exception)
                {
                    LogError("Skipping invite: invalid regex pattern.");
                    return;
                }
            }

            if (matched)
            {
                if (GroupManager.Instance()->GetGroup()->MemberCount >= 8)
                {
                    Log("Skipping invite: party full.");
                    if (timedRecruitment.isRunning)
                        timedRecruitment.FinishTimer();
                    return;
                }

                if (GroupManager.Instance()->GetGroup()->MemberCount > 0 && !GroupManager.Instance()->MainGroup.IsEntityIdPartyLeader(Svc.Objects.LocalPlayer!.EntityId))
                {
                    Log("Skipping invite: not party leader.");
                    return;
                }

                if (Svc.Party.Any(p => p.ContentId == contentId))
                {
                    Log("Skipping invite: already in party.");
                    return;
                }

                if (SeString.Parse(sender.AsSpan()).Payloads.FirstOrDefault(p => p is PlayerPayload) is PlayerPayload playerPayload)
                {
                    var tc64 = Environment.TickCount64;
                    if (tc64 > NextInviteAt)
                    {
                        if (timedRecruitment.isRunning && timedRecruitment.MaxInvitations > 0)
                        {
                            if (timedRecruitment.InvitationAttempts >= timedRecruitment.MaxInvitations)
                            {
                                Log($"Reached target amound of invitations, won't invite {timedRecruitment.InvitationAttempts}/{timedRecruitment.MaxInvitations}");
                                timedRecruitment.FinishTimer();
                                return;
                            }
                            else
                            {
                                timedRecruitment.InvitationAttempts++;
                                Log($"Invitation {timedRecruitment.InvitationAttempts} out of {timedRecruitment.MaxInvitations}");
                            }
                        }
                        NextInviteAt = tc64 + Config.Ratelimit;
                        Log($"Attempting to invite {playerPayload.PlayerName}");

                        // InInvitableInstance() reads game state, so it needs to happen here on
                        // the framework thread (MsgHookDetour is called synchronously from the
                        // game's own AddMsgSourceEntry call, so this line is already safe).
                        var invitable = InInvitableInstance();
                        var playerName = playerPayload.PlayerName;
                        var worldRowId = playerPayload.World.RowId;

                        // Dispatched to a separate (non-unsafe) method: `await` isn't allowed inside
                        // a block lexically nested in an unsafe context, and MsgHookDetour is unsafe.
                        _ = SendInviteAsync(contentId, invitable, playerName, worldRowId);
                    }
                }
            }
        }

        /// <summary>
        /// Waits a randomized delay (between Config.DelayMin and Config.DelayMax) off the main
        /// thread, then hops onto the framework thread (the only thread it's safe to touch
        /// FFXIVClientStructs game memory from) to actually send the invite. Deliberately not
        /// `unsafe` at the method level so it's allowed to `await` — only the synchronous
        /// callback passed to RunOnFrameworkThread, which has no `await` in it, is marked unsafe.
        /// </summary>
        private async Task SendInviteAsync(ulong contentId, bool invitable, string playerName, uint worldRowId)
        {
            var lo = Math.Max(0, Math.Min(Config.DelayMin, Config.DelayMax));
            var hi = Math.Max(0, Math.Max(Config.DelayMin, Config.DelayMax));
            var delayMs = lo == hi ? lo : Random.Shared.Next(lo, hi + 1);
            await Task.Delay(delayMs);

            // Per Dalamud's docs this needs to be the last thing in the delegate: any code
            // after an `await` inside RunOnFrameworkThread() would run back on the thread
            // pool, not the framework thread.
            await Svc.Framework.RunOnFrameworkThread(() =>
            {
                unsafe
                {
                    if (invitable)
                    {
                        InfoProxyPartyInvite.Instance()->InviteToPartyInInstanceByContentId(contentId);
                    }
                    else
                    {
                        fixed (byte* namePtr = ToTerminatedBytes(playerName))
                            InfoProxyPartyInvite.Instance()->InviteToParty(contentId, namePtr, (ushort)worldRowId);
                    }
                }
            });
        }

        public unsafe void CommandHandler(string command, string arguments)
        {
            var args = arguments.Trim().Replace("\"", string.Empty);

            if (string.IsNullOrEmpty(args) || args.Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                ConfigurationWindow.Toggle();
                return;
            }
            else if (args == "on")
            {
                Config.Enable = true;
                ShowStatusToast(string.Format(localizer.Localize("Auto invite is turned on for \"{0}\""), ResolveEffectiveTextPattern(Config, GetCurrentZoneRule())));
                Config.Save();
                UpdateDtrBar();
            }
            else if (args == "off")
            {
                Config.Enable = false;
                ShowStatusToast(localizer.Localize("Auto invite is turned off"));
                Config.Save();
                UpdateDtrBar();
            }
            else if (args == "party")
            {
                Log($"MemberCount:{GroupManager.Instance()->MainGroup.MemberCount}");
                Log($"LeaderIndex:{GroupManager.Instance()->MainGroup.PartyLeaderIndex}");
                if (GroupManager.Instance()->MainGroup.MemberCount > 0)
                    Log($"LeaderName:{ConvertSpanToString(GroupManager.Instance()->MainGroup.GetPartyMemberByIndex((int)GroupManager.Instance()->MainGroup.PartyLeaderIndex)->Name)}");
                Log($"SelfName:{Svc.Objects.LocalPlayer?.Name}");
                Log($"isLeader:{GroupManager.Instance()->MainGroup.PartyLeaderIndex == 0}");
            }
            else if (args == "toggle")
            {
                Config.Enable = !Config.Enable;
                if (Config.Enable)
                {
                    ShowStatusToast(string.Format(localizer.Localize("Auto invite is turned on for \"{0}\""), ResolveEffectiveTextPattern(Config, GetCurrentZoneRule())));
                }
                else
                {
                    ShowStatusToast(localizer.Localize("Auto invite is turned off"));
                }
                Config.Save();
                UpdateDtrBar();
            }
            else if (timedRecruitment.TryProcessCommandTimedEnable(args))
            {
                //success
            }
            else if (Svc.Commands.Commands.TryGetValue("/xinvite", out var cmdInfo))
            {
                Svc.Chat.Print(cmdInfo.HelpMessage);
            }
        }

        public void Log(string message)
        {
            if (!Config.PrintMessage)
                return;
            var msg = $"[Inviter] {message}";
            Svc.Log.Info(msg);
            Svc.Chat.Print(msg);
        }

        public void LogError(string message)
        {
            if (!Config.PrintError)
                return;
            var msg = $"[Inviter] {message}";
            Svc.Log.Error(msg);
            Svc.Chat.PrintError(msg);
        }
        private unsafe bool InInvitableInstance() => Svc.Condition[ConditionFlag.BoundByDuty56] && Svc.Data.GetExcelSheet<TerritoryType>().GetRowOrDefault(GameMain.Instance()->CurrentTerritoryTypeId)?.TerritoryIntendedUse.RowId is 41 or 47 or 48 or 52 or 53 or 61;

        public static string ConvertSpanToString(Span<byte> byteSpan)
        {
            int length = 0;
            for (int i = 0; i < byteSpan.Length; i++)
            {
                if (byteSpan[i] == 0)
                {
                    break;
                }
                length++;
            }
            return Encoding.UTF8.GetString(byteSpan[..length]);
        }

        private static byte[] ToTerminatedBytes(string s)
        {
            var utf8 = Encoding.UTF8;
            var bytes = new byte[utf8.GetByteCount(s) + 1];
            utf8.GetBytes(s, 0, s.Length, bytes, 0);
            bytes[^1] = 0;
            return bytes;
        }
    }
}
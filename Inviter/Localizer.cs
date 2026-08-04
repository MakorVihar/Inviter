using System.Collections.Generic;

namespace Inviter
{
    public class Localizer
    {
        public string Language = "en";
        private readonly Dictionary<string, string> zh = [];
        private readonly Dictionary<string, string> fr = [];
        public Localizer(string language = "en")
        {
            Language = language;
            LoadZh();
            LoadFr();
        }
        public string Localize(string message)
        {
            if (Language == "zh")
                return zh.TryGetValue(message, out string? value) ? value : message;
            else if (Language == "fr")
                return fr.TryGetValue(message, out string? value) ? value : message;
            return message;
        }
        private void LoadZh()
        {
            zh.Add("Panel", "面板");
            zh.Add("TabBar", "标签栏");
            zh.Add("Settings", "设置");
            zh.Add("General Settings", "通用设置");
            zh.Add("Enable", "启用");
            zh.Add("Tooltips", "选项说明");
            zh.Add("Change the UI Language.", "更改UI语言");
            zh.Add("Eureka", "优雷卡");
            zh.Add("Delay(ms):", "延时(毫秒)：");
            zh.Add("Delay the invitation after triggered.", "触发后延迟邀请。");
            zh.Add("Language:", "语言：");
            zh.Add("Print Debug Message", "打印调试消息");
            zh.Add("Print Error Message", "打印错误消息");
            zh.Add("Filters", "过滤设置");
            zh.Add("Regex", "正则表达式");
            zh.Add("Filter out duplicates", "过滤重复消息");
            zh.Add("FiltersTable", "过滤表格");
            zh.Add("Debug", "调试");
            zh.Add("Urgent", "紧急");
            zh.Add("Notice", "通知");
            zh.Add("Say", "说话");
            zh.Add("Shout", "喊话");
            zh.Add("TellOutgoing", "发出悄悄话");
            zh.Add("TellIncoming", "收到悄悄话");
            zh.Add("Party", "小队");
            zh.Add("Alliance", "团队");
            zh.Add("FreeCompany", "部队");
            zh.Add("Ls1", "通讯贝1");
            zh.Add("Ls2", "通讯贝2");
            zh.Add("Ls3", "通讯贝3");
            zh.Add("Ls4", "通讯贝4");
            zh.Add("Ls5", "通讯贝5");
            zh.Add("Ls6", "通讯贝6");
            zh.Add("Ls7", "通讯贝7");
            zh.Add("Ls8", "通讯贝8");
            zh.Add("Yell", "呼喊");
            zh.Add("CrossParty", "跨服小队");
            zh.Add("PvPTeam", "PvP小队");
            zh.Add("NoviceNetwork", "新人频道");
            zh.Add("CrossLinkShell1", "跨服通讯贝1");
            zh.Add("CrossLinkShell2", "跨服通讯贝2");
            zh.Add("CrossLinkShell3", "跨服通讯贝3");
            zh.Add("CrossLinkShell4", "跨服通讯贝4");
            zh.Add("CrossLinkShell5", "跨服通讯贝5");
            zh.Add("CrossLinkShell6", "跨服通讯贝6");
            zh.Add("CrossLinkShell7", "跨服通讯贝7");
            zh.Add("CrossLinkShell8", "跨服通讯贝8");
            zh.Add("Echo", "默语");
            zh.Add("Automatically invite people to your party (doesn't work for CWLS).", "自动邀请人们参与组队（不适用于跨服通讯贝）。");
            zh.Add("Pattern of the chat message to trigger the invitation.", "触发邀请的聊天消息的模式串。");
            zh.Add("Use regex to match the pattern to chat messages.", "使用正则表达式匹配聊天消息。");
            zh.Add("Pattern:", "模式：");
            zh.Add("Sender", "发送者");
            zh.Add("Message", "消息");
            zh.Add("Time", "时间");
            zh.Add("Retrieve", "检索");
            zh.Add("Teleport", "传送");
            zh.Add("Delete", "删除");
            zh.Add("View", "查看");
            zh.Add("Tele", "传送");
            zh.Add("Del", "删除");
            zh.Add("Clear", "清空");
            zh.Add("Auto invite is turned off", "自动邀请已关闭");
            zh.Add("Auto invite is turned on for \"{0}\"", "已启用\"{0}\"的自动邀请");
            zh.Add("Automatic recruitment enabled, {0} minutes left", "自动邀请已启用，剩余{0}分钟");
            zh.Add("Automatic recruitment finished", "自动邀请已结束");
            zh.Add("Can't start timed recruitment because Inviter is turned on permanently", "Inviter 启用时无法开始限时自动邀请");
            zh.Add("Commenced automatic recruitment for {0} minutes", "开始了时长为{0}分钟的自动邀请");
            zh.Add("Recruitment is not running, can not cancel", "限时自动邀请未运行，无法取消");
            zh.Add("Time can not be negative", "时间不能为负数");
            zh.Add("Please enter amount of time in minutes", "请输入分钟数作为时长");
            zh.Add("Recruitment will finish after {0} invitation attempts", "自动邀请将在完成{0}次邀请后关闭");

            // General Settings additions
            zh.Add("Inviter Enabled", "Inviter 已启用");
            zh.Add("Inviter Disabled", "Inviter 已禁用");
            zh.Add("Delay the invitation after triggered, randomized between these two values each time.", "触发后延迟邀请，每次在这两个数值之间随机取值。");
            zh.Add("to", "至");
            zh.Add("Rate limit (ms):", "速率限制（毫秒）：");
            zh.Add("How much time must pass between invitations.", "两次邀请之间必须间隔的时间。");
            zh.Add("Show an Inviter status entry in the server info bar. Left-click toggles it, right-click opens a quick menu.", "在服务器信息栏中显示 Inviter 状态。左键点击切换开关，右键点击打开快捷菜单。");
            zh.Add("Server Info Bar Enabled", "服务器信息栏已启用");
            zh.Add("Server Info Bar Disabled", "服务器信息栏已禁用");
            zh.Add("Display as Toasts:", "显示为提示消息：");
            zh.Add("Show status changes as a pop-up toast, using the styles below. More than one can be enabled at once.", "以弹出提示的形式显示状态变化，使用下方的样式。可以同时启用多种样式。");
            zh.Add("Quest", "任务");
            zh.Add("Normal", "普通");
            zh.Add("Error", "错误");

            // Tab bar / Channels tab
            zh.Add("Features", "功能");
            zh.Add("Channels", "频道");
            zh.Add("User Restriction", "使用限制");
            zh.Add("All", "全选");
            zh.Add("Check every channel.", "勾选所有频道。");
            zh.Add("Uncheck every channel.", "取消勾选所有频道。");

            // User Restriction: Everywhere / zone rules
            zh.Add("Everywhere...", "所有地方……");
            zh.Add("...but for these Maps", "……但这些地图除外");
            zh.Add("The default pattern used everywhere, unless a zone under \"...but for these Maps\" overrides it.", "在所有地方使用的默认模式，除非在“……但这些地图除外”中为该区域设置了覆盖。");
            zh.Add("Pattern is empty — Unless overridden for a zone, inviter will not match any messages until you enter one.", "模式为空 — 在您输入内容之前，Inviter 不会匹配任何消息（除非某个地图设置了独立覆盖）。");
            zh.Add(
                "Zones listed here override \"Everywhere...\". Inherit only overrides the Pattern (and whether it's " +
                "regex) if you check its box below; Force On/Off override the main Enable switch for that zone specifically.",
                "此处列出的区域将覆盖“所有地方……”的设置。“继承”仅在您勾选下方的复选框时才会覆盖模式（以及是否为正则表达式）；“强制开启/关闭”则会专门针对该区域覆盖主开关。");
            zh.Add("(No zone rules added.)", "（尚未添加区域规则。）");
            zh.Add("Mode", "模式");
            zh.Add("Zone", "区域");
            zh.Add("Pattern override", "模式覆盖");
            zh.Add("Only applies when this zone has a pattern override.", "仅在此区域设置了模式覆盖时生效。");
            zh.Add("Remove", "移除");
            zh.Add("Inherit", "继承");
            zh.Add("Force On", "强制开启");
            zh.Add("Force Off", "强制关闭");
            zh.Add("Add Current Zone", "添加当前区域");
            zh.Add("Search zone name...", "搜索区域名称……");

            // DTR bar status text and quick menu
            zh.Add("On", "开启");
            zh.Add("Off", "关闭");
            zh.Add("{0} (Inherited)", "{0} (继承)");
            zh.Add("{0} (Forced)", "{0} (强制)");
            zh.Add("Timed ({0}m)", "限时（{0}分钟）");
            zh.Add("Left-click: toggle. Right-click: menu.", "左键点击：切换。右键点击：菜单。");
            zh.Add("Turn On", "开启");
            zh.Add("Turn Off", "关闭");
            zh.Add("Open Settings...", "打开设置……");
            zh.Add("Cancel timed recruitment", "取消限时自动邀请");
            zh.Add("Zone forced {0}", "区域已强制{0}");
            zh.Add("Custom:", "自定义：");
            zh.Add("min", "分钟");
            zh.Add("attempts", "次尝试");
            zh.Add("Start", "开始");
            zh.Add("Enable for {0} minutes", "启用{0}分钟");

            // Timed recruitment toasts (matching the current exact wording used in code)
            zh.Add("Automatic recruitment canceled", "限时自动邀请已取消");
            zh.Add("Recruitment is not running, cannot cancel", "限时自动邀请未运行，无法取消");
            zh.Add("Invalid time format. Please enter minutes as a number", "时间格式无效，请输入数字形式的分钟数");
            zh.Add("Recruitment finished: Invitation limit reached", "招募已结束：已达到邀请数量上限");
        }
        private void LoadFr()
        {
            fr.Add("Panel", "Config");
            fr.Add("TabBar", "TabBar");
            fr.Add("Settings", "Paramètres");
            fr.Add("General Settings", "Paramètres Généraux");
            fr.Add("Enable", "Activer");
            fr.Add("Tooltips", "Infobulle");
            fr.Add("Change the UI Language.", "Changer la langue de l'interface.");
            fr.Add("Eureka", "Eureka");
            fr.Add("Delay(ms):", "Délais (ms):");
            fr.Add("Delay the invitation after triggered.", "Delai de l'invitation après déclenchement.。");
            fr.Add("Language:", "Langage:");
            fr.Add("Print Debug Message", "Imprimer les messages de débuggage.");
            fr.Add("Print Error Message", "Imprimer les messages d'erreur.");
            fr.Add("Filters", "Filtres");
            fr.Add("Regex", "Regex");
            fr.Add("Filter out duplicates", "Filtrer les doublons");
            fr.Add("FiltersTable", "FiltersTable");
            fr.Add("Debug", "Débug");
            fr.Add("Urgent", "Urgent");
            fr.Add("Notice", "Avis");
            fr.Add("Say", "Dire");
            fr.Add("Shout", "Hurler");
            fr.Add("TellOutgoing", "Murmure Sortant");
            fr.Add("TellIncoming", "Murmure Entrant");
            fr.Add("Party", "Équipe");
            fr.Add("Alliance", "Alliance");
            fr.Add("FreeCompany", "Compagnie Libre");
            fr.Add("Ls1", "Ls1");
            fr.Add("Ls2", "Ls2");
            fr.Add("Ls3", "Ls3");
            fr.Add("Ls4", "Ls4");
            fr.Add("Ls5", "Ls5");
            fr.Add("Ls6", "Ls6");
            fr.Add("Ls7", "Ls7");
            fr.Add("Ls8", "Ls8");
            fr.Add("Yell", "Crier");
            fr.Add("CrossParty", "Inter-Monde");
            fr.Add("PvPTeam", "Équipe JcJ");
            fr.Add("NoviceNetwork", "Réseau des Novices");
            fr.Add("CrossLinkShell1", "Linkshell Inter-Monde1");
            fr.Add("CrossLinkShell2", "Linkshell Inter-Monde2");
            fr.Add("CrossLinkShell3", "Linkshell Inter-Monde3");
            fr.Add("CrossLinkShell4", "Linkshell Inter-Monde4");
            fr.Add("CrossLinkShell5", "Linkshell Inter-Monde5");
            fr.Add("CrossLinkShell6", "Linkshell Inter-Monde6");
            fr.Add("CrossLinkShell7", "Linkshell Inter-Monde7");
            fr.Add("CrossLinkShell8", "Linkshell Inter-Monde8");
            fr.Add("Echo", "Écho");
            fr.Add("Automatically invite people to your party (doesn't work for CWLS).", "Inviter automatiquement les personnes dans votre groupe (ne fonctionne pas pour les LS inter-mondes.");
            fr.Add("Pattern of the chat message to trigger the invitation.", "Pattern du message qui déclenche l'invitation.");
            fr.Add("Use regex to match the pattern to chat messages.", "Utiliser une regex pour identifier les messages.");
            fr.Add("Pattern:", "Pattern:");
            fr.Add("Sender", "Envoyeur");
            fr.Add("Message", "Message");
            fr.Add("Time", "Heure");
            fr.Add("Retrieve", "Récupérer");
            fr.Add("Teleport", "Téléporter");
            fr.Add("Delete", "Supprimer");
            fr.Add("View", "Inspected");
            fr.Add("Tele", "Télé");
            fr.Add("Del", "Del");
            fr.Add("Clear", "Nettoyer");

            // Timed recruitment toasts (previously missing from fr entirely)
            fr.Add("Auto invite is turned off", "L'invitation automatique est désactivée");
            fr.Add("Auto invite is turned on for \"{0}\"", "L'invitation automatique est activée pour « {0} »");
            fr.Add("Automatic recruitment enabled, {0} minutes left", "Recrutement automatique activé, {0} minutes restantes");
            fr.Add("Automatic recruitment finished", "Recrutement automatique terminé");
            fr.Add("Commenced automatic recruitment for {0} minutes", "Recrutement automatique commencé pour {0} minutes");
            fr.Add("Recruitment will finish after {0} invitation attempts", "Le recrutement se terminera après {0} tentatives d'invitation");
            fr.Add("Automatic recruitment canceled", "Recrutement automatique annulé");
            fr.Add("Recruitment is not running, cannot cancel", "Le recrutement n'est pas en cours, impossible d'annuler");
            fr.Add("Invalid time format. Please enter minutes as a number", "Format de temps invalide. Veuillez entrer les minutes sous forme de nombre");
            fr.Add("Recruitment finished: Invitation limit reached", "Recrutement terminé : limite d'invitations atteinte");

            // General Settings additions
            fr.Add("Inviter Enabled", "Inviter activé");
            fr.Add("Inviter Disabled", "Inviter désactivé");
            fr.Add("Delay the invitation after triggered, randomized between these two values each time.", "Délai avant l'invitation après déclenchement, tiré aléatoirement entre ces deux valeurs à chaque fois.");
            fr.Add("to", "à");
            fr.Add("Rate limit (ms):", "Limite de fréquence (ms) :");
            fr.Add("How much time must pass between invitations.", "Le temps devant s'écouler entre deux invitations.");
            fr.Add("Show an Inviter status entry in the server info bar. Left-click toggles it, right-click opens a quick menu.", "Afficher l'état d'Inviter dans la barre d'informations. Clic gauche pour basculer, clic droit pour ouvrir un menu rapide.");
            fr.Add("Server Info Bar Enabled", "Barre d'informations activée");
            fr.Add("Server Info Bar Disabled", "Barre d'informations désactivée");
            fr.Add("Display as Toasts:", "Afficher en tant que notifications :");
            fr.Add("Show status changes as a pop-up toast, using the styles below. More than one can be enabled at once.", "Afficher les changements d'état sous forme de notification, avec les styles ci-dessous. Plusieurs styles peuvent être activés en même temps.");
            fr.Add("Quest", "Quête");
            fr.Add("Normal", "Normal");
            fr.Add("Error", "Erreur");

            // Tab bar / Channels tab
            fr.Add("Features", "Fonctionnalités");
            fr.Add("Channels", "Canaux");
            fr.Add("User Restriction", "Restriction d'utilisation");
            fr.Add("All", "Tout");
            fr.Add("Check every channel.", "Cocher tous les canaux.");
            fr.Add("Uncheck every channel.", "Décocher tous les canaux.");

            // User Restriction: Everywhere / zone rules
            fr.Add("Everywhere...", "Partout...");
            fr.Add("...but for these Maps", "...mais pour ces zones");
            fr.Add("The default pattern used everywhere, unless a zone under \"...but for these Maps\" overrides it.", "Le pattern par défaut utilisé partout, sauf si une zone sous « ...mais pour ces zones » le remplace.");
            fr.Add("Pattern is empty — Unless overridden for a zone, inviter will not match any messages until you enter one.", "Le pattern est vide — Sauf s'il est remplacé pour une zone, Inviter ne correspondra à aucun message tant que vous n'en aurez pas saisi un.");
            fr.Add(
                "Zones listed here override \"Everywhere...\". Inherit only overrides the Pattern (and whether it's " +
                "regex) if you check its box below; Force On/Off override the main Enable switch for that zone specifically.",
                "Les zones listées ici remplacent « Partout... ». Hériter ne remplace le pattern (et le fait qu'il soit une regex) que si vous cochez sa case ci-dessous ; Forcer activé/désactivé remplace l'interrupteur principal spécifiquement pour cette zone.");
            fr.Add("(No zone rules added.)", "(Aucune règle de zone ajoutée.)");
            fr.Add("Mode", "Mode");
            fr.Add("Zone", "Zone");
            fr.Add("Pattern override", "Substitution de pattern");
            fr.Add("Only applies when this zone has a pattern override.", "S'applique uniquement si cette zone possède une substitution de pattern.");
            fr.Add("Remove", "Supprimer");
            fr.Add("Inherit", "Hériter");
            fr.Add("Force On", "Forcer activé");
            fr.Add("Force Off", "Forcer désactivé");
            fr.Add("Add Current Zone", "Ajouter la zone actuelle");
            fr.Add("Search zone name...", "Rechercher une zone...");

            // DTR bar status text and quick menu
            fr.Add("On", "Activé");
            fr.Add("Off", "Désactivé");
            fr.Add("{0} (Inherited)", "{0} (Hérité)");
            fr.Add("{0} (Forced)", "{0} (Forcé)");
            fr.Add("Timed ({0}m)", "Minuté ({0}m)");
            fr.Add("Left-click: toggle. Right-click: menu.", "Clic gauche : basculer. Clic droit : menu.");
            fr.Add("Turn On", "Activer");
            fr.Add("Turn Off", "Désactiver");
            fr.Add("Open Settings...", "Ouvrir les paramètres...");
            fr.Add("Cancel timed recruitment", "Annuler le recrutement minuté");
            fr.Add("Zone forced {0}", "Zone forcée {0}");
            fr.Add("Custom:", "Personnalisé :");
            fr.Add("min", "min");
            fr.Add("attempts", "tentatives");
            fr.Add("Start", "Démarrer");
            fr.Add("Enable for {0} minutes", "Activer pendant {0} minutes");
        }
    }
}

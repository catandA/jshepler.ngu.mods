using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods.WebService.Triggers
{
    [HarmonyPatch]
    internal class TriggerConfig
    {
        private static Button _settingsButton;

        internal static bool RemoteTriggersEnabled
        {
            get => Options.RemoteTriggers.Enabled.Value;
            set => Options.RemoteTriggers.Enabled.Value = value;
        }

        internal static bool AutoBoostEnabled
        {
            get => Options.RemoteTriggers.AutoBoost.Enabled.Value;
            set => Options.RemoteTriggers.AutoBoost.Enabled.Value = value;
        }

        internal static bool AutoMergeEnabled
        {
            get => Options.RemoteTriggers.AutoMerge.Enabled.Value;
            set => Options.RemoteTriggers.AutoMerge.Enabled.Value = value;
        }

        internal static bool TossGoldEnabled
        {
            get => Options.RemoteTriggers.TossGold.Enabled.Value;
            set => Options.RemoteTriggers.TossGold.Enabled.Value = value;
        }

        internal static bool FightBossEnabled
        {
            get => Options.RemoteTriggers.FightBoss.Enabled.Value;
            set => Options.RemoteTriggers.FightBoss.Enabled.Value = value;
        }

        internal static bool KittyEnabled
        {
            get => Options.RemoteTriggers.Kitty.Enabled.Value;
            set => Options.RemoteTriggers.Kitty.Enabled.Value = value;
        }

        internal static bool TwitchIntegrationEnabled
        {
            get => Options.Twitch.Enabled.Value;
            set => Options.Twitch.Enabled.Value = value;
        }

        internal static bool TwitchAutoConnect
        {
            get => Options.Twitch.AutoConnect.Value;
            set => Options.Twitch.AutoConnect.Value = value;
        }

        [HarmonyPrepare, HarmonyPatch]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnGameStart += (o, e) =>
            {
                _settingsButton = GameObject.Find("Canvas/Player Panel Canvas/Player Panel/Settings").GetComponent<Button>();
                _settingsButton.gameObject.AddComponent<ClickHandlerComponent>()
                    .OnRightClick(e => OpenConfig());

                InitPopup();
            };

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.GameHasStarted)
                    return;

                if (Input.GetKeyDown(KeyCode.F10))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        OpenConfig();
                    else
                        ToggleRemoteTriggersEnabled();
                }
            };

            Plugin.onGUI += (o, e) =>
            {
                _settingsButton.image.color =
                    !RemoteTriggersEnabled ? Color.white
                    : !TwitchIntegrationEnabled || Twitch.Manager.IsConnected ? Plugin.ButtonColor_Green
                    : Plugin.ButtonColor_Red;
            };
        }

        private static void ToggleRemoteTriggersEnabled()
        {
            RemoteTriggersEnabled = !RemoteTriggersEnabled;
            _settingsButton.image.color = RemoteTriggersEnabled ? Plugin.ButtonColor_Green : Color.white;
        }

        const string ROOTCANVAS = "Canvas";
        private static GameObject _blocker;
        private static Rect _windowRect;
        private static bool _isOpen = false;

        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _selected;
        private static GUIStyle _notSelected;

        private static void InitPopup()
        {
            _windowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 114, 400, 340);
            _blocker = buildBLocker();
            IsOpen = false;

            Plugin.onGUI += (o, e) =>
            {
                if (!IsOpen)
                    return;

                if (Input.GetKeyDown(KeyCode.Escape))
                    CloseConfig();
                else
                    DrawWindow();
            };
        }

        private static bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                _blocker.SetActive(value);
            }
        }

        private static void OpenConfig()
        {
            IsOpen = true;
        }

        private static void CloseConfig()
        {
            IsOpen = false;
        }

        private static void DrawWindow()
        {
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle("box");
                _windowStyle.normal.background = new Color32(50, 50, 50, 255).CreateSolidColorTexture(_windowRect);

                _titleStyle = new GUIStyle("label");
                _titleStyle.alignment = TextAnchor.MiddleCenter;

                _notSelected = new GUIStyle("button");
                _notSelected.normal.background = Texture2D.blackTexture;

                _selected = new GUIStyle("button");
                _selected.normal.background = Texture2D.whiteTexture;
                _selected.normal.textColor = Color.black;
                _selected.hover.background = Texture2D.whiteTexture;
                _selected.hover.textColor = Color.black;
            }

            GUILayout.BeginArea(_windowRect, _windowStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>配置远程触发器</b>", _titleStyle);
            if (GUILayout.Button("×", GUILayout.Width(25))) CloseConfig();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("接受触发请求");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !RemoteTriggersEnabled ? _selected : _notSelected)) RemoteTriggersEnabled = false;
            if (GUILayout.Button("启用", RemoteTriggersEnabled ? _selected : _notSelected)) RemoteTriggersEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("自动加速");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !AutoBoostEnabled ? _selected : _notSelected)) AutoBoostEnabled = false;
            if (GUILayout.Button("启用", AutoBoostEnabled ? _selected : _notSelected)) AutoBoostEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("自动合并");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !AutoMergeEnabled ? _selected : _notSelected)) AutoMergeEnabled = false;
            if (GUILayout.Button("启用", AutoMergeEnabled ? _selected : _notSelected)) AutoMergeEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("投掷金币");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !TossGoldEnabled ? _selected : _notSelected)) TossGoldEnabled = false;
            if (GUILayout.Button("启用", TossGoldEnabled ? _selected : _notSelected)) TossGoldEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("对决首领");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !FightBossEnabled ? _selected : _notSelected)) FightBossEnabled = false;
            if (GUILayout.Button("启用", FightBossEnabled ? _selected : _notSelected)) FightBossEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("猫咪");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !KittyEnabled ? _selected : _notSelected)) KittyEnabled = false;
            if (GUILayout.Button("启用", KittyEnabled ? _selected : _notSelected)) KittyEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Twitch 集成");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !TwitchIntegrationEnabled ? _selected : _notSelected)) TwitchIntegrationEnabled = false;
            if (GUILayout.Button("启用", TwitchIntegrationEnabled ? _selected : _notSelected)) TwitchIntegrationEnabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("自动连接");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("禁用", !TwitchAutoConnect ? _selected : _notSelected)) TwitchAutoConnect = false;
            if (GUILayout.Button("启用", TwitchAutoConnect ? _selected : _notSelected)) TwitchAutoConnect = true;
            GUILayout.EndHorizontal();

            if (TwitchIntegrationEnabled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{(Twitch.Manager.IsConnecting ? "连接中..." : Twitch.Manager.IsConnected ? "已连接" : "未连接")}");

                if (!Twitch.Manager.IsConnecting)
                    if (GUILayout.Button($"{(Twitch.Manager.IsConnected ? "断开" : "连接")}"))
                    {
                        if (Twitch.Manager.IsConnected)
                            Twitch.Manager.Disconnect();
                        else
                            Twitch.Manager.Connect();
                    };

                if (GUILayout.Button("重置")) Twitch.Manager.Reset();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // modeled after https://github.com/AppertaFoundation/IXN_IBM_MIND/blob/8aa21d78bfe90e6feeab3cce6256e7fbb6036734/Whack-A-Mole/Library/PackageCache/com.unity.textmeshpro%402.0.0/Scripts/Runtime/TMP_Dropdown.cs#L847
        private static GameObject buildBLocker()
        {
            var blocker = new GameObject("Blocker");

            var blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(GameObject.Find(ROOTCANVAS).transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            blocker.AddComponent<GraphicRaycaster>();

            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;
            //blockerImage.color = new Color(0, 0, 0, .5f);

            var blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(() =>
            {
                if (!_windowRect.Contains(Event.current.mousePosition))
                    CloseConfig();
            });

            return blocker;
        }
    }
}

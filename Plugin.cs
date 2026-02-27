using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MossLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomTP;

[BepInPlugin("blackmoss.randomtp", "Random TP", "1.1.0")]
[BepInDependency("blackmoss.mosslib")]
public class Plugin : BaseUnityPlugin
{
    internal const string Guid = "blackmoss.randomtp";
    internal const string Name = "Random TP";

    private static ManualLogSource _logger;
    private static readonly Harmony Harmony = new(Guid);

    public static ConfigEntry<float> ConfigTpCountdown;
    public static ConfigEntry<bool> ConfigTipStyle;
    public static ConfigEntry<int> ConfigRandomTpXMin;
    public static ConfigEntry<int> ConfigRandomTpXMax;
    public static ConfigEntry<int> ConfigRandomTpYMin;
    public static ConfigEntry<int> ConfigRandomTpYMax;

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private static Plugin Instance { get; set; } = null!;

    private void Awake()
    {
        _logger = Logger;
        Instance = this;
        Harmony.PatchAll();

        ModLocale.Initialize(_logger);

        // 倒计时冷却（默认60秒）
        ConfigTpCountdown = Config.Bind(
            "General",
            "TpCountdown",
            60f
        );
        // 倒计时提示样式配置（true:屏幕中央, false:屏幕底部）
        ConfigTipStyle = Config.Bind(
            "General",
            "TpTipStyle",
            false,
            "true: Center of the screen, false: Bottom of the screen"
        );

        {
            const string c = "General";
            const string k = "RandomTp";
            ConfigRandomTpXMin = Config.Bind(
                c,
                k + "XMin",
                -511
            );
            ConfigRandomTpXMax = Config.Bind(
                c,
                k + "XMax",
                511
            );

            ConfigRandomTpYMin = Config.Bind(
                c,
                k + "YMin",
                -511
            );
            ConfigRandomTpYMax = Config.Bind(
                c,
                k + "YMax",
                511
            );
        }
    }

    // 随机传送
    private static void Tp()
    {
        var vector = new Vector2(
            Random.Range(ModConfigs.RandomTpXMin, ModConfigs.RandomTpXMax),
            Random.Range(ModConfigs.RandomTpYMin, ModConfigs.RandomTpYMax));

        PlayerCamera.main.body.transform.position = vector;
        PlayerCamera.main.transform.position = vector;
    }

    [HarmonyPatch(typeof(Body), "Update")]
    public class BodyPatch
    {
        private static float _tpTimer;
        private static float _lastAlertTime;

        // ReSharper disable once UnusedMember.Global
        public static void RandomTp()
        {
            _tpTimer += Time.deltaTime;

            var remainingTime = ModConfigs.TpCountdown - _tpTimer;
            var remainingTimeInt = Mathf.CeilToInt(remainingTime);

            // 检查是否需要显示倒计时提醒
            if (ShouldShowAlert(remainingTimeInt) && !Mathf.Approximately(_lastAlertTime, remainingTimeInt))
            {
                Tools.Alert(ModLocale.GetFormat("alert.countdown", remainingTimeInt), ModConfigs.TipStyle);
                _lastAlertTime = remainingTimeInt;
            }

            // 倒计时结束
            if (!(_tpTimer >= ModConfigs.TpCountdown)) return;
            _logger.LogInfo($"TP triggered at {_tpTimer}s");
            Tp();
            _tpTimer = 0f;
            _lastAlertTime = 0f;
        }

        // 判断是否需要显示倒计时提醒
        private static bool ShouldShowAlert(int time)
        {
            return time is 1 or 2 or 3 or 10 or 30 or 60;
        }
    }
}
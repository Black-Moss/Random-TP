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
    public static ConfigEntry<bool>  ConfigTipStyle;
    public static ConfigEntry<int>   ConfigRandomTpXMin;
    public static ConfigEntry<int>   ConfigRandomTpXMax;
    public static ConfigEntry<int>   ConfigRandomTpYMin;
    public static ConfigEntry<int>   ConfigRandomTpYMax;

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private static Plugin Instance { get; set; } = null!;

    private void Awake()
    {
        _logger = base.Logger;
        Instance = this;
        ModLocale.Initialize(_logger);
        ModCommand.Initialize(_logger);
        Harmony.PatchAll();
        
        ConfigTpCountdown = Config.Bind(
            "General",
            "TpCountdown",
            60f
        );
        ConfigTipStyle = Config.Bind(
            "General",
            "TpTipStyle",
            false,
            ModLocale.GetFormat("config.TpTipStyle")
        );

        
        ConfigRandomTpXMin = Config.Bind(
            "General",
            "RandomTpXMin", 
            -511
            );
        ConfigRandomTpXMax = Config.Bind(
            "General",
            "RandomTpXMax",
            511 
            );
        ConfigRandomTpYMin = Config.Bind(
            "General",
            "RandomTpYMin",
            -511 
            );
        ConfigRandomTpYMax = Config.Bind(
            "General",
            "RandomTpYMax",
            511
            );
    }

    // 随机传送
    public static void Tp()
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
            
        [HarmonyPostfix]
        // ReSharper disable once UnusedMember.Global
        public static void RandomTp()
        {
            _tpTimer += Time.deltaTime;
                
            float remainingTime = ConfigTpCountdown.Value - _tpTimer;
            int remainingTimeInt = Mathf.CeilToInt(remainingTime);
                
            // 检查是否需要显示倒计时提醒
            if (ShouldShowAlert(remainingTimeInt) && !Mathf.Approximately(_lastAlertTime, remainingTimeInt))
            {
                Tools.Alert(ModLocale.GetFormat("alert.countdown", remainingTimeInt), ConfigTipStyle.Value);
                _lastAlertTime = remainingTimeInt;
            }
                
            if (_tpTimer >= ConfigTpCountdown.Value)
            {
                _logger.LogInfo(ModLocale.GetFormat("log.tp_triggered", _tpTimer));
                Tp();
                Reset();
            }
        }
            
        private static bool ShouldShowAlert(int time)
        {
            return time is 1 or 2 or 3 or 10 or 30 or 60;
        }
        
        public static void Reset()
        {
            _tpTimer = 0f;
            _lastAlertTime = 0f;
        }
    }
}
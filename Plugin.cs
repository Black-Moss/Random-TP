using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomTP
{
    [BepInPlugin("blackmoss.randomtp", "Random TP", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public new static ManualLogSource Logger;
        private readonly Harmony _harmony = new("blackmoss.randomtp");
        private static Plugin Instance { get; set; } = null!;
        
        // ReSharper disable once InconsistentNaming
        private static ConfigEntry<float> ConfigTpCountdown;    // 倒计时配置
        // ReSharper disable once InconsistentNaming
        private static ConfigEntry<bool> ConfigTipStyle;        // 提示样式配置

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            _harmony.PatchAll();
            
            ConfigTpCountdown = Config.Bind(
                "General",
                "TpCountdown",
                60f     // 默认倒计时60秒
            );
            ConfigTipStyle = Config.Bind(
                "General",
                "TpTipStyle",
                false,   // true: 居中, false: 底端
                "true: Center of the screen, false: Bottom of the screen"
            );
        }
        
        [HarmonyPatch(typeof(Body), "Update")]
        public class BodyPatch
        {
            private static float _tpTimer = 0f;
            private static float _lastAlertTime = 0f; // 记录上次提醒的时间
            
            [HarmonyPostfix]
            public static void Postfix_Body_Update()
            {
                _tpTimer += Time.deltaTime;
                
                // 计算剩余时间
                float remainingTime = ConfigTpCountdown.Value - _tpTimer;
                int remainingTimeInt = Mathf.CeilToInt(remainingTime); // 向上取整
                
                // 检查是否需要显示倒计时提醒
                if (ShouldShowAlert(remainingTimeInt) && !Mathf.Approximately(_lastAlertTime, remainingTimeInt))
                {
                    AlertText($"Random TP countdown: {remainingTimeInt}", ConfigTipStyle.Value);
                    _lastAlertTime = remainingTimeInt;
                }
                
                // 当计时器达到设定值时执行TP
                if (_tpTimer >= ConfigTpCountdown.Value)
                {
                    Logger.LogInfo($"TP triggered at {_tpTimer}s");
                    Tp();
                    _tpTimer = 0f; // 重置计时器
                    _lastAlertTime = 0f; // 重置提醒时间
                }
            }
            
            private static bool ShouldShowAlert(int time)
            {
                return time is 1 or 2 or 3 or 10 or 30 or 60;
            }
        }
        
        private static void Tp()
        {
            // 生成随机坐标
            var vector = new Vector2(Random.Range(-512, 513), Random.Range(-512, 513));
            
            PlayerCamera.main.body.transform.position = vector;
            PlayerCamera.main.transform.position = vector;
        }
        
        private static void AlertText(string text, bool important)
        {
            PlayerCamera.main.DoAlert(text, important);
        }
    }
}
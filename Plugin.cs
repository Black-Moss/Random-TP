using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomTP
{
    [BepInPlugin("blackmoss.randomtp", "Random TP", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // ReSharper disable once MemberCanBePrivate.Global
        internal new static ManualLogSource Logger;
        private readonly Harmony _harmony = new("blackmoss.randomtp");
        private static Plugin Instance { get; set; } = null!;
        private bool _isRandomTpLoopRunning;    // 倒计时状态
        private float _tpCountdown;             // 传送倒计时
        
        // ReSharper disable once InconsistentNaming
        private static ConfigEntry<float> configTpCountdown;    // 倒计时配置
        // ReSharper disable once InconsistentNaming
        private static ConfigEntry<bool> configTipStyle;        // 提示样式配置

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            _harmony.PatchAll();
            
            configTpCountdown = Config.Bind(
                "General",
                "TpCountdown",
                60f     // 默认倒计时60秒
            );
            configTipStyle = Config.Bind(
                "General",
                "TipStyle",
                true,   // true: 居中, false: 底端
                "true: Center of the screen, false: Bottom of the screen"
            );
            
            // 防止开局传送
            _tpCountdown = configTpCountdown.Value + 3f;
        }
        
        [HarmonyPatch(typeof(PlayerCamera), "Awake")]
        public class PlayerCameraAwakePatch
        {
            // ReSharper disable once UnusedMember.Global
            public static void Postfix()
            {
                if (Instance != null)
                {
                    Instance.StartRandomTpLoop();
                }
            }
        }
        
        public void StartRandomTpLoop()
        {
            if (_isRandomTpLoopRunning) return;
            _isRandomTpLoopRunning = true;
            StartCoroutine(RandomTpLoop());
        }
        
        public void StopRandomTpLoop()
        {
            _isRandomTpLoopRunning = false;
        }
        
        public System.Collections.IEnumerator RandomTpLoop()
        {
            while (_isRandomTpLoopRunning)
            {
                yield return new WaitForSeconds(1);
                RandomTp();
            }
        }
        
        public void RandomTp()
        {
            // 提前递减倒计时，避免初始值为0时的逻辑错误
            _tpCountdown--;

            // 生成随机坐标
            var vector = new Vector2(Random.Range(-512, 513), Random.Range(-512, 513));

            // 倒计时提示逻辑统一处理
            HandleCountdownAlert();

            // 执行传送逻辑
            if (_tpCountdown <= 0)
            {
                ResetTpState(vector);
            }
        }

        private void HandleCountdownAlert()
        {
            // 统一处理倒计时提示
            if (_tpCountdown is > 0 and < 11)
            {
                AlertText($"Random TP countdown: {_tpCountdown}", configTipStyle.Value);
            }
            else if (Mathf.Approximately(_tpCountdown, 30) || Mathf.Approximately(_tpCountdown, 60))
            {
                AlertText($"Random TP countdown: {_tpCountdown}", configTipStyle.Value);
            }
        }

        private void ResetTpState(Vector2 targetPosition)
        {
            // 重置倒计时和状态
            _tpCountdown = configTpCountdown.Value;
            _isRandomTpLoopRunning = true;
            
            PlayerCamera.main.body.transform.position = targetPosition;
            PlayerCamera.main.transform.position = targetPosition;
        }
        
        public void AlertText(string text, bool important)
        {
            PlayerCamera.main.DoAlert(text, important);
        }
    }
}
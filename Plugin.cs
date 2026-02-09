using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomTP
{
    [BepInPlugin("com.blackmoss.randomtp", "Random TP", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // ReSharper disable once MemberCanBePrivate.Global
        internal new static ManualLogSource Logger;
        private readonly Harmony _harmony = new("com.blackmoss.randomtp");
        private static Plugin Instance { get; set; } = null!;
        private bool _isRandomTpLoopRunning;
        private int _tpCountdown = 12;
        
        // ReSharper disable once InconsistentNaming
        private static ConfigEntry<int> configTpCountdown;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            _harmony.PatchAll();
            
            configTpCountdown = Config.Bind(
                "General",
                "TpCountdown",
                12
            );
        }
        
        [HarmonyPatch(typeof(PlayerCamera), "Awake")]
        public class PlayerCameraAwakePatch
        {
            public static void Postfix()
            {
                if (Instance != null)
                {
                    Instance.StartRandomTpLoop(13);
                    // configTpCountdown.Value);
                }
            }
        }
        
        public void StartRandomTpLoop(float interval)
        {
            if (_isRandomTpLoopRunning) return;
            _isRandomTpLoopRunning = true;
            StartCoroutine(RandomTpLoop(interval));
            StartCoroutine(TpTipsLoop(1));
        }
        
        public void StopRandomTpLoop()
        {
            _isRandomTpLoopRunning = false;
        }
        
        public System.Collections.IEnumerator RandomTpLoop(float interval)
        {
            while (_isRandomTpLoopRunning)
            {
                yield return new WaitForSeconds(interval);
                RandomTp();
            }
        }
        
        public System.Collections.IEnumerator TpTipsLoop(float interval)
        {
            while (_isRandomTpLoopRunning)
            {
                yield return new WaitForSeconds(interval);
                TpTips();
            }
        }
        
        public void RandomTp()
        {
            var vector = new Vector2(Random.Range(-512, 512), Random.Range(-512, 512));
            PlayerCamera.main.body.transform.position = vector; 
            PlayerCamera.main.transform.position = vector;
        }
        
        public void TpTips()
        {
            if (_tpCountdown != 1)
            {
                AlertText($"Random TP countdown: {_tpCountdown}");
            }
            else
            {
                AlertText($"Random TP countdown: 1");
                _tpCountdown = 12;
            }
            
            _tpCountdown--;
        }
        
        public void AlertText(string text, bool important = true)
        {
            PlayerCamera.main.DoAlert(text, important);
        }
    }
}
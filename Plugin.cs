using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomTP
{
    [BepInPlugin("blackmoss.randomtp", "RandomTP", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        private readonly Harmony _harmony = new("blackmoss.randomtp");
        private static Plugin Instance { get; set; } = null!;

        private bool _isRandomTpLoopRunning;
        private int _tpCountdown = configTpCountdown.Value;
        
        // ReSharper disable once InconsistentNaming
        static ConfigEntry<int> configTpCountdown;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            _harmony.PatchAll();
            
            configTpCountdown = Config.Bind(
                "General",
                "ConfigTpCountdown",
                60,
                "Default 1 minute."
            );
        }
        
        [HarmonyPatch(typeof(PlayerCamera), "Awake")]
        public class PlayerCameraAwakePatch
        {
            public static void Postfix()
            {
                if (Instance != null)
                {
                    Instance.StartRandomTpLoop(configTpCountdown.Value);
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
            switch (_tpCountdown)
            {
                case 10:
                    AlertText("Random TP countdown: 10");
                    break;
                case <= 3 and > 0:
                    AlertText($"Random TP countdown: {_tpCountdown}");
                    _tpCountdown--;
                    break;
                case <= 1:
                    _tpCountdown = configTpCountdown.Value - 1;
                    break;
                default:
                    _tpCountdown--;
                    break;
            }
        }
        
        public void AlertText(string text, bool important = true)
        {
            PlayerCamera.main.DoAlert(text, important);
        }
    }
}
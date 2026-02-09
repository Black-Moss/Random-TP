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
        private float _tpCountdown;
        
        // ReSharper disable once InconsistentNaming
        private static ConfigEntry<float> configTpCountdown;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            _harmony.PatchAll();
            
            configTpCountdown = Config.Bind(
                "General",
                "TpCountdown",
                12f
            );
            
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
            var vector = new Vector2(Random.Range(-512, 512), Random.Range(-512, 512));
            
            if (_tpCountdown is > 0 and < 11)
            {
                AlertText($"Random TP countdown: {_tpCountdown}");
            }
            if (_tpCountdown == 0)
            {
                _tpCountdown = configTpCountdown.Value;
                _isRandomTpLoopRunning = true;
                PlayerCamera.main.body.transform.position = vector;
                PlayerCamera.main.transform.position = vector;
            }

            _tpCountdown--;
        }
        
        public void AlertText(string text, bool important = true)
        {
            PlayerCamera.main.DoAlert(text, important);
        }
    }
}
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using MossLib.Base;
using UnityEngine;

namespace RandomTP;

public class ModCommand : ModCommandBase
{
    private static ModCommand Instance { get; set; } = new();
    
    private static ModCommand _instance;
    
    public static void Initialize(ManualLogSource logger)
    { 
        if (_instance != null) return;
        _instance = new ModCommand();
        Instance = _instance;
        _instance.Initialize(logger, Plugin.Guid, Plugin.Name, Assembly.GetExecutingAssembly());
    }
    
    [HarmonyPatch(typeof(ConsoleScript), "RegisterAllCommands")]
    public class ConsoleScriptRegisterAllCommandsPatcher
    {
        [HarmonyPostfix]
        // ReSharper disable once UnusedMember.Global
        public static void RandomTpCommands()
        {
            ConsoleScript.Commands.Add(new Command(
                "randomtp", 
                ModLocale.GetFormat("command.randomtp.name"),
                args =>
                {
                    Instance.CheckForWorld();
                    
                    Plugin.Tp();
                    Plugin.BodyPatch.Reset();
                    
                    Instance.LogToConsole(ModLocale.GetFormat("command.randomtp.message"));
                },
                null
            ));
        }
    }
    
    [HarmonyPatch(typeof(ConsoleScript), "Awake")]
    public new class ConsoleScriptAwakePatcher
    {
        [HarmonyPostfix]
        // ReSharper disable once UnusedMember.Global
        public static void AddCustomLogCallback()
        {
            Application.logMessageReceived += Instance.ApplicationLogCallback;
        }
    }
}
using UnityEngine;

namespace CodeNifty.DraftYourViewers2
{
    public class Logger
    {
        public static void Log(string message) => Debug.Log($"[MOD | Draft Your  Viewers 2] {message}");
        public static void LogInfo(string message) => Log($"[INFO] | {message}");
        public static void LogWarn(string message) => Log($"[WARN] | {message}");
        public static void LogError(string message) => Debug.LogError($"Draft Twitch Viewers : [ERR ] | {message}");
    }
}

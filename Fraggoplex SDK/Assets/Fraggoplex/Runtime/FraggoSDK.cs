using UnityEngine;

namespace Fraggoplex.SDK
{
    public enum FraggoStatus
    {
        Uninitialized,
        Licensed,
        Unlicensed,
        Mock
    }

    public static class FraggoSDK 
    {
        public static FraggoStatus Status { get; private set; } = FraggoStatus.Uninitialized;
        public static string UnlicensedReason { get; private set; }
        public static string Player { get; private set; }
        public static string Seat { get; private set; }

        /// <summary>
        /// It turns on the SDK when a game starts.
        /// </summary>
        public static void Init(FraggoSettings settings)
        {

        }

        public static void LogEvent(string name, double? value = null, object data = null)
        {
            // Implementation coming later
        }

        public static void ShowEndScreen(string steamAppId)
        {
            // Implementation coming later
        }

        // Shutdown SDK
        public static void Shutdown()
        {
            // Implementation coming later
        }
    }
}
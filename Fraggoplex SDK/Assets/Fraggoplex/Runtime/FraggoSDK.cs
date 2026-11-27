using System;
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

        private const string NO_TOKEN = "no_token";
        private const string BAD_TOKEN = "bad_token";
        private const string FRAGGO_MOCK = "FRAGGO_MOCK";

        /// <summary>
        /// It turns on the SDK when a game starts.
        /// </summary>

        public static void Init(FraggoSettings settings)
        {
            try
            {
                if (Status != FraggoStatus.Uninitialized)
                {
                    Debug.LogWarning("[FraggoSDK] Already initialized.");
                    return;
                }

                if (settings == null)
                {
                    SetUnlicensed(BAD_TOKEN);
                    return;
                }

                if (string.IsNullOrWhiteSpace(settings.gameId))
                {
                    SetUnlicensed(BAD_TOKEN);
                    Debug.LogError("[FraggoSDK] Game ID is missing.");
                    return;
                }

                bool mockMode = (Application.isEditor && settings.mockInEditor) || Environment.GetEnvironmentVariable(FRAGGO_MOCK) == "1";

                if (mockMode)
                {
                    InitializeMock();
                    return;
                }

                Debug.Log("[FraggoSDK] Starting real initialization.");

                string[] args = Environment.GetCommandLineArgs();
                LaunchArgs launchArgs = LaunchArgs.Parse(args);

                string token = launchArgs.Token;
                string buildSha = launchArgs.BuildSha256;

                long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                VerifyResult result = TokenVerifier.Verify(token, buildSha, settings.gameId, settings.publicKeys, nowUnix);

                if (!result.Ok)
                {
                    SetUnlicensed(result.Reason);

                    Debug.LogWarning("[FraggoSDK] Unlicensed: " + result.Reason);
                    return;
                }

                Status = FraggoStatus.Licensed;

                UnlicensedReason = null;

                Player = result.Claims.player;
                Seat = result.Claims.seat;

                Debug.Log("[FraggoSDK] Token verified successfully.");
                Debug.Log("[FraggoSDK] Player: " + Player);
                Debug.Log("[FraggoSDK] Seat: " + Seat);
            }

            catch (Exception ex)
            {
                Debug.LogError("[FraggoSDK] Initialization error: " + ex.Message);
                SetUnlicensed(BAD_TOKEN);
            }
        }

        private static void InitializeMock()
        {
            Status = FraggoStatus.Mock;

            Player = "mock0000000000000000";
            Seat = "M1";

            UnlicensedReason = null;

            Debug.Log("[FraggoSDK] Initialized in Mock mode.");
        }

        private static void SetUnlicensed(string reason)
        {
            Status = FraggoStatus.Unlicensed;
            UnlicensedReason = reason;

            Player = null;
            Seat = null;
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
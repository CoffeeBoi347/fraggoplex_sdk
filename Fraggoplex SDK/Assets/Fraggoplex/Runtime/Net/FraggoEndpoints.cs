
namespace Fraggoplex.SDK
{
    public static class FraggoEndpoints
    {
        public const string SDK = "/api/sdk";
        public const string LAUNCHER = "/api/launcher";

        // SDK

        public const string SESSION_START = SDK + "/sessions/start";
        public const string SESSION_EVENTS = SDK + "/sessions/{0}/events";
        public const string SESSION_END = SDK + "/sessions/{0}/end";
        public const string UNLICENSED = SDK + "/unlicensed";

        // LAUNCHER

        public const string PUBLIC_KEY = LAUNCHER + "/public-key";
        public const string PAIR = LAUNCHER + "/pair";
        public const string MANIFEST = LAUNCHER + "/manifest";
        public const string INSTALLS = LAUNCHER + "/installs";
        public const string SEAT_SESSIONS = LAUNCHER + "/seat-sessions";
        public const string SEAT_SESSION_BY_CODE = LAUNCHER + "/seat-sessions/{0}";
    }
}

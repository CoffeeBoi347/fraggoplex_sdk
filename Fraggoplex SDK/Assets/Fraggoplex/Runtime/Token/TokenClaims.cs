namespace Fraggoplex.SDK
{
    [System.Serializable]
    public class TokenClaims
    {
        public int v;
        public string iss;

        public string jti;
        public long iat;

        public long nbf;
        public long exp;

        public string game_id;
        public string build_id;
        public string build_sha256;

        public string event_id;
        public string cafe_id;
        public string pc_id;

        public string seat;
        public string player;
    }
}
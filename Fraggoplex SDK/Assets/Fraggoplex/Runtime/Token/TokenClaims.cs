namespace Fraggoplex.SDK
{
    [System.Serializable]
    public class TokenClaims
    {
        public int v;
        public string iss;

        public string game_id;
        public string build_id;

        public string seat;
        public string player;

        public long nbf;
        public long exp;
    }
}
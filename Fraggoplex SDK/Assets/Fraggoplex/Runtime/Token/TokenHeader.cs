using System;

namespace Fraggoplex.SDK
{
    [Serializable]
    public class TokenHeader
    {
        public string alg;
        public string typ;
        public string kid;
    }
}
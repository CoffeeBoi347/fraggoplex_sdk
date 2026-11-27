
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Fraggoplex.SDK
{
    public struct VerifyResult
    {
        public bool Ok;
        public string Reason;
        public TokenClaims Claims;
    }

    public static class TokenVerifier
    {
        private const string NO_TOKEN = "no_token";
        private const string BAD_TOKEN = "bad_token";
        private const string EXPIRED_TOKEN = "expired_token";

        private const long CLOCK_SKEW = 300;

        private sealed class CachedKey
        {
            public string Modulus;
            public string Exponent;
            public RSA Rsa;
        }

        private static readonly Dictionary<string, CachedKey> keyCache = new Dictionary<string, CachedKey>();

        private static readonly object keyCacheLock = new object();
        public static VerifyResult Verify(
            string token,
            string buildShaArg,
            string gameId,
            IList<KeyEntry> keys,
            long nowUnix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return Fail(NO_TOKEN);

                foreach (char c in token)
                {
                    if (char.IsWhiteSpace(c) || c > 127)
                        return Fail(BAD_TOKEN);
                }

                string[] parts = token.Split('.');

                if (parts.Length != 3)
                    return Fail(BAD_TOKEN);

                if (string.IsNullOrEmpty(parts[0]) ||
                    string.IsNullOrEmpty(parts[1]) ||
                    string.IsNullOrEmpty(parts[2]))
                {
                    return Fail(BAD_TOKEN);
                }

                byte[] headerBytes = Base64Url.Decode(parts[0]);
                byte[] claimsBytes = Base64Url.Decode(parts[1]);
                byte[] signatureBytes = Base64Url.Decode(parts[2]);


                string headerJson =
                    Encoding.UTF8.GetString(headerBytes);

                TokenHeader header =
                    JsonUtility.FromJson<TokenHeader>(headerJson);

                if (header == null ||
                    header.alg != "RS256" ||
                    header.typ != "FGT" ||
                    string.IsNullOrWhiteSpace(header.kid))
                {
                    return Fail(BAD_TOKEN);
                }


                if (keys == null || keys.Count == 0)
                    return Fail(BAD_TOKEN);

                KeyEntry selectedKey = null;

                foreach (KeyEntry key in keys)
                {
                    if (key != null && key.kid == header.kid)
                    {
                        selectedKey = key;
                        break;
                    }
                }

                if (selectedKey == null)
                    return Fail(BAD_TOKEN);


                byte[] data = Encoding.ASCII.GetBytes(
                    parts[0] + "." + parts[1]
                );

                bool signatureValid;

                lock (keyCacheLock)
                {
                    RSA rsa = KeyFromInternal(selectedKey);

                    signatureValid = rsa.VerifyData(
                        data,
                        signatureBytes,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1
                    );
                }

                if (!signatureValid)
                    return Fail(BAD_TOKEN);


                string claimsJson =
                    Encoding.UTF8.GetString(claimsBytes);

                TokenClaims claims =
                    JsonUtility.FromJson<TokenClaims>(claimsJson);

                if (claims == null ||
                    claims.v != 1 ||
                    claims.iss != "fraggoplex")
                {
                    return Fail(BAD_TOKEN);
                }

                if (string.IsNullOrWhiteSpace(claims.game_id) ||
                    string.IsNullOrWhiteSpace(claims.player) ||
                    string.IsNullOrWhiteSpace(claims.seat))
                {
                    return Fail(BAD_TOKEN);
                }

                if (claims.nbf <= 0 ||
                    claims.exp <= 0 ||
                    claims.exp <= claims.nbf)
                {
                    return Fail(BAD_TOKEN);
                }

                if ((decimal)nowUnix + CLOCK_SKEW < claims.nbf ||
                    (decimal)nowUnix - CLOCK_SKEW > claims.exp)
                {
                    return Fail(EXPIRED_TOKEN);
                }

                if (!string.IsNullOrWhiteSpace(buildShaArg))
                {
                    if (string.IsNullOrWhiteSpace(claims.build_sha256) ||
                        !string.Equals(
                            buildShaArg,
                            claims.build_sha256,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return Fail(BAD_TOKEN);
                    }
                }

                if (string.IsNullOrWhiteSpace(gameId) ||
                    claims.game_id != gameId)
                {
                    return Fail(BAD_TOKEN);
                }

                return new VerifyResult
                {
                    Ok = true,
                    Reason = null,
                    Claims = claims
                };
            }
            catch (Exception)
            {
                return Fail(BAD_TOKEN);
            }
        }
        public static RSA KeyFrom(KeyEntry key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key.kid) ||
                string.IsNullOrWhiteSpace(key.n) ||
                string.IsNullOrWhiteSpace(key.e))
            {
                throw new ArgumentException(
                    "Invalid RSA public key."
                );
            }

            lock (keyCacheLock)
            {
                return KeyFromInternal(key);
            }
        }

        private static RSA KeyFromInternal(KeyEntry key)
        {
            if (keyCache.TryGetValue(key.kid, out CachedKey cached))
            {
                if (cached.Modulus != key.n ||
                    cached.Exponent != key.e)
                {
                    throw new CryptographicException(
                        "Key ID reused with different key material."
                    );
                }

                return cached.Rsa;
            }

            RSAParameters parameters = new RSAParameters
            {
                Modulus = Base64Url.Decode(key.n),
                Exponent = Base64Url.Decode(key.e)
            };

            RSA rsa = RSA.Create();

            try
            {
                rsa.ImportParameters(parameters);

                keyCache.Add(key.kid, new CachedKey
                {
                    Modulus = key.n,
                    Exponent = key.e,
                    Rsa = rsa
                });

                return rsa;
            }
            catch
            {
                rsa.Dispose();
                throw;
            }
        }

        public static void ClearKeyCache()
        {
            lock (keyCacheLock)
            {
                foreach (CachedKey cached in keyCache.Values)
                {
                    cached.Rsa.Dispose();
                }

                keyCache.Clear();
            }
        }

        private static VerifyResult Fail(string reason)
        {
            return new VerifyResult
            {
                Ok = false,
                Reason = reason,
                Claims = null
            };
        }
    }
}

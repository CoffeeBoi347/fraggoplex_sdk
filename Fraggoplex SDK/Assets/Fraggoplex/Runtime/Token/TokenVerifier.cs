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

        public static VerifyResult Verify(string token, string buildShaArg, string gameId, IList<KeyEntry> keys, long nowUnix)
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

                if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]) || string.IsNullOrWhiteSpace(parts[2]))
                    return Fail(BAD_TOKEN);

                byte[] headerBytes;
                byte[] claimsBytes;
                byte[] signatureBytes;

                headerBytes = Base64Url.Decode(parts[0]);
                claimsBytes = Base64Url.Decode(parts[1]);
                signatureBytes = Base64Url.Decode(parts[2]);

                // Parse and validate the JWT header

                string headerJson = Encoding.UTF8.GetString(headerBytes);
                TokenHeader header = JsonUtility.FromJson<TokenHeader>(headerJson);

                if (header == null || header.alg != "RS256" || header.typ != "FGT" || string.IsNullOrWhiteSpace(header.kid))
                    return Fail(BAD_TOKEN);

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

                if (selectedKey == null || string.IsNullOrWhiteSpace(selectedKey.n) || string.IsNullOrWhiteSpace(selectedKey.e))
                    return Fail(BAD_TOKEN);

                // Verify RSA Signature

                byte[] data = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]);
                RSAParameters rsaParameters = new RSAParameters
                {
                    Modulus = Base64Url.Decode(selectedKey.n),
                    Exponent = Base64Url.Decode(selectedKey.e)
                };

                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportParameters(rsaParameters);

                    bool signatureValid = rsa.VerifyData(data, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    if (!signatureValid)
                        return Fail(BAD_TOKEN);
                }

                // Verify Payload

                string claimJson = Encoding.UTF8.GetString(claimsBytes);
                TokenClaims claims = JsonUtility.FromJson<TokenClaims>(claimJson);

                if (claims == null || claims.v != 1 || claims.iss != "fraggoplex")
                    return Fail(BAD_TOKEN);

                if ((decimal)nowUnix + 300 < claims.nbf || (decimal)nowUnix - 300 > claims.exp)
                    return Fail(EXPIRED_TOKEN);

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

                if (string.IsNullOrEmpty(gameId) || claims.game_id != gameId)
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
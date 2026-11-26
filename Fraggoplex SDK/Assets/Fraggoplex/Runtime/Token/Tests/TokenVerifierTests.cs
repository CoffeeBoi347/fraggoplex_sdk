using Fraggoplex.SDK;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TokenVerifierTests : MonoBehaviour
{
    [ContextMenu("Run Token Verifier Tests")]
    public static void RunTests()
    {
        int passed = 0;
        int failed = 0;

        void Assert(string testName, bool condition, string message = "")
        {
            if (condition)
            {
                Debug.Log($"<color=green>[PASS]</color> {testName}");
                passed++;
            }
            else
            {
                Debug.LogError($"<color=red>[FAIL]</color> {testName} - {message}");
                failed++;
            }
        }

        // null token

        {
            var result = TokenVerifier.Verify(null, "sha123", "game1", new List<KeyEntry>(), 1000L);
            Assert("Null Token Returns No Token", !result.Ok && result.Reason == "no_token");
        }

        // whitespace token

        {
            var result = TokenVerifier.Verify("   ", "sha123", "game1", new List<KeyEntry>(), 1000L);
            Assert("Whitespace Token Returns No Token", !result.Ok && result.Reason == "no_token");
        }

        // malformed token

        {
            var result = TokenVerifier.Verify("header.payload", "sha123", "game1", new List<KeyEntry>(), 1000L);
            Assert("Malformed Token Returns Bad Token", !result.Ok && result.Reason == "bad_token");
        }

        // empty keys list should fill with bad_token
        {
            var result = TokenVerifier.Verify("header.payload.sig", "sha123", "game1", new List<KeyEntry>(), 1000L);
            Assert("Empty Keys Returns Bad Token", !result.Ok && result.Reason == "bad_token");
        }

        Debug.Log($"=== Test Run Complete. Passed: {passed}, Failed: {failed} ===");
    }

    public static RSA KeyFrom(KeyEntry key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (string.IsNullOrWhiteSpace(key.n) || string.IsNullOrWhiteSpace(key.e))
        {
            throw new ArgumentException("Invalid RSA Public Key.");
        }

        RSAParameters rsaParms = new RSAParameters()
        {
            Modulus = Base64Url.Decode(key.n),
            Exponent  = Base64Url.Decode(key.e)
        };

        RSA rsa = RSA.Create();

        try
        {
            rsa.ImportParameters(rsaParms);
            return rsa;
        }
        catch
        {
            rsa?.Dispose();
            throw;
        }
    }
}
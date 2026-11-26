#if UNITY_EDITOR
using UnityEditor;

public static class TokenVerifierTestsMenu
{
    [MenuItem("Tools/Run Token Verifier Tests")]
    public static void TriggerTests()
    {
        TokenVerifierTests.RunTests();
    }
}
#endif
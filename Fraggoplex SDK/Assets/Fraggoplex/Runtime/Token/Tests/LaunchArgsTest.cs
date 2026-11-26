using Fraggoplex.SDK;
using UnityEngine;

public class LaunchArgsTest : MonoBehaviour
{
    private void Start()
    {
        string[] testArgs =
{
            "MyGame.exe",
            "--fraggo-token",
            "sample_token_123",
            "--fraggo-build-sha256",
            "abc123"
        };

        LaunchArgs launch = LaunchArgs.Parse(testArgs);

        Debug.Log("Token: " + launch.Token);
        Debug.Log("Build SHA: " + launch.BuildSha256);
    }
}

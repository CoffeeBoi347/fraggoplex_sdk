using Fraggoplex.SDK;
using UnityEngine;

public class FraggoTest : MonoBehaviour
{
    [SerializeField] private FraggoSettings settings;

    private void Start()
    {
        FraggoSDK.Init(settings);

        Debug.Log("Status: " + FraggoSDK.Status);
        Debug.Log("Player: " + FraggoSDK.Player);
        Debug.Log("Seat: " + FraggoSDK.Seat);
    }
}

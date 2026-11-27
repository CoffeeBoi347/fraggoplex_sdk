
using System.Collections.Generic;
using UnityEngine;

namespace Fraggoplex.SDK
{
    [CreateAssetMenu(
        fileName = "FraggoSettings",
        menuName = "Fraggoplex/SDK Settings"
    )]
    public class FraggoSettings : ScriptableObject
    {
        [Header("Backend Config")]
        public string apiBaseUrl = "https://fraggaplex-backend.vercel.app";

        [Header("Game Config")]
        public string gameId;
        public string gameSlug;

        [Header("Security")]
        public List<KeyEntry> publicKeys = new List<KeyEntry>();

        [Header("Development")]
        public bool mockInEditor = true;

        [Header("Unlicensed Behaviour")]
        public UnlicensedBehaviour unlicensedBehaviour = UnlicensedBehaviour.Block;
    }

    public enum UnlicensedBehaviour
    {
        Block,
        DemoMode
    }
}

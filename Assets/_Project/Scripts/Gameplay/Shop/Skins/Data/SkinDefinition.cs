using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game/Skins/Skin Definition", fileName = "New Player Skin")]
public class SkinDefinition : ScriptableObject
{
    [Header("Prefab (fallback / editor)")]
    public GameObject Prefab;

    [Header("Prefab (fallback / editor)")]
    public GameObject bikePrefab;

    [Header("Addressables (optional)")]
    public AssetReferenceT<GameObject> PrefabReference;
}
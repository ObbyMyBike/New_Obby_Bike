using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class PlayerSkin : MonoBehaviour
{
    [SerializeField] private Transform _skinContainer;
    [SerializeField] private Transform _bikeContainer;
    [SerializeField] private Animator _animator;

    private AsyncOperationHandle<GameObject>? _handle;
    private bool _skinLoaded;
    
    [Inject] private Player _player;
    [Inject] private PlayerConfig _playerConfig;
    [Inject] private SkinSaver _skinSaver;
    
    private void Start()
    {
        TryLoadSavedSkin();
    }
    
    private void TryLoadSavedSkin()
    {
        if (_skinLoaded)
            return;

        if (_skinSaver == null || _playerConfig == null)
            return;
        
        string savedID = _skinSaver.GetSelected();
        
        if (string.IsNullOrEmpty(savedID))
            return;
        
        var list = _playerConfig.AvailableSkins;
        
        if (list == null || list.Count == 0)
            return;
        
        SkinDefinition skin = list.FirstOrDefault(s => s != null && s.name == savedID);
        
        if (skin == null)
            return;
        
        _ = ApplyCharacterSkinAsync(skin);
        _skinLoaded = true;
    }
    
    public async Task ApplyCharacterSkinAsync(SkinDefinition skin)
    {
        if (_skinContainer == null)
            return;

        if (_skinContainer == null)
            return;
        
        Transform parent = _skinContainer.parent;
        
        if (parent == null)
            return;
        
        
        Destroy(_skinContainer.gameObject);

        if (_handle.HasValue)
        {
            Addressables.Release(_handle.Value);
            _handle = null;
        }

        GameObject prefab = skin.Prefab;
        
        if (prefab == null && skin.PrefabReference.RuntimeKeyIsValid())
        {
            _handle = skin.PrefabReference.LoadAssetAsync<GameObject>();
            prefab = await _handle.Value.Task;
        }

        if (prefab == null)
            return;

        int created = 0;
        
        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            Transform child = prefab.transform.GetChild(i);
            
            if (child.GetComponentInChildren<Canvas>() != null)
                continue;

            GameObject instance = Instantiate(child.gameObject, parent, true);
            instance.name = child.name;

            RenameRecursively(instance.transform);

            instance.transform.localPosition = child.localPosition;
            instance.transform.localRotation = child.localRotation;
            instance.transform.localScale = child.localScale;

            if (i == 0)
                _skinContainer = instance.transform;

            created++;
        }

        StartCoroutine(UpdateAnimatorCoroutine());
        
        BikeRiderDetacher detacher = _player.PlayerCharacterRoot.GetComponent<BikeRiderDetacher>();
        
        if (detacher != null)
            detacher.OverrideRoots(_skinContainer, _bikeContainer);
    }

    private IEnumerator UpdateAnimatorCoroutine()
    {
        if (_animator != null)
        {
            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
            
            _animator.enabled = false;
            
            yield return new WaitForSeconds(0.01f);
            
            _animator.enabled = true;

            if (_animator.runtimeAnimatorController == null)
                _animator.runtimeAnimatorController = controller;

            if (_player != null && _player.PlayerCharacterRoot != null)
                _player.PlayerCharacterRoot.SetAnimator(_animator);
        }
    }

    private void RenameRecursively(Transform transform)
    {
        foreach (Transform c in transform)
            RenameRecursively(c);
    }
}
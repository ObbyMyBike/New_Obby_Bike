using UnityEngine;
using Zenject;

public class NameplateService
{
    private readonly NameplateView _prefab;
    private readonly Vector3 _offset;
    private readonly DiContainer _container;

    public NameplateService(NameplateView prefab, Vector3 offset, DiContainer container)
    {
        _prefab = prefab;
        _offset = offset;
        _container = container;
    }

    public void Attach(GameObject target, string name)
    {
        if (!_prefab || !target)
            return;
        
        NameplateView view = _container.InstantiatePrefabForComponent<NameplateView>(_prefab.gameObject, target.transform);

        view.transform.localPosition = _offset;
        view.transform.localRotation = Quaternion.identity;
        view.transform.localScale = Vector3.one;

        view.SetText(name);
    }
}
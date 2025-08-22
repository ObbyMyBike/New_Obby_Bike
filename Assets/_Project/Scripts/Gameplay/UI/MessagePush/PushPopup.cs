using UnityEngine;
using Zenject;

public class PushPopup
{
    private readonly PushPopupView _prefab;
    private readonly Vector3 _offset;
    private readonly DiContainer _container;

    public PushPopup(PushPopupView prefab, Vector3 offset, DiContainer container)
    {
        _prefab = prefab;
        _offset = offset;
        _container = container;
    }

    public void ShowOver(Transform target, string message)
    {
        if (!_prefab || target == null)
            return;

        PushPopupView view = _container.InstantiatePrefabForComponent<PushPopupView>(_prefab.gameObject, target);
        
        view.transform.localPosition = _offset;
        view.transform.localRotation = Quaternion.identity;
        view.transform.localScale = Vector3.one;

        view.Play(message);
    }
}
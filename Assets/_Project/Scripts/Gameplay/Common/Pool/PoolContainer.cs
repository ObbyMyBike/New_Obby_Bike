using UnityEngine;

public static class PoolContainer
{
    private const string ROOT_NAME = "GlobalPoolContainer";
    
    private static Transform _root;

    public static Transform Root
    {
        get
        {
            if (_root == null)
            {
                var existing = GameObject.Find(ROOT_NAME);
                if (existing != null)
                {
                    _root = existing.transform;
                }
                else
                {
                    var go = new GameObject(ROOT_NAME);
                    _root = go.transform;
                }
            }

            return _root;
        }
    }
}
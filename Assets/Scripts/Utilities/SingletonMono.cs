using System;
using UnityEngine;


public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Inst
    {
        get
        {
            if (_inst is null)
            {
                var go = new GameObject(typeof(T).Name);
                _inst = go.AddComponent<T>();
                return _inst;
            }

            return _inst;
        }
    }

    static T _inst;

    private void Awake()
    {
        if(_inst !=null )
        {
            Destroy(gameObject);
        }
        else
        {
            _inst = this as T;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singletone<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    private bool _isInitialized = false;

    protected static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<T>();

            if (_instance == null)
            {
                var holder = new GameObject(typeof(T).Name);
                holder.AddComponent<T>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            if (_instance is Singletone<T> singletone && !singletone._isInitialized)
                singletone.InternalInitialize();

            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _instance = null;
    }

    protected void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        if (_instance == null)
            _instance = this as T;

        if (_instance is Singletone<T> singletone && !singletone._isInitialized)
            singletone.InternalInitialize();
    }

    private void InternalInitialize()
    {
        _isInitialized = true;
        Initialize();
    }

    protected abstract void Initialize();
}

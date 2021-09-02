using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    public class Singleton<T> : RFBMonoBehaviour where T : MonoBehaviour
    {
        // Static reference
        public static T instance
        {
            get
            {
                if (_instance == null && Application.isPlaying)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        string iName = typeof(T).ToString() + "_SINGLETON";
                        _instance = new GameObject(iName).AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
        private static T _instance;

        // On Awake, Add Ref
        protected virtual void Awake()
        {
            _instance = GetComponent<T>();
        }

        // On Destroy, Remove Ref
        protected virtual void OnDestroy()
        {
            if (_instance == GetComponent<T>())
            {
                _instance = null;
            }
        }
    }
}

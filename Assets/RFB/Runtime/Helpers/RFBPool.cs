using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    public class RFBPool : Singleton<RFBPool>
    {
        // Prefabs
#if UNITY_EDITOR
        [SerializeField]
#endif
        private List<GameObject> _prefabs = new List<GameObject>();

        // Instance items
        private List<GameObject> _instances = new List<GameObject>();
        private List<int> _instancePrefabs = new List<int>();
        private Dictionary<int, List<int>> _available = new Dictionary<int, List<int>>();

        // On awake, deactivate
        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(false);
        }

        // Get prefab index from prefab
        public int GetPrefabIndex(GameObject prefab)
        {
            // Ensure prefab exists
            if (prefab == null)
            {
                Log("Cannot add null prefab", LogType.Error);
                return -1;
            }

            // Find index
            int index = _prefabs.IndexOf(prefab);

            // Add to prefab list
            if (index == -1)
            {
                index = _prefabs.Count;
                _prefabs.Add(prefab);
            }

            // Return index
            return index;
        }

        // Get prefab index from prefab id
        public int GetPrefabIndex(string prefabID)
        {
            // Ensure prefab id is not null
            if (string.IsNullOrEmpty(prefabID))
            {
                Log("Cannot find null prefab id", LogType.Error);
                return -1;
            }

            // Find index
            int index = -1;
            for (int p = 0; p < _prefabs.Count; p++)
            {
                GameObject prefab = _prefabs[p];
                if (string.Compare(prefab.name, prefabID, true) == 0)
                {
                    index = p;
                    break;
                }
            }
            if (index == -1)
            {
                Log("Cannot find prefab\nID: " + prefabID, LogType.Warning);
            }

            // Return index
            return index;
        }

        // Unload an instance
        public void Unload(GameObject inst)
        {
            // Get index
            int index = _instances.IndexOf(inst);
            if (index == -1)
            {
                Log("Cannot unload instance\nInstance: " + (inst == null ? "null" : inst.name), LogType.Error);
                return;
            }

            // Add to available list
            int prefabIndex = _instancePrefabs[index];
            List<int> available = _available.ContainsKey(prefabIndex) ? _available[prefabIndex] : new List<int>();
            available.Add(index);
            _available[prefabIndex] = available;

            // Add into transform
            inst.transform.SetParent(transform);
        }

        // Load instance with an actual prefab
        public GameObject Load(GameObject prefab, bool instanceMaterials = false)
        {
            return Load(GetPrefabIndex(prefab), instanceMaterials);
        }

        // Load instance with a prefab id
        public GameObject Load(string prefabID, bool instanceMaterials = false)
        {
            return Load(GetPrefabIndex(prefabID), instanceMaterials);
        }

        // Load an instance with a prefab index
        public GameObject Load(int prefabIndex, bool instanceMaterials = false)
        {
            // Check prefab index
            if (prefabIndex < 0 || prefabIndex >= _prefabs.Count || _prefabs[prefabIndex] == null)
            {
                Log("Cannot load invalid index\nIndex: " + prefabIndex, LogType.Error);
                return null;
            }

            // The instance to return
            GameObject inst = null;

            // Look for available indices
            if (_available.ContainsKey(prefabIndex))
            {
                List<int> available = _available[prefabIndex];
                if (available.Count > 0)
                {
                    int index = available[0];
                    inst = _instances[index];
                    available.RemoveAt(0);
                    _available[prefabIndex] = available;
                }
            }

            // Not found, instantiate
            if (inst == null)
            {
                // Instantiate
                GameObject prefab = _prefabs[prefabIndex];
                inst = Instantiate<GameObject>(prefab);
                inst.name = prefab.name;
                inst.transform.SetParent(transform);
                _instances.Add(inst);
                _instancePrefabs.Add(prefabIndex);

                // Instantiate materials
                if (instanceMaterials)
                {
                    Renderer[] renderers = inst.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer rend in renderers)
                    {
                        Material[] sharedMats = rend.sharedMaterials;
                        for (int i = 0; i < sharedMats.Length; i++)
                        {
                            sharedMats[i] = new Material(sharedMats[i]);
                        }
                        rend.sharedMaterials = sharedMats;
                    }
                }
            }

            // Remove parent
            inst.transform.SetParent(null);

            // Return loaded instance
            return inst;
        }

        // Preload
        public void Preload(int count, GameObject prefab)
        {
            Preload(count, GetPrefabIndex(prefab));
        }

        // Preload
        public void Preload(int count, string prefabID)
        {
            Preload(count, GetPrefabIndex(prefabID));
        }

        // Preload
        public void Preload(int count, int prefabIndex)
        {
            // Check prefab index
            if (prefabIndex < 0 || prefabIndex >= _prefabs.Count || _prefabs[prefabIndex] == null)
            {
                Log("Cannot preload invalid index\nIndex: " + prefabIndex, LogType.Error);
                return;
            }

            // Load
            List<GameObject> insts = new List<GameObject>();
            for (int c = 0; c < count; c++)
            {
                insts.Add(Load(prefabIndex));
            }

            // Unload
            for (int c = 0; c < count; c++)
            {
                Unload(insts[c]);
            }
        }
    }
}

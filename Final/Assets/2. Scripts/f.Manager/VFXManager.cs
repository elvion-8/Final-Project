using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    private static VFXManager _instance;
    public static VFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if (go != null)
                {
                    _instance = go.GetComponent<VFXManager>();
                    if (_instance == null)
                    {
                        _instance = go.AddComponent<VFXManager>();
                    }
                }
                else
                {
                    go = new GameObject { name = "@VFXManager" };
                    _instance = go.AddComponent<VFXManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private Dictionary<int, Queue<GameObject>> _poolDict = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, int> _instanceToPrefabMap = new Dictionary<int, int>();

    public void Init()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public GameObject PlayVFX(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, float? customDuration = null)
    {
        if (prefab == null) return null;

        int prefabId = prefab.GetInstanceID();
        if (!_poolDict.ContainsKey(prefabId))
        {
            _poolDict[prefabId] = new Queue<GameObject>();
        }

        GameObject obj = null;
        Queue<GameObject> queue = _poolDict[prefabId];

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            if (item != null)
            {
                obj = item;
                break;
            }
        }

        if (obj == null)
        {
            obj = Instantiate(prefab, position, rotation, parent);
            _instanceToPrefabMap[obj.GetInstanceID()] = prefabId;
        }
        else
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent);
            obj.SetActive(true);
        }

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear();
            ps.Play(true);
        }

        ParticleSystem[] childPS = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var child in childPS)
        {
            child.Clear();
            child.Play(true);
        }

        float duration = 0f;
        if (customDuration.HasValue)
        {
            duration = customDuration.Value;
        }
        else if (ps != null)
        {
            var main = ps.main;
            if (!main.loop)
            {
                duration = main.duration + main.startDelay.constantMax;
                
                foreach (var child in childPS)
                {
                    var childMain = child.main;
                    if (!childMain.loop)
                    {
                        float childDuration = childMain.duration + childMain.startDelay.constantMax;
                        if (childDuration > duration)
                        {
                            duration = childDuration;
                        }
                    }
                }
            }
            else
            {
                duration = -1f;
            }
        }
        else
        {
            duration = 2.0f;
        }

        if (duration > 0f)
        {
            StartCoroutine(CoAutoReturn(obj, duration));
        }

        return obj;
    }

    public void ReturnVFX(GameObject obj)
    {
        if (obj == null) return;

        int instanceId = obj.GetInstanceID();
        if (_instanceToPrefabMap.TryGetValue(instanceId, out int prefabId))
        {
            if (_poolDict.ContainsKey(prefabId) && !_poolDict[prefabId].Contains(obj))
            {
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                _poolDict[prefabId].Enqueue(obj);
            }
        }
        else
        {
            Destroy(obj);
        }
    }

    private IEnumerator CoAutoReturn(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && obj.activeSelf)
        {
            ReturnVFX(obj);
        }
    }
}

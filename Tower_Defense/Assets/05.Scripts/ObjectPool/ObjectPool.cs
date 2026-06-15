using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolObjectData
{
    public GameObject prefab;

    [Tooltip("poolCount는 1개 이상 설정")]
    public int poolCount = 10;

    [Header("해당 오브젝트 설명")]
    public string explan;
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] private bool dontDestroy = true;
    [SerializeField] private List<PoolObjectData> poolList = new List<PoolObjectData>();

    //풀링 부모
    private Dictionary<string, Transform> poolParentDic = new Dictionary<string, Transform>();
    //풀링 생성 오브젝트
    private Dictionary<string, PoolObjectData> instantiateObject = new Dictionary<string, PoolObjectData>();

    private void Awake()
    {
        if (instance == null) {

            instance = this;

            if (dontDestroy) 
                DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        MakeDir("Using");
        Pool(poolList);
    }

    #region Instantiate
    public void Pool(List<PoolObjectData> poolObjList)
    {
        GameObject prefab;

        string prefabName;
        int count;

        PoolObjectData data;
        for (int i = 0; i < poolList.Count; i++) {
            data = poolObjList[i];

            prefab = data.prefab;
            prefabName = prefab.name;
            count = data.poolCount;

            if (prefab == null) {
                Debug.LogError("프리팹 등록이 안됨!");
                continue;
            }

            //등록이 안됬다면
            if (!poolParentDic.ContainsKey(prefabName)) {
                MakeDir(prefabName);
                InstantiatePool(prefab, poolParentDic[prefabName], count);
                instantiateObject[prefabName] = data;
            }
            else
                continue;
        }
    }

    /// <summary> 옵젝 반환 or 생성할, 부모를 생성 </summary>
    GameObject MakeDir(string name)
    {
        GameObject newDir = new GameObject(name);
        newDir.transform.SetParent(this.transform);
        poolParentDic[name] = newDir.transform;
        return newDir;
    }

    /// <summary> 옵젝을 갯수만큼 생성 및 마지막에 생성된 옵젝 반환 </summary>
    GameObject InstantiatePool(GameObject prefab, Transform parent, int count)
    {
        if (count <= 0) {
            Debug.LogError($"오브젝트 풀 : Count Setting Error {prefab.name}");
            return null;
        }

        GameObject instance = null;
        for (int i = 0; i < count; i++) {
            instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            instance.name = prefab.name;
            instance.SetActive(false);
        }

        return instance;
    }
    #endregion

    #region Get
    public GameObject GetObj(string id, Transform parent = null, bool enable = true)
    {
        if (!poolParentDic.ContainsKey(id)) {
            Debug.LogError($"오브젝트 풀 : not found object name {id}");
            return null;
        }

        for (int i = 0;  i < poolParentDic[id].childCount; i++) {
            Transform pool = poolParentDic[id].GetChild(i);

            if (pool.gameObject.activeSelf == true)
                continue;

            if (parent == null)
                pool.SetParent(poolParentDic[id]);
            else
                pool.SetParent(parent);

            pool.gameObject.SetActive(enable);

            return pool.gameObject;
        }

        //부족하면 하나 생성 후 반환
        return InstantiatePool(instantiateObject[id].prefab, poolParentDic[id], instantiateObject[id].poolCount);
    }

    public GameObject GetObj(string id, Vector3 spawn, Transform parent = null, bool enable = true)
    {
        if (poolParentDic.ContainsKey(id) == false) {
            Debug.LogErrorFormat("오브젝트 풀 : not found object name {0}", id);
            return null;
        }

        for (int i = 0; i < poolParentDic[id].childCount; i++) {
            Transform pool = poolParentDic[id].GetChild(i);

            if (pool.gameObject.activeSelf == true)
                continue;

            if (parent == null) 
                pool.SetParent(poolParentDic[id]);
            else 
                pool.SetParent(parent);

            pool.gameObject.SetActive(enable);
            pool.transform.localPosition = spawn;
            return pool.gameObject;
        }

        // 부족한 경우 추가 생성 후 하나를 반환해 줍니다.
        return InstantiatePool(instantiateObject[id].prefab, poolParentDic[id], instantiateObject[id].poolCount);
    }
    #endregion

    #region Return
    public void ReturnObj(GameObject _object)
    {
        if (_object == null) {
            Debug.LogError("오브젝트 풀 : return object is null");
            return;
        }

        if (!poolParentDic.ContainsKey(_object.name)) {
            Debug.LogError($" 오브젝트 풀 : not found object name is {_object.name}");
            return;
        }

        _object.transform.SetParent(poolParentDic[_object.name]);
        _object.transform.localPosition = Vector3.zero;
        _object.SetActive(false);
    }

    /// <summary> 프리팹의 이름과 해당 프리팹의 상위 부모의 이름이 다를 경우 해당 ReturnObject 함수 사용 </summary>
    public void ReturnObj(GameObject _object, string _id)
    {
        if (_object == null) {
            Debug.LogError("오브젝트 풀 : return object is null");
            return;
        }

        if (!poolParentDic.ContainsKey(_id)) {
            Debug.LogError($" 오브젝트 풀 : not found object name is {_id} ");
            return;
        }

        _object.transform.SetParent(poolParentDic[_id]);
        _object.transform.localPosition = Vector3.zero;
        _object.SetActive(false);
    }
    #endregion
}


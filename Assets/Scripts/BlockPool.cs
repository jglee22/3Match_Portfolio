using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>블록 Instantiate/Destroy 대신 재사용</summary>
public class BlockPool : MonoBehaviour
{
    [SerializeField] GameObject blockPrefab;
    [SerializeField] Transform poolRoot;
    [SerializeField] int prewarmCount = 32;

    readonly Queue<GameObject> available = new Queue<GameObject>();

    void Awake()
    {
        if (poolRoot == null)
            poolRoot = transform;

        for (int i = 0; i < prewarmCount; i++)
            available.Enqueue(CreateInstance());
    }

    GameObject CreateInstance()
    {
        var obj = Instantiate(blockPrefab, poolRoot);
        obj.SetActive(false);
        return obj;
    }

    public GameObject Get(Vector3 position, Transform parent)
    {
        GameObject obj = available.Count > 0 ? available.Dequeue() : CreateInstance();
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj.SetActive(true);

        var block = obj.GetComponent<Block>();
        if (block != null)
        {
            block.isSpecial = false;
            block.isRowClear = false;
        }

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOKill();
            var c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        return obj;
    }

    public void Release(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(poolRoot);
        available.Enqueue(obj);
    }
}

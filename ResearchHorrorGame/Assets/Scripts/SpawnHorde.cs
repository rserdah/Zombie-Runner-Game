using System.Collections.Generic;
using UnityEngine;

public class SpawnHorde : MonoBehaviour
{
    public GameObject prefab;
    public Bounds bounds;


    private List<GameObject> spawns = new List<GameObject>();

    /// <summary>
    /// How many prefabs should the spawner attempt to spawn. It can fail to spawn the target count if the bounds are too small and/or the spawnMargin is too large (because it
    /// will exceed the maxSpawnAttempts for many spawns).
    /// </summary>
    public int targetSpawnCount = 5;

    /// <summary>
    /// If the current spawn is too close to other prefabs, then how many times should the spawner try to relocate the prefab instance before destroying it and moving on?
    /// Increase this number to increase chances of reaching the targetSpawnCount, but decrease this number to speed up total time taken to spawn all prefabs.
    /// </summary>
    public int maxSpawnAttempts = 10;

    /// <summary>
    /// The minimum distance between each spawn. Typically used to prevent spawns from intersecting with each other on spawn.
    /// </summary>
    public float spawnMargin = 1f;

    private bool init;
    private Vector3[] initPos;


    private void Start()
    {
        GameObject g;
        Vector3 v = Vector3.zero;
        int attempts;

        for(int i = 0; i < targetSpawnCount; i++)
        {
            attempts = 0;

            do
            {
                if(attempts >= maxSpawnAttempts)
                    continue;

                v = RandomPoint();

                attempts++;
            }
            while(!SafeDistance(v));

            g = Instantiate(prefab);
            g.transform.position = v;

            spawns.Add(g);
        }

        InitPos();
    }

    private Vector3 RandomPoint()
    {
        Vector3 point = Vector3.zero;
        Vector3 center = transform.position + bounds.center;

        point.x = Random.Range(center.x - bounds.extents.x / 2f, center.x + bounds.extents.x / 2f);
        point.y = Random.Range(center.y - bounds.extents.y / 2f, center.y + bounds.extents.y / 2f);
        point.z = Random.Range(center.z - bounds.extents.z / 2f, center.z + bounds.extents.z / 2f);

        return point;
    }

    private bool SafeDistance(Vector3 point)
    {
        if(spawns.Count > 0)
            foreach(GameObject g in spawns)
                if((g.transform.position - point).sqrMagnitude < spawnMargin * spawnMargin)
                    return false;

        return true;
    }

    private void InitPos()
    {
        initPos = new Vector3[spawns.Count];

        for(int i = 0; i < initPos.Length; i++)
            initPos[i] = spawns[i].transform.position;

        init = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + bounds.center, bounds.extents);

        Gizmos.color = Color.blue;
        if(initPos.Length > 0)
            foreach(Vector3 v in initPos)
                Gizmos.DrawWireSphere(v, spawnMargin);
    }
}

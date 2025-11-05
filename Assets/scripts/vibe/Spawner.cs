using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns prefabs at a random position inside a rectangular area every random interval (minInterval..maxInterval)
/// Provides gizmos to edit the spawn area and preview spawn points in the editor.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public Transform parentForSpawns;
    public float minInterval = 2f;
    public float maxInterval = 6f;

    [Header("Area (center relative to this object)")]
    public Vector2 areaCenter = Vector2.zero;
    public Vector2 areaSize = new Vector2(10f, 5f);

    [Header("Spawn Options")]
    public bool spawnOnStart = true;
    public bool randomRotation = true;
    public bool localSpace = true; // area center relative to this transform

    private Coroutine spawnRoutine;

    private void Start()
    {
        if (spawnOnStart)
            StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
            SpawnOne();
        }
    }

    public GameObject SpawnOne()
    {
        if (prefabToSpawn == null) return null;

        Vector2 spawnPos = GetRandomPointInArea();
        Vector3 worldPos = localSpace ? transform.TransformPoint((Vector3)spawnPos) : new Vector3(spawnPos.x, spawnPos.y, transform.position.z);

        Quaternion rot = randomRotation ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) : Quaternion.identity;
        GameObject go = Instantiate(prefabToSpawn, worldPos, rot, parentForSpawns);
        return go;
    }

    public Vector2 GetRandomPointInArea()
    {
        float rx = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float ry = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
        Vector2 point = areaCenter + new Vector2(rx, ry);
        return point;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Vector3 center = localSpace ? transform.TransformPoint(areaCenter) : (Vector3)areaCenter + transform.position;
        Vector3 size3 = new Vector3(areaSize.x, areaSize.y, 0.1f);
        Gizmos.DrawCube(center, size3);
        Gizmos.color = new Color(0f, 0.6f, 1f, 1f);
        Gizmos.DrawWireCube(center, size3);

        // draw a few sample points
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 6; i++)
        {
            Vector2 p = GetRandomPointInArea();
            Vector3 wp = localSpace ? transform.TransformPoint(p) : (Vector3)p + transform.position;
            Gizmos.DrawSphere(wp, 0.12f);
        }
    }
}

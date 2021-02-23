using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpperBillboard : MonoBehaviour
{
    [SerializeField]
    private Text remainingDistanceNumber;

    private int cooldownDistance;

    [SerializeField]
    private int minCoolDistance;

    [SerializeField]
    private int maxCoolDistance;

    private int lastSpawnDistance = 0;

    private Vector3 spawnLocation;

    public bool IsReadyToSpawn(int traveledDistance)
    {
        return traveledDistance - lastSpawnDistance > cooldownDistance;
    }

    public void SpawnIfReady(int traveledDistance, float heroZ)
    {
        if (!this.IsReadyToSpawn(traveledDistance))
        {
            return;
        }

        this.lastSpawnDistance = traveledDistance;
        var remainingDistance = Settings.TotalRoadDistanceInKM - traveledDistance;
        var translated = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, spawnLocation.z + heroZ);
        this.gameObject.transform.position = translated;
        cooldownDistance = Random.Range(this.minCoolDistance, this.maxCoolDistance);

        this.OnSpawn(remainingDistance);
    }

    protected virtual void OnSpawn(int remainingDistance)
    {
        this.remainingDistanceNumber.text = remainingDistance.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.spawnLocation = this.gameObject.transform.position;
        cooldownDistance = Random.Range(this.minCoolDistance, this.maxCoolDistance);
    }
}

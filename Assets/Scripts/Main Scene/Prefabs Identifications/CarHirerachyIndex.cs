using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The purpose is passing hirerachy to DOTS while converting each part to a seperate entity and child
/// </summary>
public class CarHirerachyIndex : MonoBehaviour
{
    public int CarID { get; set; }

    public GameObject ParentCar;

    [SerializeField]
    private List<GameObject> wheels;

    [SerializeField]
    public List<int> WheelIndexesInParent;

    private void Awake()
    {
        foreach (var wheel in this.GetAllWheels())
        {
            Debug.LogError(wheel.transform.GetSiblingIndex());
        }
    }

    public void SwitchWheels(bool isOn)
    {
        foreach (var wheel in this.GetAllWheels())
        {
            wheel.SetActive(isOn);
            Debug.LogError(wheel.transform.GetSiblingIndex());
        }
    }
    
    public IEnumerable<GameObject> GetAllWheels()
    {
        return this.wheels;
    }
}
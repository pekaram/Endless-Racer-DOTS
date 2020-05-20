using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose is passing hirerachy to DOTS while converting each part to a seperate entity and child
/// </summary>
public class CarHirerachyIndex : MonoBehaviour
{
    public GameObject ParentCar;

    [SerializeField]
    private List<GameObject> wheels;

    public void SwitchWheels(bool isOn)
    {
        foreach(var wheel in this.GetAllWheels())
        {
            wheel.SetActive(isOn);
        }
    }

    public IEnumerable<GameObject> GetAllWheels()
    {
        return this.wheels;
    }
}